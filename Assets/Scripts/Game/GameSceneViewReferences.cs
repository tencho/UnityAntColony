using System;
using Omoch.Sprites;
using UnityEngine;

namespace AntColony.Game
{
    [Serializable]
    public class GameSceneViewReferences
    {
        /// <summary>Gameカメラ</summary>
        [SerializeField] public Camera mainCamera;

        /// <summary>Canvasカメラ</summary>
        [SerializeField] public Camera canvasCamera;

        /// <summary>蟻の巣などを表示するRootオブジェクト</summary>
        [SerializeField] public Transform colonyRoot;

        /// <summary>マウスドラッグ処理を無視するCanvas上の領域</summary>
        [SerializeField] public RectTransform[] ignoreClickRects;

        /// <summary>コロニー内壁描画用Sprite</summary>
        [SerializeField] public SpriteRenderer wallSpriteRenderer;

        /// <summary>蟻</summary>
        [SerializeField] public GameObject antObject;

        /// <summary>アイテム</summary>
        [SerializeField] public GameObject itemObject;

        /// <summary>蟻を配置するコンテナ</summary>
        [SerializeField] public Transform antContainer;

        /// <summary>アイテムを配置するコンテナ</summary>
        [SerializeField] public Transform itemContainer;

        /// <summary>空スプライト</summary>
        [SerializeField] public SpriteRectTransform sky;
    }
}