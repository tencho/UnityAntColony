namespace AntColony.Title
{
    public class GameSceneInput
    {
        public TitleSceneInputKind Kind { get; set; }
        public TitleSceneInputInfo Info { get; set; }

        public static GameSceneInput Click(TitleSceneInputClickKind clickKind) => new()
        {
            Kind = TitleSceneInputKind.Click,
            Info = new TitleSceneInputInfo
            {
                Click = new TitleSceneInputInfoClick(TitleSceneInputClickKind.Background),
            },
        };
    }

    public enum TitleSceneInputKind : int
    {
        Click = 1,
    }

    public struct TitleSceneInputInfo
    {
        public TitleSceneInputInfoClick Click { get; set; }
    }

    public struct TitleSceneInputInfoClick
    {
        public TitleSceneInputClickKind Kind { get; set; }
        public TitleSceneInputInfoClick(TitleSceneInputClickKind kind)
        {
            Kind = kind;
        }
    }

    public enum TitleSceneInputClickKind : int
    {
        Background = 1,
    }
}