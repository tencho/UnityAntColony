namespace AntColony.Game.Colonies.Structures
{
    public class ColonyConsts
    {
        /// <summary>部屋生成時の重なりチェックでコロニーのセル解像度からこの倍率スケールダウンして低解像度で高速チェックする</summary>
        public const int ChamberGridScale = 4;

        /// <summary>コロニーのセル解像度(幅)</summary>
        //public const int Width = 200;//256

        /// <summary>コロニーのセル解像度(高さ)</summary>
        //public const int Height = 160;//200

        /// <summary>地上からこの深さまでは部屋を生成させない</summary>
        public const int NoChamberDepth = 40;//52

        /// <summary>セル1つの表示サイズ</summary>
        public const float SizePerCell = 10f;
    }
}