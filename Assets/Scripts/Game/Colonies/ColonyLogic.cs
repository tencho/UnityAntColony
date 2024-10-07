using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AntColony.Game.Colonies.Ants;
using AntColony.Game.Colonies.Finders;
using AntColony.Game.Colonies.Items;
using AntColony.Game.Colonies.Structures;
using AntColony.Settings;
using Omoch.Framework;
using Omoch.Randoms;
using UnityEngine;
using VContainer;

#nullable enable

namespace AntColony.Game.Colonies
{
    /// <summary>
    /// コロニー全体の処理
    /// </summary>
    public class ColonyLogic
        : LogicBaseWithInput<IColonyViewOrder, ColonyInput>
        , IColonyPeek
        , IUpdatableLogic
    {
        [Inject] private readonly OmochBinder binder = null!;
        [Inject] private readonly ColonySetting setting = null!;

        private ColonyData? data;
        private PathFinder? pathFinder;

        private readonly List<AntLogic> ants;
        private readonly ItemLogicPool itemPool;
        private readonly Queue<AntLogic> removeAntQueue;

        private bool isBegan;
        private int time;
        private int antIndexCounter;
        private int itemIndexCounter;
        private bool colonyInitialized;

        public ColonyData Data => data ?? throw new ArgumentNullException();
        public IColonyDataPeek DataPeek => data ?? throw new ArgumentNullException();
        public PathFinder PathFinder => pathFinder ?? throw new ArgumentNullException();
        public ColonySetting Setting => setting;

        public int Speed { get; set; }
        public int TotalAnts { get; private set; }
        public int TotalEggs { get; private set; }
        public int TotalFoods { get; private set; }

        private static readonly Lazy<Offset[]> NearbyOffsets = new(LazyNearbyOffsets);

        public ColonyLogic()
        {
            ants = new List<AntLogic>();
            itemPool = new ItemLogicPool();
            removeAntQueue = new Queue<AntLogic>();

            isBegan = false;
            time = 0;
            antIndexCounter = 0;
            itemIndexCounter = 0;
            colonyInitialized = false;

            Speed = 1;
            TotalAnts = 0;
            TotalEggs = 0;
            TotalFoods = 0;
        }

        public void AfterInject()
        {
            binder.BindLogicWithInput<IColonyPeek, IColonyViewOrder, ColonyInput>(this, LinkKey.Colony);
        }

        public void SetColonyData(ColonyData data)
        {
            this.data = data;
            colonyInitialized = true;
            pathFinder = new PathFinder(data);
        }

        public void Begin()
        {
            isBegan = true;

            for (int i = 0; i < 50; i++)
            {
                AddAntAtChamber(AntKind.Worker, ChamberID.Queen);
            }
            AddAntAtChamber(AntKind.Queen, ChamberID.Queen);

            ViewOrder.DrawSurface();
            ViewOrder.DrawWall();
        }

        public void UpdateLogic()
        {
            if (colonyInitialized)
            {
                PathFinder.Update();
            }

            if (!isBegan)
            {
                return;
            }

            // シミュレーション速度分繰り返す
            for (int i = 0; i < Speed; i++)
            {
                time++;

                // 蟻の行動
                foreach (AntLogic ant in ants)
                {
                    ant.Simulate();
                }

                // 破棄された蟻をリストから消す
                while (removeAntQueue.Any())
                {
                    ants.Remove(removeAntQueue.Dequeue());
                }

                // 部屋の時間経過とアンロック
                if (time % 30 == 0)
                {
                    AdvanceRoomTime();
                    UpdateStatsAndUnlock();
                }

                // 定期的に迂回経路を探索する
                if (!PathFinder.IsFinding)
                {
                    PathFinder.FindPathAll(PathFindMode.Detour, true);
                }
            }
        }


        /// <summary>
        /// 全部屋の情報を調べ統計を更新しつつ必要なら部屋のアンロック処理もする
        /// </summary>
        public void UpdateStatsAndUnlock()
        {
            TotalAnts = ants.Count;
            TotalEggs = 0;
            TotalFoods = 0;

            var keys = Data.Chambers.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                var chamber = Data.Chambers[keys[i]];
                chamber.UpdateStats();
                TotalEggs += chamber.EggCount;
                TotalFoods += chamber.FoodAmount;
                // 部屋が一定割合以上掘られたら次の部屋をアンロックする
                if (i < keys.Length - 1 && chamber.DugRatio > setting.UnlockDugRatio)
                {
                    Data.Chambers[keys[i + 1]].IsUnlocked = true;
                }
            }
        }

        public void AdvanceRoomTime()
        {
            foreach (Chamber chamber in GetAllChambers())
            {
                // 部屋の中にアイテムがあれば時間経過させ、Lifeがなくなったら形態変化
                if (chamber.IsOpened)
                {
                    foreach (Cell cell in chamber.Cells)
                    {
                        ItemLogic? item = cell.Item;
                        if (item is null || item.Life <= 0 || --item.Life > 0)
                        {
                            continue;
                        }

                        switch (item.Kind)
                        {
                            case ItemKind.Dirt:
                            case ItemKind.Debris:
                                RemoveItem(item);
                                cell.Item = null;
                                break;

                            case ItemKind.Egg:
                                RemoveItem(item);
                                cell.Item = null;
                                AddAnt(AntKind.Worker, item.X, item.Y);
                                break;

                            case ItemKind.Food:
                            case ItemKind.Body:
                                item.Kind = ItemKind.Debris;
                                item.Reset(setting);
                                item.ApplySprite();
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 蟻を部屋内のランダムな位置に生成
        /// </summary>
        public void AddAntAtChamber(AntKind kind, ChamberID id)
        {
            var cell = Randomizer.Pick(GetChamber(id).Cells);
            AddAnt(kind, cell.X + Randomizer.NextFloat(), cell.Y + Randomizer.NextFloat());
        }

        /// <summary>
        /// 蟻を指定座標に生成
        /// </summary>
        public void AddAnt(AntKind kind, float x, float y)
        {
            var ant = new AntLogic(this, kind, x, y);
            ant.OnDispose += () => removeAntQueue.Enqueue(ant);
            ants.Add(ant);

            antIndexCounter++;
            binder.BindLogic<IAntPeek, IAntViewOrder>(ant, LinkKey.Ant(antIndexCounter));
            ViewOrder.AddAnt(antIndexCounter);
        }

        public void RemoveItem(ItemLogic item)
        {
            item.Hide();
            itemPool.Add(item);
        }

        public ItemLogic AddItem(ItemKind kind)
        {
            if (itemPool.Pop(kind, out ItemLogic item))
            {
                item.Show();
            }
            else
            {
                itemIndexCounter++;
                binder.BindLogicWithInput<IItemPeek, IItemViewOrder, ItemInput>(item, LinkKey.Item(itemIndexCounter));
                ViewOrder.AddItem(itemIndexCounter);
            }
            item.Reset(setting);
            item.ApplySprite();

            return item;
        }

        /// <summary>
        /// その座標から部屋に向かう方向を取得
        /// </summary>
        public float GetDirection(int x, int y, ChamberID chamberID, PathFindMode findMode)
        {
            return PathFinder.GetDirection(x, y, chamberID, findMode);
        }

        /// <summary>
        /// 部屋をIDで取得する
        /// </summary>
        public Chamber GetChamber(ChamberID id)
        {
            return Data.Chambers[id];
        }

        /// <summary>
        /// 指定種類の部屋を全て取得する
        /// </summary>
        public Chamber[] GetChambersByKind(ChamberKind kind)
        {
            return Data.Chambers.Values.Where(chamber => chamber.Kind == kind).ToArray();
        }

        /// <summary>
        /// 指定アイテム種が置ける開通済みの部屋を全て取得する
        /// </summary>
        public Chamber[] GetOpenedChambersByAllowedItem(ItemKind kind)
        {
            return Data.Chambers.Values.Where(chamber => chamber.IsOpened && chamber.AllowedItemKinds.Has(kind)).ToArray();
        }

        /// <summary>
        /// アンロック済みの部屋を全て取得する
        /// </summary>
        public Chamber[] GetUnlockedChambers()
        {
            return Data.Chambers.Values.Where(chamber => chamber.IsUnlocked).ToArray();
        }

        /// <summary>
        /// 開通済みの部屋を全て取得する
        /// </summary>
        public Chamber[] GetOpenedChambers()
        {
            return Data.Chambers.Values.Where(chamber => chamber.IsOpened).ToArray();
        }

        /// <summary>
        /// 指定位置のセルを掘ってViewに反映する
        /// </summary>
        public void DigAndApply(int x, int y)
        {
            if (Data.TryGetCell(x, y, out var cell))
            {
                cell.IsDug = true;
                if (cell.ChamberID is not null)
                {
                    var chamber = GetChamber(cell.ChamberID.Value);
                    chamber.IsOpened = chamber.IsUnlocked;
                    chamber.LastDugCell = cell;
                }
                ViewOrder.Dig(x, y);
            }
        }

        public bool TryGetCell(int x, int y, [NotNullWhen(true)] out Cell? cell)
        {
            return Data.TryGetCell(x, y, out cell);
        }

        public IEnumerable<Chamber> GetAllChambers()
        {
            return Data.Chambers.Values;
        }

        public float GetSurfaceY(float x)
        {
            return setting.ColonySize.Height - Data.Surface.GetDepth(x);
        }

        /// <summary>
        /// 近くの何も置かれていない空洞のセルを取得
        /// </summary>
        public bool TryGetNearbyEmptyCell(int x, int y, [NotNullWhen(true)] out Cell? nearbyCell)
        {
            foreach (Offset offset in NearbyOffsets.Value)
            {
                if (TryGetCell(x + offset.X, y + offset.Y, out Cell? cell))
                {
                    if (cell.IsDug && cell.Item is null)
                    {
                        nearbyCell = cell;
                        return true;
                    }
                }
            }
            nearbyCell = null;
            return false;
        }

        /// <summary>
        /// 近くの空間を探して角度を返す
        /// </summary>
        public float FindNearbySpaceDirection(int x, int y)
        {
            var tx = 0f;
            var ty = 0f;
            // NearbyOffsets.Valueの最初は(0,0)なので2番目から
            for (var i = 1; i < NearbyOffsets.Value.Length; i++)
            {
                Offset offset = NearbyOffsets.Value[i];
                int px = x + offset.X;
                int py = y + offset.Y;
                if (TryGetCell(px, py, out var nearbyCell) && nearbyCell.IsDug)
                {
                    tx += offset.X / offset.Distance;
                    ty += offset.Y / offset.Distance;
                }
            }

            return Mathf.Atan2(ty, tx);
        }

        private static Offset[] LazyNearbyOffsets()
        {
            var offsets = new List<Offset>();
            for (var dx = -2; dx <= 2; dx++)
            {
                for (var dy = -2; dy <= 2; dy++)
                {
                    offsets.Add(new Offset(dx, dy, Mathf.Sqrt(dx * dx + dy * dy)));
                }
            }
            offsets.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            return offsets.ToArray();
        }

        /// <summary>
        /// コロニー座標に近い蟻を返す
        /// </summary>
        public bool TryGetNearbyAnt(Vector2 point, [NotNullWhen(true)] out AntLogic? neabyAnt)
        {
            foreach (AntLogic ant in ants)
            {
                float distance = Vector2.Distance(new Vector2(ant.X, ant.Y), point);
                if (distance < 3f)
                {
                    neabyAnt = ant;
                    return true;
                }
            }

            neabyAnt = null;
            return false;
        }
    }

    public interface IColonyPeek
    {
        public IColonyDataPeek DataPeek { get; }
    }

    public class Offset
    {
        public int X;
        public int Y;
        public float Distance;

        public Offset(int x, int y, float distance)
        {
            X = x;
            Y = y;
            Distance = distance;
        }
    }
}