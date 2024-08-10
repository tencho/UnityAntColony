using Omoch.Geom;
using UnityEngine;

#nullable enable

namespace AntColony.Game.Colonies.Builders
{
    /// <summary>
    /// コロニーの構成時の通路データで使う線分の情報
    /// </summary>
    public class PathwayLine : Line2
    {
        public int Thisness { get; }
        public bool IsDug { get; }

        public PathwayLine(Vector2 start, Vector2 end, int thisness, bool isDug) : base(start, end)
        {
            Thisness = thisness;
            IsDug = isDug;
        }
    }
}