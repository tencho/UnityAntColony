using System.Linq;
using AntColony.Game.Colonies.Items;
using Omoch.Flows;

namespace AntColony.Game.Colonies.Ants.States
{
    public class WorkerStateBase<TData> : OmStateMachine<AntLogic>.StateWith<TData>
    {
        protected bool ExecuteInColony()
        {
            var currentCell = Context.CurrentCell;
            if (currentCell is null)
            {
                return false;
            }

            // 何も運んでいない時
            if (Context.CarryingItem is null)
            {
                var currentItem = currentCell.Item;

                // お腹が空いたら食べてみる
                if (Context.IsHungry && Context.TryEat(currentCell))
                {
                    Context.Wait(45);
                    return true;
                }

                // 掘削可能なセルに接触したら掘って入口へ向かう
                if (currentCell.IsDiggable && !currentCell.IsDug)
                {
                    Context.CarryingItem = Context.Colony.AddItem(ItemKind.Dirt);
                    Context.ApplyCarriedPosition();
                    Context.Colony.DigAndApply(Context.GridX, Context.GridY);
                    Context.Wait(30);
                    StateMachine.ChangeState<WorkerStateThink>();
                    return true;
                }

                // 足元のアイテムが通路もしくは場違いな部屋にあったら拾う
                if (currentItem is not null && (currentCell.ChamberID is null || !Context.Colony.GetChamber(currentCell.ChamberID.Value).AllowedItemKinds.Has(currentItem.Kind)))
                {
                    // 足元のアイテムが運び込める部屋がある時のみ拾う
                    if (Context.Colony.GetOpenedChambersByAllowedItem(currentItem.Kind).Any())
                    {
                        Context.CarryingItem = currentItem;
                        currentCell.Item = null;
                        Context.ApplyCarriedPosition();
                        Context.Wait(15);
                        StateMachine.ChangeState<WorkerStateThink>();
                        return true;
                    }
                }
            }

            return false;
        }
    }
}