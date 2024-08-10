using System.Collections.Generic;

namespace AntColony.Game.Colonies.Items
{
    public class ItemLogicPool
    {
        private readonly Stack<ItemLogic> items;

        public ItemLogicPool()
        {
            items = new();
        }

        public void Add(ItemLogic item)
        {
            items.Push(item);
        }

        /// <summary>
        /// プールからアイテムを取り出し成功すればtrueを返す。プールが空なら新規アイテムインスタンスを生成してfalseを返す
        /// </summary>
        public bool Pop(ItemKind kind, out ItemLogic result)
        {
            if (items.TryPop(out var item))
            {
                item.Kind = kind;
                result = item;
                return true;
            }
            else
            {
                result = new ItemLogic(kind);
                return false;
            }
        }
    }
}