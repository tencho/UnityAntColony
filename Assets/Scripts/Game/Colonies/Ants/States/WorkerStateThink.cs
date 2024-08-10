using System.Linq;
using AntColony.Game.Colonies.Finders;
using AntColony.Game.Colonies.Structures;
using Omoch.Flows;
using Omoch.Randoms;

namespace AntColony.Game.Colonies.Ants.States
{
    public class WorkerStateThink : OmStateMachine<AntLogic>.State
    {
        // 優先度を重みにして部屋をランダム選択する用
        private WeightedRandomSelector<ChamberID> randomSelector;

        public WorkerStateThink()
        {
            randomSelector = new();
        }

        public override void Enter(OmStateMachine<AntLogic>.IState prevState)
        {
            // 何も運んでいないなら
            if (Context.CarryingItem is null)
            {
                // 今部屋にいるなら
                if (Context.CurrentCell is not null && Context.CurrentCell.ChamberID is not null)
                {
                    var currentChamber = Context.Colony.GetChamber(Context.CurrentCell.ChamberID.Value);
                    // 入口にいるなら確率で外へ探索
                    if (currentChamber.Kind == ChamberKind.Entrance && Randomizer.NextFloat() < 0.2f)
                    {
                        var isRight = Randomizer.NextBool();
                        StateMachine.ChangeState<WorkerStateExploringOutside, bool>(isRight);
                        return;
                    }

                    // 部屋内のまだ掘っていないセルがあれば掘りに行く
                    if (Randomizer.NextFloat() < 0.8f)
                    {
                        var dirtCells = currentChamber.Cells.Where(cell => !cell.IsDug).ToArray();
                        if (dirtCells.Any())
                        {
                            var cell = Randomizer.Pick(dirtCells);
                            StateMachine.ChangeState<WorkerStateMoveToCell, Cell>(cell);
                            return;
                        }
                    }

                    // 部屋内のアイテムの場所までいく
                    if (Randomizer.NextFloat() < 0.6f)
                    {
                        var itemCells = currentChamber.Cells.Where(cell => cell.Item is not null).ToArray();
                        if (itemCells.Any())
                        {
                            var cell = Randomizer.Pick(itemCells);
                            StateMachine.ChangeState<WorkerStateMoveToCell, Cell>(cell);
                            return;
                        }
                    }

                    // 確率で部屋内のランダムな場所に行く
                    // 別の場所から部屋に辿り着いていた場合は確定で行く
                    if (Randomizer.NextFloat() < 0.3f || prevState == StateMachine.GetState<WorkerStateMoveToChamber>())
                    {
                        var dugCells = currentChamber.Cells.Where(cell => cell.IsDug).ToArray();
                        if (dugCells.Any())
                        {
                            var cell = Randomizer.Pick(dugCells);
                            StateMachine.ChangeState<WorkerStateMoveToCell, Cell>(cell);
                            return;
                        }
                    }
                }

                // ランダムなアンロック済みの部屋に直行
                Chamber[] unlockedChambers = Context.Colony.GetUnlockedChambers();
                // 優先度の高い部屋が選ばれやすい
                randomSelector.Clear();
                foreach (var chamber in unlockedChambers)
                {
                    randomSelector.Add(chamber.ID, chamber.GetPriority(Context.Colony));
                }
                var chamberID = randomSelector.GetRandom(Randomizer.NextFloat());
                StateMachine.ChangeState<WorkerStateMoveToChamber, ChamberDistination>
                    (new ChamberDistination(chamberID, PathFindMode.Shortest));
            }
            else
            {
                // 運んでいるものが置けるどこかの部屋に行く
                var chambers = Context.Colony.GetOpenedChambersByAllowedItem(Context.CarryingItem.Kind);
                var chamber = Randomizer.Pick(chambers);
                StateMachine.ChangeState<WorkerStateMoveToChamber, ChamberDistination>
                    (new ChamberDistination(chamber.ID, PathFindMode.Detour));
            }
        }
    }
}