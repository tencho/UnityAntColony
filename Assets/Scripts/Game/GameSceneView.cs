using AntColony.Game.Gestures;
using AntColony.Game.Colonies;
using Omoch.Framework;
using AntColony.Game.Cameras;
using VContainer;

namespace AntColony.Game
{
    public class GameSceneView
        : MonoViewBaseWithInput<IGameScenePeek, GameSceneInput>
        , IGameSceneViewOrder
    {
        [Inject] private readonly OmochBinder binder = null!;
        [Inject] private readonly ColonyView colony = null!;
        [Inject] private readonly GestureView gesture = null!;
        [Inject] private readonly GameCameraView gameCamera = null!;

        private void Awake()
        {
            binder.BindViewWithInput<IGameScenePeek, IGameSceneViewOrder, GameSceneInput>(this, LinkKey.GameScene);

            colony.AfterInject();
            gesture.AfterInject();
            gameCamera.AfterInkect();
        }
    }

    public interface IGameSceneViewOrder
    {
    }
}
