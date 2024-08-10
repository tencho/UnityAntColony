using System;
using System.Collections.Generic;
using AntColony.Game.Colonies.Items;

namespace AntColony.Game.Colonies.Builders
{
    /// <summary>
    /// ColonySettingで設定する部屋に置けるアイテム種
    /// </summary>
    [Serializable]
    public class ChamberBuildInfo
    {
        public List<ItemKind> AllowedItems;
    }
}