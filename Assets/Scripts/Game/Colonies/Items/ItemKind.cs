namespace AntColony.Game.Colonies.Items
{
    /// <summary>
    /// アイテム種
    /// ビット演算で組み合わせて使うので2進数にしている
    /// </summary>
    public enum ItemKind : int
    {
        Dirt   = 0b00001,
        Food   = 0b00010,
        Egg    = 0b00100,
        Debris = 0b01000,
        Body   = 0b10000,
    }
}