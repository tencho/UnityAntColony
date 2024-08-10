using AntColony.Game.Colonies.Structures;
using AntColony.Settings;
using UnityEngine;
using VContainer;

#nullable enable

namespace AntColony.Game.Colonies
{
    /// <summary>
    /// コロニーマップデータから内壁を描画する
    /// </summary>
    public class ColonyWallView
    {
        private Texture2D? texture;

        [Inject] private readonly GameSceneViewReferences references = null!;
        [Inject] private readonly ColonySetting setting = null!;

        public ColonyWallView()
        {
        }

        public void AfterInjedct()
        {
            var colonySize = setting.ColonySize;
            var spriteRenderer = references.wallSpriteRenderer;

            // SpriteRendererに新規スプライトを割り当てる
            texture = new Texture2D(colonySize.Width, colonySize.Height, TextureFormat.ARGB32, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 1f), 1f);
            spriteRenderer.enabled = false;
            spriteRenderer.transform.localScale = new Vector3(ColonyConsts.SizePerCell, ColonyConsts.SizePerCell, 1f);
        }

        /// <summary>
        /// コロニー構成データから巣の内壁などを描画
        /// </summary>
        public void Draw(IColonyDataPeek colony)
        {
            if (texture == null)
            {
                return;
            }

            var colonySize = setting.ColonySize;
            var cells = colony.Cells;
            var colors = new Color[colonySize.Width * colonySize.Height];
            for (int ix = 0; ix < colonySize.Width; ix++)
            {
                for (int iy = 0; iy < colonySize.Height; iy++)
                {
                    var cell = cells[ix, iy];
                    var colorIndex = iy * colonySize.Width + ix;
                    colors[colorIndex] = cell.IsDug ? Color.white : Color.black;
                }
            }
            texture.SetPixels(colors);
            texture.Apply();

            references.wallSpriteRenderer.enabled = true;
        }

        public void Dispose()
        {
            GameObject.Destroy(texture);
        }

        public void Dig(int x, int y)
        {
            if (texture == null)
            {
                return;
            }

            texture.SetPixel(x, y, Color.white);
            texture.Apply();
        }
    }
}