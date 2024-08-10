using AntColony.Game.Colonies.Finders;
using AntColony.Game.Colonies.Structures;

namespace AntColony.Game.Colonies.Ants
{
    public struct AntState
    {
        public AntStateKind Kind;
        public AntStateInfo Info;

        public static AntState ReturningToColony(ChamberID chamberID) => new()
        {
            Kind = AntStateKind.ReturningToColony,
            Info = new AntStateInfo { ReturningToColony = new AntStateReturningToColony(chamberID) },
        };

        public static AntState ExploringOutside(bool right) => new()
        {
            Kind = AntStateKind.ExploringOutside,
            Info = new AntStateInfo { ExploringOutside = new AntStateExploringOutside(right) },
        };

        public static AntState MoveToChamber(ChamberID chamberID, PathFindMode pathFindMode) => new()
        {
            Kind = AntStateKind.MoveToChamber,
            Info = new AntStateInfo { MoveToChamber = new AntStateMoveToChamber(chamberID, pathFindMode) },
        };

        public static AntState MoveToCell(Cell cell) => new()
        {
            Kind = AntStateKind.MoveToCell,
            Info = new AntStateInfo { MoveToCell = new AntStateMoveToCell(cell) },
        };
    }

    public struct AntStateInfo
    {
        public AntStateMoveToChamber MoveToChamber;
        public AntStateMoveToCell MoveToCell;
        public AntStateExploringOutside ExploringOutside;
        public AntStateReturningToColony ReturningToColony;
    }

    public struct AntStateMoveToCell
    {
        public Cell Cell;
        public AntStateMoveToCell(Cell cell)
        {
            Cell = cell;
        }
    }

    public struct AntStateMoveToChamber
    {
        public ChamberID ChamberID;
        public PathFindMode PathFindMode;
        public AntStateMoveToChamber(ChamberID chamberID, PathFindMode pathFindMode)
        {
            ChamberID = chamberID;
            PathFindMode = pathFindMode;
        }
    }

    public struct AntStateExploringOutside
    {
        public bool Right;
        public AntStateExploringOutside(bool right)
        {
            Right = right;
        }
    }

    public struct AntStateReturningToColony
    {
        public ChamberID ChamberId;
        public AntStateReturningToColony(ChamberID chamberId)
        {
            ChamberId = chamberId;
        }
    }

    public enum AntStateKind : int
    {
        MoveToChamber = 1,
        MoveToCell = 2,
        ExploringOutside = 3,
        ReturningToColony = 4,
    }
}