using System;
using System.Collections.Generic;
using AntColony.Game.Colonies.Structures;
using Cysharp.Threading.Tasks;

#nullable enable

namespace AntColony.Game.Colonies.Finders
{
    /// <summary>
    /// 全セルの部屋への経路検索をする
    /// </summary>
    public class PathFinder
    {
        private readonly ColonyData data;
        /// <summary>ダイクストラ法で経路検索する際の移動コストを記録</summary>
        private readonly float[,] cellCosts;
        /// <summary>各セルの部屋へ到達するための角度を記録</summary>
        private readonly PathDirection[,] pathDirections;
        /// <summary>経路検索中かどうか</summary>
        private bool isFinding;

        public PathFinder(ColonyData data)
        {
            this.data = data;

            isFinding = false;

            cellCosts = new float[data.Width, data.Height];
            pathDirections = new PathDirection[data.Width, data.Height];
            for (var ix = 0; ix < data.Width; ix++)
            {
                for (var iy = 0; iy < data.Height; iy++)
                {
                    pathDirections[ix, iy] = new PathDirection();
                }
            }
        }

        /// <summary>
        /// 指定座標のセルでの部屋へ向かう方向を取得
        /// </summary>
        public float GetDirection(int x, int y, ChamberID chamberID, PathFindMode findMode)
        {
            if (x >= 0 && x < data.Width && y >= 0 && y < data.Height)
            {
                return findMode switch
                {
                    PathFindMode.Shortest => pathDirections[x, y].Shortest[chamberID],
                    PathFindMode.Detour => pathDirections[x, y].Detour[chamberID],
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                if (y >= data.Height)
                {
                    return -90f;
                }
                else
                {
                    return 0f;
                }
            }
        }

        /// <summary>
        /// 非同期で全部屋への経路を探索する
        /// </summary>
        public async UniTask FindPathAllAsync(PathFindMode mode)
        {
            await UniTask.RunOnThreadPool(() => FindPathAll(mode));
        }

        /// <summary>
        /// 同期処理内で非同期の全部屋経路探索を実行するが、既に実行中なら何もしない
        /// </summary>
        public void FindPathAllSafeAsync(PathFindMode mode)
        {
            if (isFinding)
            {
                return;
            }

            isFinding = true;
            UniTask.RunOnThreadPool(() => FindPathAll(mode));
        }

        public void FindPathAll(PathFindMode mode)
        {
            foreach (Chamber chamber in data.Chambers.Values)
            {
                FindPath(chamber, mode);
            }
            isFinding = false;
        }

        /// <summary>
        /// セルが空洞扱いかどうか
        /// </summary>
        private bool IsSpace(Cell cell, PathFindMode mode)
        {
            return mode switch
            {
                // 迂回モードなら掘った個所のみ空洞扱い
                PathFindMode.Detour => cell.IsDug,
                // 最短モードなら掘れる個所すべて空洞扱い
                PathFindMode.Shortest => cell.IsDiggable,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private void FindPath(Chamber chamber, PathFindMode mode)
        {
            if (chamber.LastDugCell is null)
            {
                return;
            }

            var queue = new Queue<Cell>();

            // 全経路のコストをリセットしておく
            for (int ix = 0; ix < data.Width; ix++)
            {
                for (int iy = 0; iy < data.Height; iy++)
                {
                    cellCosts[ix, iy] = float.MaxValue;
                    var pathDirection = mode switch
                    {
                        PathFindMode.Detour => pathDirections[ix, iy].Detour,
                        PathFindMode.Shortest => pathDirections[ix, iy].Shortest,
                        _ => throw new NotImplementedException(),
                    };
                    pathDirection[chamber.ID] = 0f;
                }
            }

            // 探索開始セルは部屋で最後に掘られたセル
            var startCell = data.Cells[chamber.LastDugCell.X, chamber.LastDugCell.Y];
            cellCosts[chamber.LastDugCell.X, chamber.LastDugCell.Y] = 0f;
            queue.Enqueue(startCell);

            // 対象セルがなくなるまで探索
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                var cellCost = cellCosts[cell.X, cell.Y];
                // 隣接セルを探索
                foreach (Node node in cell.Nodes)
                {
                    var linkCell = node.Link;
                    var linkCellCost = cellCosts[linkCell.X, linkCell.Y];
                    var newCost = cellCost + node.Cost;
                    // 隣接セルが空洞でないor開始地点からのコストが低い値に更新できなければ終了
                    if (!IsSpace(linkCell, mode) || newCost >= linkCellCost)
                    {
                        continue;
                    }

                    cellCosts[linkCell.X, linkCell.Y] = newCost;
                    var pathDirection = mode switch
                    {
                        PathFindMode.Detour => pathDirections[linkCell.X, linkCell.Y].Detour,
                        PathFindMode.Shortest => pathDirections[linkCell.X, linkCell.Y].Shortest,
                        _ => throw new NotImplementedException(),
                    };
                    pathDirection[chamber.ID] = node.Direction;
                    // 隣接セルを新たな探索対象に追加
                    queue.Enqueue(linkCell);
                }
            }
        }
    }

    public class PathDirection
    {
        public Dictionary<ChamberID, float> Detour;
        public Dictionary<ChamberID, float> Shortest;

        public PathDirection()
        {
            Detour = new();
            Shortest = new();
        }
    }
}

