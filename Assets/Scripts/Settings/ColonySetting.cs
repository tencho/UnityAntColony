using System.Collections.Generic;
using AntColony.Game.Colonies.Builders;
using AntColony.Game.Colonies.Items;
using Omoch.Collections;
using Omoch.Geom;
using UnityEngine;

namespace AntColony.Settings
{
    [CreateAssetMenu(fileName = "ColonySetting", menuName = "ScriptableObject/ColonySetting")]
    public class ColonySetting : ScriptableObject
    {
        [field: SerializeField] public Size2Int ColonySize { get; private set; }
        [field: SerializeField] public float UnlockDugRatio { get; private set; }
        [field: SerializeField] public float SurfaceWidth { get; private set; }
        [field: SerializeField] public float SurfaceDepth { get; private set; }
        [field: SerializeField] public SerializableDictionary<ItemKind, ItemInfo> ItemInfos { get; private set; }
        [field: SerializeField] public List<ChamberBuildInfo> Chambers { get; private set; }

#if UNITY_EDITOR
        public ColonySetting()
        {
            ColonySize = new(256, 160);
            ItemInfos = new();
            ItemInfos[ItemKind.Dirt] = new ItemInfo
            {
                Icons = new() { null },
                Life = 150,
                Amount = 1,
            };
            ItemInfos[ItemKind.Food] = new ItemInfo
            {
                Icons = new() { null },
                Life = 400,
                Amount = 250,
            };
            ItemInfos[ItemKind.Egg] = new ItemInfo
            {
                Icons = new() { null },
                Life = 300,
                Amount = 1,
            };
            ItemInfos[ItemKind.Debris] = new ItemInfo
            {
                Icons = new() { null },
                Life = 150,
                Amount = 1,
            };
            ItemInfos[ItemKind.Body] = new ItemInfo
            {
                Icons = new() { null },
                Life = 150,
                Amount = 1,
            };
            Chambers = new()
            {
                new ChamberBuildInfo{ AllowedItems = new(){ ItemKind.Food } },
                new ChamberBuildInfo{ AllowedItems = new(){ ItemKind.Egg } },
                new ChamberBuildInfo{ AllowedItems = new(){ ItemKind.Debris, ItemKind.Body } },
                new ChamberBuildInfo{ AllowedItems = new(){ ItemKind.Food } },
                new ChamberBuildInfo{ AllowedItems = new(){ ItemKind.Egg } },
                new ChamberBuildInfo{ AllowedItems = new(){ ItemKind.Debris, ItemKind.Body } },
                new ChamberBuildInfo{ AllowedItems = new(){  } },
                new ChamberBuildInfo{ AllowedItems = new(){ ItemKind.Food } },
            };
            UnlockDugRatio = 0.8f;
            SurfaceWidth = 800f;
            SurfaceDepth = 10f;
        }
#endif
    }
}