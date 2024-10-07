using System.Diagnostics.CodeAnalysis;
using Omoch.Collections;

#nullable enable

namespace AntColony.Game.Colonies.Structures
{
    /// <summary>
    /// コロニーの構成データ
    /// </summary>
    public class ColonyData : IColonyDataPeek
    {
        /// <summary>
        /// 格子状に並んだ全セルのデータ。原点は左下で右上が正
        /// </summary>
        public Cell[,] Cells { get; set; }

        /// <summary>
        /// セルの幅(個数)
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// セルの高さ(個数)
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// 全部屋のデータ
        /// </summary>
        public OrderedDictionary<ChamberID, Chamber> Chambers { get; set; }

        /// <summary>
        /// 地表の凹凸データ
        /// </summary>
        public GroundSurfaceData Surface { get; set; }

        public ColonyData(Cell[,] cells, Chamber[] chamberArray, GroundSurfaceData surface)
        {
            Cells = cells;
            Width = Cells.GetLength(0);
            Height = Cells.GetLength(1);

            Chambers = new();
            foreach (var chamber in chamberArray)
            {
                Chambers[chamber.ID] = chamber;
            }
            Surface = surface;
        }

        /// <summary>
        /// 指定位置のセルをoutで取得する。戻り値は成否
        /// </summary>
        public bool TryGetCell(int x, int y, [NotNullWhen(true)] out Cell? cell)
        {
            if ((uint)x < (uint)Width && (uint)y < (uint)Height)
            {
                cell = Cells[x, y];
                return true;
            }

            cell = null;
            return false;
        }
    }

    public interface IColonyDataPeek
    {
        public Cell[,] Cells { get; }
        public OrderedDictionary<ChamberID, Chamber> Chambers { get; }
        public GroundSurfaceData Surface { get; }
    }
}