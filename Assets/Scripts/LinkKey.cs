using Omoch.Framework;

namespace AntColony
{
    public class LinkKey
    {
        public static readonly LinkID TitleScene = LinkID.From("TitleScene");
        public static readonly LinkID GameScene = LinkID.From("GameScene");
        public static readonly LinkID Gesture = LinkID.From("Camera");
        public static readonly LinkID Colony = LinkID.From("Colony");
        public static readonly LinkID GameCamera = LinkID.From("GameCamera");
        public static readonly LinkID StatusWindow = LinkID.From("StatusWindow");
        public static readonly LinkID HudButtonGroup = LinkID.From("HudButtonGroup");
        public static LinkID Ant(int index) => LinkID.From($"Ant_{index}");
        public static LinkID Item(int index) => LinkID.From($"Item_{index}");
    }
}