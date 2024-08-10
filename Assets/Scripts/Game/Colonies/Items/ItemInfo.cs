using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntColony.Game.Colonies.Items
{
    [Serializable]
    public class ItemInfo
    {
        [field: SerializeField] public List<Sprite> Icons { get; set; }
        [field: SerializeField] public int Life { get; set; }
        [field: SerializeField] public int Amount { get; set; }
    }
}