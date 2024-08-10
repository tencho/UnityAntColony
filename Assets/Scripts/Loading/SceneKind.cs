using System;

#nullable enable

namespace AntColony.Loading
{
    public enum SceneKind : int
    {
        Title = 1,
        Game = 2,
    }

    public static class SceneKindExtensions
    {
        public static string ToAddressKey(this SceneKind kind)
        {
            return kind switch
            {
                SceneKind.Game => "GameScene",
                SceneKind.Title => "TitleScene",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
