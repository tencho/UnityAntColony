using System.Linq;
using AntColony.Game.Colonies.Structures;
using Omoch.Randoms;

namespace AntColony.Game.Colonies.Ants.States
{
    public class WorkerStateMoveToChamber : WorkerStateBase<ChamberDistination>
    {
        public override void Update()
        {
            ChamberDistination distination = StateData;

            if (ExecuteInColony())
            {
                return;
            }

            var currentCell = Context.CurrentCell;

            // 目的の部屋に到達したら
            if (currentCell is not null && currentCell.ChamberID == distination.ChamberID)
            {
                var chamber = Context.Colony.GetChamber(distination.ChamberID);
                // 何か運んでいた場合
                if (Context.CarryingItem is not null)
                {
                    if (chamber.ItemDiscardable)
                    {
                        // 捨てられる部屋なら運んでいるものを消す
                        Context.DiscardCarryingItem();
                        Context.Wait(15);
                        StateMachine.ChangeState<WorkerStateThink>();
                        return;
                    }
                    else
                    {
                        // 部屋の空いているセルまで運ぶ
                        var cells = chamber.Cells.Where(cell => cell.IsDug && cell.Item is null).ToArray();
                        if (cells.Any())
                        {
                            var cell = Randomizer.Pick(cells);
                            StateMachine.ChangeState<WorkerStateMoveToCell, Cell>(cell);
                            return;
                        }
                    }
                }
                StateMachine.ChangeState<WorkerStateThink>();
                return;
            }

            float rotation;
            if (currentCell is not null && !currentCell.IsDug)
            {
                // 壁にめり込んだら近くの空間に向かう
                rotation = Context.Colony.FindNearbySpaceDirection(Context.GridX, Context.GridY);
            }
            else
            {
                var direction = Context.Colony.GetDirection(Context.GridX, Context.GridY, distination.ChamberID, distination.PathFindMode);
                rotation = direction + Context.RandomSwayAngle;
            }
            Context.RotateAndMove(rotation, 0f);
        }
    }
}