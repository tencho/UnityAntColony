using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AntColony.Game.Colonies.Structures;
using Cysharp.Threading.Tasks;

#nullable enable

namespace AntColony.Game.Colonies.Finders
{
    /// <summary>
    /// 全セルの部屋への経路探索をする
    /// </summary>
    public class PathFinder
    {
        /// <summary>
        /// パフォーマンスモードON時に経路探索にかけられる時間/frame
        /// </summary>
        private const int FrameTimeLimitLow = 5;
        /// <summary>
        /// パフォーマンスモードOFF時に経路探索にかけられる時間/frame
        /// </summary>
        private const int FrameTimeLimitHigh = 33;

        private readonly ColonyData data;
        /// <summary>ダイクストラ法で経路探索する際の移動コストを記録</summary>
        private readonly float[,] cellCosts;
        /// <summary>各セルの部屋へ到達するための角度を記録</summary>
        private readonly PathDirection[,] pathDirections;

        private readonly Queue<Cell> cellQueue;
        private readonly Queue<Chamber> chamberQueue;
        private Chamber? currentCamber;
        private PathFindMode? currentMode;
        private Stopwatch stopwatch;
        private bool isPerformanceMode;

        /// <summary>経路探索中かどうか</summary>
        public bool IsFinding { get; private set; }

        public PathFinder(ColonyData data)
        {
            this.data = data;

            IsFinding = false;
            cellQueue = new Queue<Cell>();
            chamberQueue = new Queue<Chamber>();
            currentCamber = null;
            currentMode = null;
            stopwatch = new Stopwatch();
            isPerformanceMode = false;

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
            // 0 <= x < width && 0 <= y < height
            if ((uint)x < (uint)data.Width && (uint)y < (uint)data.Height)
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
        public async UniTask FindPathAllAsync(PathFindMode mode, bool isPerformanceMode)
        {
            FindPathAll(mode, isPerformanceMode);
            await UniTask.WaitUntil(() => !IsFinding);
        }

        public void FindPathAll(PathFindMode mode, bool isPerformanceMode)
        {
            if (IsFinding)
            {
                throw new Exception("現在経路探索中です。");
            }

            currentMode = mode;
            this.isPerformanceMode = isPerformanceMode;
            foreach (Chamber chamber in data.Chambers.Values)
            {
                chamberQueue.Enqueue(chamber);
            }
            TryFindNextChamber();
        }

        private void TryFindNextChamber()
        {
            if (!chamberQueue.Any())
            {
                IsFinding = false;
                return;
            }

            IsFinding = true;
            currentCamber = chamberQueue.Dequeue();

            var startCell = currentCamber.LastDugCell;
            if (startCell is null)
            {
                TryFindNextChamber();
                return;
            }

            // 全経路のコストをリセットしておく
            for (int ix = 0; ix < data.Width; ix++)
            {
                for (int iy = 0; iy < data.Height; iy++)
                {
                    cellCosts[ix, iy] = float.MaxValue;
                    var pathDirection = currentMode switch
                    {
                        PathFindMode.Detour => pathDirections[ix, iy].Detour,
                        PathFindMode.Shortest => pathDirections[ix, iy].Shortest,
                        _ => throw new NotImplementedException(),
                    };
                    pathDirection[currentCamber.ID] = 0f;
                }
            }

            // 探索開始セルは部屋で最後に掘られたセル
            cellCosts[startCell.X, startCell.Y] = 0f;
            cellQueue.Enqueue(startCell);
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

        public void Update()
        {
            if (!IsFinding || currentMode == null || currentCamber == null)
            {
                return;
            }

            // 対象セルがなくなるまで探索
            int timeLimit = isPerformanceMode ? FrameTimeLimitLow : FrameTimeLimitHigh;
            stopwatch.Restart();
            while (cellQueue.Any() && stopwatch.ElapsedMilliseconds <= timeLimit)
            {
                var cell = cellQueue.Dequeue();
                var cellCost = cellCosts[cell.X, cell.Y];
                // 隣接セルを探索
                foreach (Node node in cell.Nodes)
                {
                    var linkCell = node.Link;
                    var linkCellCost = cellCosts[linkCell.X, linkCell.Y];
                    var newCost = cellCost + node.Cost;
                    // 隣接セルが空洞でないor開始地点からのコストが低い値に更新できなければ終了
                    if (!IsSpace(linkCell, currentMode.Value) || newCost >= linkCellCost)
                    {
                        continue;
                    }

                    cellCosts[linkCell.X, linkCell.Y] = newCost;
                    var pathDirection = currentMode switch
                    {
                        PathFindMode.Detour => pathDirections[linkCell.X, linkCell.Y].Detour,
                        PathFindMode.Shortest => pathDirections[linkCell.X, linkCell.Y].Shortest,
                        _ => throw new NotImplementedException(),
                    };
                    pathDirection[currentCamber.ID] = node.Direction;
                    // 隣接セルを新たな探索対象に追加
                    cellQueue.Enqueue(linkCell);
                }
            }
            stopwatch.Stop();

            if (!cellQueue.Any())
            {
                TryFindNextChamber();
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

