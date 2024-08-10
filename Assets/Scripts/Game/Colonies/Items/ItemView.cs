using AntColony.Game.Colonies.Structures;
using AntColony.Settings;
using Omoch.Framework;
using UnityEngine;

namespace AntColony.Game.Colonies.Items
{
    public class ItemView
        : MonoViewBaseWithInput<IItemPeek, ItemInput>
        , IItemViewOrder
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ColonySetting setting;

        public override void Initialized()
        {
            ApplySprite();
            ApplyPosition();
        }

        public void ApplySprite()
        {
            var icons = setting.ItemInfos[Peek.Kind].Icons;
            spriteRenderer.sprite = icons[Mathf.FloorToInt(icons.Count * Peek.RandomSeed)];
        }

        public void ApplyPosition()
        {
            var position = transform.localPosition;
            position.x = (Peek.X - setting.ColonySize.Width / 2f) * ColonyConsts.SizePerCell;
            position.y = (Peek.Y - setting.ColonySize.Height) * ColonyConsts.SizePerCell;
            transform.localPosition = position;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }

    public interface IItemViewOrder
    {
        public void ApplySprite();
        public void ApplyPosition();
        public void Show();
        public void Hide();
    }
}

