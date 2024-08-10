using AntColony.Game.Colonies.Structures;
using UnityEngine;

namespace AntColony.Game.Colonies.Ants.States
{
    public class WorkerStateMoveToCell : WorkerStateBase<Cell>
    {
        private const int StressLimit = 210;
        private int stress;

        public WorkerStateMoveToCell()
        {
            stress = 0;
        }

        public override void Update()
        {
            Cell targetCell = StateData;
            
            if (ExecuteInColony())
            {
                return;
            }

            var currentCell = Context.CurrentCell;

            // 目的のセルに到達したら（もしくは到達に時間がかかりすぎたら）
            if (stress++ > StressLimit || (currentCell is not null && currentCell == targetCell))
            {
                stress = 0;

                // 運んでいるものがあれば近くのセルに置く
                if (Context.CarryingItem is not null)
                {
                    Context.TryDropCarryingItem();
                }
                Context.Wait(15);
                StateMachine.ChangeState<WorkerStateThink>();
                return;
            }

            float targetRotation;
            float turningIntensity;
            if (currentCell is not null && !currentCell.IsDug)
            {
                // 壁にめり込んだら近くの空間に向かう
                targetRotation = Context.Colony.FindNearbySpaceDirection(Context.GridX, Context.GridY);
                turningIntensity = 0f;
            }
            else
            {
                targetRotation = Mathf.Atan2(targetCell.CenterY - Context.Y, targetCell.CenterX - Context.X);
                // セルに近づくほど小回りが効くようにする
                float dx = targetCell.CenterX - Context.X;
                float dy = targetCell.CenterY - Context.Y;
                turningIntensity = Mathf.Max(Mathf.InverseLerp(4f, 0f, Mathf.Sqrt(dx * dx + dy * dy)), 0f);
            }
            Context.RotateAndMove(targetRotation, turningIntensity);
        }
    }
}