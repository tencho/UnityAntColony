using AntColony.Game.Colonies.Structures;
using Omoch.Flows;
using UnityEngine;

namespace AntColony.Game.Colonies.Ants.States
{
    public class WorkerStateReturningToColony : OmStateMachine<AntLogic>.StateWith<ChamberID>
    {
        public override void Update()
        {
            ChamberID chamberID = StateData;
            // 帰巣中に入口付近に到達したら巣での行動に移る
            var entranceChamber = Context.Colony.GetChamber(chamberID);
            if (Mathf.Abs(entranceChamber.X - Context.X) < 2f)
            {
                StateMachine.ChangeState<WorkerStateThink>();
                return;
            }

            float entranceX = Context.Colony.GetChamber(chamberID).X;
            float offsetX = Mathf.Sign(entranceX - Context.X) * 1f;
            float rotation = Mathf.Atan2(Context.Colony.GetSurfaceY(Context.X + offsetX) - Context.Y, offsetX);
            Context.RotateAndMove(rotation, 0f);
        }
    }
}