using System.Collections.Generic;
using UnityEngine;

namespace AntColony.Game.Colonies.Items
{
    [CreateAssetMenu(fileName = "ItemSource", menuName = "ScriptableObject/ItemSource")]
    public class ItemSource : ScriptableObject
    {
        [field: SerializeField] public List<Sprite> Icons { get; set; }
        [field: SerializeField] public int Life { get; set; }
        [field: SerializeField] public int Amount { get; set; }
    }
}