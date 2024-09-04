namespace AntColony.Game.Colonies.Items
{
    /// <summary>
    /// 複数のItemKindをビット演算で組み合わせたもの
    /// </summary>
    public struct ItemKindMask
    {
        private readonly int value;
        private ItemKindMask(int value) => this.value = value;

        public static ItemKindMask Create(params ItemKind[] kinds)
        {
            var value = 0;
            foreach (ItemKind kind in kinds)
            {
                value |= (int)kind;
            }
            return new ItemKindMask(value);
        }

        public bool Has(ItemKind kind)
        {
            return (value & (int)kind) == (int)kind;
        }
    }
}