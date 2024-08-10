using AntColony.Game.Colonies.Items;
using AntColony.Game.Colonies.Structures;
using Omoch.Flows;
using UnityEngine;

#nullable enable

namespace AntColony.Game.Colonies.Ants.States
{
    public class QueenStateMoveToCell : OmStateMachine<AntLogic>.StateWith<Cell>
    {
        private const int StressLimit = 210;
        private int stress;
        private int time;

        public QueenStateMoveToCell()
        {
            stress = 0;
            time = 0;
        }

        public override void Update()
        {
            Cell targetCell = StateData;

            var currentCell = Context.CurrentCell;
            if (currentCell is null)
            {
                return;
            }

            // お腹が空いたら食べてみる
            if (Context.IsHungry && Context.TryEat(currentCell))
            {
                Context.Wait(45);
                return;
            }

            // 卵を産む
            if (++time % 150 == 0)
            {
                if (Context.Colony.TryGetNearbyEmptyCell(Context.GridX, Context.GridY, out Cell? nearbyCell))
                {
                    var egg = Context.Colony.AddItem(ItemKind.Egg);
                    egg.SetPosition(nearbyCell.CenterX, nearbyCell.CenterY);
                    nearbyCell.Item = egg;
                    Context.Wait(60);
                    return;
                }
            }

            // 目的のセルに到達したら（もしくは到達に時間がかかりすぎたら）
            if (stress++ > StressLimit || (currentCell is not null && currentCell == targetCell))
            {
                stress = 0;
                Context.Wait(15);
                StateMachine.ChangeState<QueenStateThink>();
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