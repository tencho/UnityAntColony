using System;
using UnityEngine;

namespace AntColony.Game
{
    [Serializable]
    public class GameSceneLogicReferences
    {
        [field: SerializeField] public Transform ColonyRoot { get; set; }
    }
}