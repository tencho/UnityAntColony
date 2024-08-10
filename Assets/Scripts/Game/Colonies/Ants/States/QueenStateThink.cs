using System.Linq;
using AntColony.Game.Colonies.Items;
using AntColony.Game.Colonies.Structures;
using Omoch.Flows;
using Omoch.Randoms;

namespace AntColony.Game.Colonies.Ants.States
{
    public class QueenStateThink : OmStateMachine<AntLogic>.State
    {
        public override void Enter(OmStateMachine<AntLogic>.IState prevState)
        {
            var cells = Context.Colony.GetChamber(ChamberID.Queen).Cells;
            if (Context.IsHungry)
            {
                var foodCells = cells.Where(cell => cell.Item is not null && cell.Item.Kind == ItemKind.Food).ToArray();
                if (foodCells.Any())
                {
                    cells = foodCells;
                }
            }

            var cell = Randomizer.Pick(cells);
            StateMachine.ChangeState<QueenStateMoveToCell, Cell>(cell);
        }
    }
}