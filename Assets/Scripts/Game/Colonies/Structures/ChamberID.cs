namespace AntColony.Game.Colonies.Structures
{
    public readonly struct ChamberID
    {
        private readonly int id;
        public ChamberID(int id) => this.id = id;
        public override bool Equals(object value) => value is ChamberID chamberID && chamberID.id == id;
        public override int GetHashCode() => id;
        public override string ToString() => id.ToString();
        public int ToInt() => id;

        public static explicit operator int(ChamberID chamberID) => chamberID.id;
        public static bool operator ==(ChamberID a, ChamberID b) => a.id == b.id;
        public static bool operator !=(ChamberID a, ChamberID b) => !(a == b);
        public static bool operator ==(ChamberID? a, ChamberID? b) => a?.Equals(b) ?? b is null;
        public static bool operator !=(ChamberID? a, ChamberID? b) => !(a == b);
        
        /// <summary>巣の入口</summary>
        public static ChamberID Entrance = new ChamberID(-1);

        /// <summary>女王蟻の部屋</summary>
        public static ChamberID Queen = new ChamberID(-2);
    }
}