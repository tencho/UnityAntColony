namespace AntColony.Game.Colonies.Structures
{
    /// <summary>
    /// 隣接セルとの繋がり情報
    /// </summary>
    public struct Node
    {
        /// <summary>セル間の距離</summary>
        public float Cost;
        
        /// <summary>接続先セル</summary>
        public Cell Link;

        /// <summary>接続先セルから接続元セルへの角度</summary>
        public float Direction;
    }
}