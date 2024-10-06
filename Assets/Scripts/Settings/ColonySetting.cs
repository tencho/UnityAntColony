using System;
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
        [field: SerializeField] public SerializableDictionary<ItemKind, ItemSource> ItemInfos { get; private set; }
        [field: SerializeField] public List<ChamberBuildInfo> Chambers { get; private set; }

#if UNITY_EDITOR
        public ColonySetting()
        {
            ColonySize = new(256, 160);
            ItemInfos = new();
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