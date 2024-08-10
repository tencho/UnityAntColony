using AntColony.Settings;
using Omoch.Framework;
using Omoch.Randoms;

namespace AntColony.Game.Colonies.Items
{
    public class ItemLogic
        : LogicBaseWithInput<IItemViewOrder, ItemInput>
        , IItemPeek
    {
        public ItemKind Kind { get; set; }
        public int Life { get; set; }
        public int Amount { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float RandomSeed { get; set; }

        public ItemLogic(ItemKind kind)
        {
            Kind = kind;
            X = 0;
            Y = 0;
            RandomSeed = Randomizer.NextFloat();
        }

        public void ApplySprite()
        {
            ViewOrder.ApplySprite();
        }

        public void SetPosition(float px, float py)
        {
            if (X == px && Y == py)
            {
                return;
            }

            X = px;
            Y = py;
            ViewOrder.ApplyPosition();
        }

        public void Show()
        {
            ViewOrder.Show();
        }
        public void Hide()
        {
            ViewOrder.Hide();
        }

        public void Reset(ColonySetting setting)
        {
            var itemInfo = setting.ItemInfos[Kind];
            Amount = itemInfo.Amount;
            Life = itemInfo.Life;
        }
    }

    public interface IItemPeek
    {
        public ItemKind Kind { get; }
        public int Life { get; }
        public int Amount { get; }
        public float X { get; }
        public float Y { get; }
        public float RandomSeed { get; }
    }
}

