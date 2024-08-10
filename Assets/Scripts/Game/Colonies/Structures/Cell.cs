using AntColony.Game.Colonies.Items;

#nullable enable

namespace AntColony.Game.Colonies.Structures
{
    /// <summary>
    /// コロニーを構成するセルの情報
    /// </summary>
    public class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float CenterX => X + 0.5f;
        public float CenterY => Y + 0.5f;

        /// <summary>このセルに配置されているアイテム</summary>
        public ItemLogic? Item { get; set; } = null;

        /// <summary>最短経路検索用の隣接セルとの繋がり情報</summary>
        public Node[] Nodes { get; set; } = new Node[0];

        /// <summary>既に掘っているか</summary>
        public bool IsDug { get; set; }

        /// <summary>掘ることができるセルか</summary>
        public bool IsDiggable { get; set; }

        /// <summary>セルの種別(土、石など)</summary>
        public CellKind Kind { get; set; }

        /// <summary>どの部屋に属しているか</summary>
        public ChamberID? ChamberID { get; set; } = null;
    }
}