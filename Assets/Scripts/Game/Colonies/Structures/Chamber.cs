using System;
using AntColony.Game.Colonies.Ants;
using AntColony.Game.Colonies.Items;
using UnityEngine;

#nullable enable

namespace AntColony.Game.Colonies.Structures
{
    /// <summary>
    /// 部屋の情報
    /// </summary>
    public class Chamber : IChamber
    {
        public ChamberID ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public ChamberKind Kind { get; set; }
        public ItemKindMask AllowedItemKinds { get; set; }
        public bool IsOpened { get; set; }
        public bool IsUnlocked { get; set; }
        public Cell[] Cells { get; set; }
        public bool ItemDiscardable { get; set; }
        public Cell? LastDugCell { get; set; }

        public int DugCount { get; private set; }
        public int EggCount { get; private set; }
        public int FoodAmount { get; private set; }
        public float DugRatio { get; private set; }

        public Chamber(ChamberID id, int x, int y, ChamberKind kind, bool isOpened, ItemKindMask allowedItemKinds, bool canDiscardItem)
        {
            ID = id;
            X = x;
            Y = y;
            Kind = kind;
            IsOpened = isOpened;
            IsUnlocked = isOpened;
            AllowedItemKinds = allowedItemKinds;
            ItemDiscardable = canDiscardItem;
            LastDugCell = null;
            Cells = new Cell[0];

            DugCount = 0;
            EggCount = 0;
            FoodAmount = 0;
            DugRatio = 0;
        }

        /// <summary>
        /// 部屋を掘った割合[0f-1f]
        /// </summary>
        public void UpdateStats()
        {
            DugCount = 0;
            EggCount = 0;
            FoodAmount = 0;
            DugRatio = 0;
            if (IsOpened)
            {
                foreach (Cell cell in Cells)
                {
                    if (cell.IsDug) DugCount++;
                    if (cell.Item?.Kind == ItemKind.Egg) EggCount++;
                    if (cell.Item?.Kind == ItemKind.Food) FoodAmount += cell.Item.Amount;
                }
            }
            DugRatio = DugCount / (float)Cells.Length;
        }

        /// <summary>
        /// この部屋に行く優先度を取得
        /// </summary>
        public float GetPriority(ColonyLogic colony)
        {
            switch (Kind)
            {
                case ChamberKind.Entrance:
                    // コロニー内の食料が必要量にどれだけ足りているかで優先度を変える
                    float requiredFoods = (colony.TotalAnts + 1) * AntLogic.SatietyMax;
                    float satietyRatio = Mathf.Clamp(colony.TotalFoods / requiredFoods, 0f, 1f);
                    return Mathf.Lerp(4f, 0f, satietyRatio);

                case ChamberKind.Queen:
                    return 1f;

                case ChamberKind.Free:
                    return (IsOpened ? 1f : 4f) + (1f - DugRatio);

                default:
                    throw new NotImplementedException();
            }
        }
    }

    public interface IChamber
    {
    }
}