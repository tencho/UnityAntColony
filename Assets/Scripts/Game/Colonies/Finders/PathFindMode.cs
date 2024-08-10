namespace AntColony.Game.Colonies.Finders
{
    /// <summary>
    /// 経路検索の探索モード
    /// </summary>
    public enum PathFindMode : int
    {
        /// <summary>埋まった通路は迂回して探索する</summary>
        Detour = 1,

        /// <summary>埋まった通路を無視して最短経路で探索する</summary>
        Shortest = 2,
    }
}