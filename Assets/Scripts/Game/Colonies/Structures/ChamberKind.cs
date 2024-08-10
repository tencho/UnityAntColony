namespace AntColony.Game.Colonies.Structures
{
    /// <summary>
    /// 部屋の種別
    /// </summary>
    public enum ChamberKind : int
    {
        /// <summary>コロニーの入口</summary>
        Entrance = 1,

        /// <summary>多目的な部屋</summary>
        Free = 2,

        /// <summary>女王の部屋</summary>
        Queen = 3,
    }
}