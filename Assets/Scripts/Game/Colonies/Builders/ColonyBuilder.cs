using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AntColony.Game.Colonies.Items;
using AntColony.Game.Colonies.Structures;
using AntColony.Settings;
using Cysharp.Threading.Tasks;
using Omoch;
using Omoch.Geom;
using Omoch.Noises;
using Omoch.Randoms;
using UnityEngine;
using VContainer;

#nullable enable

namespace AntColony.Game.Colonies.Builders
{
    /// <summary>
    /// コロニーの部屋や通路の構成データを非同期で生成する
    /// </summary>
    public class ColonyBuilder
    {
        [Inject] private readonly ColonySetting setting = null!;

        private int chamberIDCounter;

        public ColonyBuilder()
        {
        }

        /// <summary>
        /// 非同期でコロニーマップを生成する
        /// </summary>
        public async UniTask<ColonyData> BuildAsync()
        {
            return await UniTask.RunOnThreadPool(BuildColonyData);
        }

        /// <summary>
        /// コロニーマップの生成
        /// </summary>
        private ColonyData BuildColonyData()
        {
            chamberIDCounter = 0;

            var chambers = new List<Chamber>();

            // セルの初期化
            var colonyWidth = setting.ColonySize.Width;
            var colonyHeight = setting.ColonySize.Height;
            var colonyCells = CreateCells(colonyWidth, colonyHeight);

            var colonyPathway = new ColonyPathway();
            var chamberPlacer = new UniqueTilePlacer(
                1,
                1,
                colonyWidth / ColonyConsts.ChamberGridScale - 2,
                (colonyHeight - ColonyConsts.NoChamberDepth) / ColonyConsts.ChamberGridScale - 2
            );

            // 入口を部屋として作成
            var entrance = CreateEntrance(colonyCells, colonyWidth / 2, colonyHeight - 2);
            chambers.Add(entrance);

            // 女王蟻の部屋を作成
            var chamberSize = new Vector2Int(15, 8);
            var chamberX = (chamberPlacer.GridWidth - chamberSize.x) / 2 + Randomizer.Next(-10, 10);
            if (chamberPlacer.TryAddTileAt(chamberX, 0, chamberSize.x, chamberSize.y, false, out var queenChamberRect))
            {
                var allowedItemKinds = ItemKindMask.Create(ItemKind.Food);
                var chamber = CreateChamber(colonyCells, queenChamberRect, ChamberKind.Queen, true, allowedItemKinds);
                chamber.ID = ChamberID.Queen;
                chambers.Add(chamber);
                // 入口から女王蟻の部屋への通路を作成
                var trunkPathway = new ColonyPathway();
                trunkPathway.Create(
                    new Vector2(entrance.X, entrance.Y),
                    new Vector2(chamber.X, chamber.Y),
                    10f,
                    2,
                    true
                );
                trunkPathway.Clamp(setting.ColonySize, new MarginInt(0, 3, 3, 3));
                colonyPathway.AddPathway(trunkPathway);
            }
            // その他の部屋を重ならないようにランダムに配置し既存の通路のどこかに通路を伸ばす処理を繰り返す
            foreach (var chamberInfo in setting.Chambers)
            {
                var chamberWidth = Randomizer.Next(8, 12);
                var chamberHeight = Randomizer.Next(4, 6);
                if (chamberPlacer.TryAddTile(chamberWidth, chamberHeight, Randomizer.NextFloat(), out var chamberRect))
                {
                    var allowedItemKinds = ItemKindMask.Create(chamberInfo.AllowedItems.ToArray());
                    var chamber = CreateChamber(colonyCells, chamberRect, ChamberKind.Free, false, allowedItemKinds);
                    chambers.Add(chamber);

                    var branchPathway = new ColonyPathway();
                    branchPathway.Create(
                        new Vector2(chamber.X, chamber.Y),
                        colonyPathway.GetRandomPointOnLine(),
                        25f,
                        2,
                        false
                    );
                    branchPathway.Clamp(setting.ColonySize, new MarginInt(0, 3, 3, 3));
                    colonyPathway.AddPathway(branchPathway);
                }
            }

            // 通路→部屋の順で掘る
            DigPathway(colonyCells, colonyPathway);
            foreach (var chamber in chambers)
            {
                DigChamber(chamber);
            }

            // 地表凹凸データ
            float surfaceX = (setting.ColonySize.Width - setting.SurfaceWidth) / 2f;
            var surface = new GroundSurfaceData(surfaceX, setting.SurfaceWidth, setting.SurfaceDepth);

            return new ColonyData(colonyCells, chambers.ToArray(), surface);
        }

        private Cell[,] CreateCells(int width, int height)
        {
            var cells = new Cell[width, height];
            for (var ix = 0; ix < width; ix++)
            {
                for (var iy = 0; iy < height; iy++)
                {
                    cells[ix, iy] = new Cell
                    {
                        X = ix,
                        Y = iy,
                        Kind = CellKind.Dirt,
                        Item = null,
                        IsDug = false,
                        IsDiggable = false,
                        ChamberID = null,
                    };
                }
            }

            // 最短経路検索用に隣接セルを繋ぐ
            for (var ix = 0; ix < width; ix++)
            {
                for (var iy = 0; iy < height; iy++)
                {
                    var cell = cells[ix, iy];
                    var nodes = new List<Node>();
                    for (var dx = -1; dx <= 1; dx++)
                    {
                        for (var dy = -1; dy <= 1; dy++)
                        {
                            var idx = ix + dx;
                            var idy = iy + dy;
                            if ((dx != 0 || dy != 0) && idx >= 0 && idx < width && idy >= 0 && idy < height)
                            {
                                nodes.Add(new Node
                                {
                                    Cost = Mathf.Sqrt(dx * dx + dy * dy),
                                    Link = cells[idx, idy],
                                    Direction = Mathf.Atan2(-dy, -dx),
                                });
                            }
                        }
                    }
                    cell.Nodes = nodes.ToArray();
                }
            }

            return cells;
        }

        public bool TryGetCell(Cell[,] cells, int x, int y, [NotNullWhen(true)] out Cell? cell)
        {
            if ((uint)x < (uint)cells.GetLength(0) && (uint)y < (uint)cells.GetLength(1))
            {
                cell = cells[x, y];
                return true;
            }
            cell = null;
            return false;
        }

        private Chamber CreateEntrance(Cell[,] colonyCells, int x, int y)
        {
            var allowedItemKinds = ItemKindMask.Create(ItemKind.Dirt, ItemKind.Debris, ItemKind.Body);
            var chamber = new Chamber(ChamberID.Entrance, x, y, ChamberKind.Entrance, true, allowedItemKinds, true);
            var cells = new List<Cell>();
            for (int dx = -3; dx <= 3; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (TryGetCell(colonyCells, x + dx, y + dy, out Cell? cell))
                    {
                        cells.Add(cell);
                    }
                }
            }
            chamber.Cells = cells.ToArray();
            TryGetCell(colonyCells, chamber.X, chamber.Y, out Cell? centerCell);
            chamber.LastDugCell = centerCell;

            return chamber;
        }

        private Chamber CreateChamber(Cell[,] colonyCells, TileRect chamberRect, ChamberKind kind, bool isOpened, ItemKindMask allowedItemKinds)
        {
            int chamberX = chamberRect.X * ColonyConsts.ChamberGridScale;
            int chamberY = chamberRect.Y * ColonyConsts.ChamberGridScale;
            int chamberWidth = chamberRect.Width * ColonyConsts.ChamberGridScale;
            int chamberHeight = chamberRect.Height * ColonyConsts.ChamberGridScale;
            int chamberCenterX = chamberX + chamberWidth / 2;
            int chamberCenterY = chamberY + chamberHeight / 2;

            chamberIDCounter++;
            var chamber = new Chamber(new ChamberID(chamberIDCounter), chamberCenterX, chamberCenterY, kind, isOpened, allowedItemKinds, false);
            var chamberPixels = CreateChamberSpace(chamberWidth, chamberHeight, Randomizer.Next(), 8f, 0.15f);
            var cells = new List<Cell>();
            foreach (var pixel in chamberPixels)
            {
                if (TryGetCell(colonyCells, chamberX + pixel.x, chamberY + pixel.y, out var cell))
                {
                    cells.Add(cell);
                }
            }
            chamber.Cells = cells.ToArray();
            TryGetCell(colonyCells, chamber.X, chamber.Y, out Cell? centerCell);
            chamber.LastDugCell = centerCell;

            return chamber;
        }

        private void DigChamber(Chamber chamber)
        {
            foreach (var cell in chamber.Cells)
            {
                cell.ChamberID = chamber.ID;
                cell.IsDiggable = true;
                if (chamber.IsOpened)
                {
                    cell.IsDug = true;
                }
            }
        }

        private void DigPathway(Cell[,] colonyCells, ColonyPathway lines)
        {
            foreach (PathwayLine line in lines.Lines)
            {
                DigLine(
                    colonyCells,
                    (int)line.Start.x,
                    (int)line.Start.y,
                    (int)line.End.x,
                    (int)line.End.y,
                    line.Thisness,
                    line.IsDug
                );
            }
        }

        /// <summary>
        /// 太さのある直線を掘る
        /// </summary>
        private void DigLine(Cell[,] colonyCells, int x0, int y0, int x1, int y1, int thickness, bool dig)
        {
            // ブレゼンハムのアルゴリズムでグリッド上に線を引き、太さ分拡張する
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int signX = x1 > x0 ? 1 : -1;
            int signY = y1 > y0 ? 1 : -1;
            int error = dx - dy;

            do
            {
                DigThickPoint(colonyCells, x0, y0, thickness, dig);

                var error2 = 2 * error;
                if (error2 > -dy)
                {
                    error -= dy;
                    x0 += signX;
                }
                if (error2 < dx)
                {
                    error += dx;
                    y0 += signY;
                }
            }
            while (!(x0 == x1 && y0 == y1));
        }

        /// <summary>
        /// 点を掘る（点を中心とした一辺がthicknessの正方形状に掘る。thicknessが偶数なら中心から右上に1pxずれた形状になる）
        /// </summary>
        private void DigThickPoint(Cell[,] colonyCells, int x, int y, int thickness, bool dig)
        {
            int halfThickness0;
            int halfThickness1;
            if (thickness % 2 == 0)
            {
                halfThickness0 = thickness / 2 - 1;
                halfThickness1 = thickness / 2;
            }
            else
            {
                halfThickness0 = halfThickness1 = (thickness - 1) / 2;
            }

            for (int ix = -halfThickness0; ix <= halfThickness1; ix++)
            {
                for (int iy = -halfThickness0; iy <= halfThickness1; iy++)
                {
                    var px = x + ix;
                    var py = y + iy;
                    if (TryGetCell(colonyCells, px, py, out var cell))
                    {
                        cell.ChamberID = null;
                        cell.IsDiggable = true;
                        if (dig)
                        {
                            cell.IsDug = true;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 部屋の内壁を適度に歪ませつつ辿り着けない空間がないようなデータを作る
        /// </summary>
        private List<Vector2Int> CreateChamberSpace(int width, int height, int randomSeed, float noiseSize, float threshold)
        {
            // パーリンノイズと放射グラデーションをかけて歪んだ空間を作る
            var perlinNoise = new PerlinNoise2(1, 5f, randomSeed);
            var pixels = new bool[width, height];
            for (var ix = 0; ix < width; ix++)
            {
                for (var iy = 0; iy < height; iy++)
                {
                    float px = ix / (float)width * 2 - 1;
                    float py = iy / (float)height * 2 - 1;
                    float alpha = Mathf.Max(1f - Mathf.Sqrt(px * px + py * py), 0f);
                    float noise = perlinNoise.GetNoise(ix / noiseSize, iy / noiseSize);
                    pixels[ix, iy] = alpha * noise > threshold;
                }
            }

            // 生成した空間のうち最も大きい繋がった領域のみを抽出して、辿り着けない空間を除外する
            var labeling = new RegionLabeling(width, height);
            labeling.Execute((x, y) => pixels[x, y]);
            var regions = labeling.GetSortedRegions(false);
            return regions[^1].Positions;
        }
    }
}