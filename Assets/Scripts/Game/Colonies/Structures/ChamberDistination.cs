using AntColony.Game.Colonies.Finders;

namespace AntColony.Game.Colonies.Structures
{
    public struct ChamberDistination
    {
        public ChamberID ChamberID { get; }
        public PathFindMode PathFindMode { get; }

        public ChamberDistination(ChamberID chamberID, PathFindMode pathFindMode)
        {
            ChamberID = chamberID;
            PathFindMode = pathFindMode;
        }
    }
}