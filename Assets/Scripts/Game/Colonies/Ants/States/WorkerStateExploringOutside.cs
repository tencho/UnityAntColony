using AntColony.Game.Colonies.Items;
using AntColony.Game.Colonies.Structures;
using Omoch.Flows;
using Omoch.Tools;
using UnityEngine;

namespace AntColony.Game.Colonies.Ants.States
{
    public class WorkerStateExploringOutside : OmStateMachine<AntLogic>.StateWith<bool>
    {
        public override void Update()
        {
            bool isRight = StateData;
            
            // 外を探索中に端まで到達したら
            int ColonyWidth = Context.Colony.Setting.ColonySize.Width;
            if ((isRight && Context.X > ColonyWidth) || (!isRight && Context.X < 0))
            {
                // 食料を持って帰る
                Context.CarryingItem = Context.Colony.AddItem(ItemKind.Food);
                Context.Wait(30);
                Context.ApplyCarriedPosition();
                StateMachine.ChangeState<WorkerStateReturningToColony, ChamberID>(ChamberID.Entrance);
                return;
            }

            float offsetX = isRight.ToSign() * 1f;
            float rotation = Mathf.Atan2(Context.Colony.GetSurfaceY(Context.X + offsetX) - Context.Y, offsetX);
            Context.RotateAndMove(rotation, 0f);
        }
    }
}