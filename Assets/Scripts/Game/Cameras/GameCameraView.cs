using AntColony.Game.Colonies.Structures;
using AntColony.Settings;
using Omoch.Display;
using Omoch.Framework;
using VContainer;

namespace AntColony.Game.Cameras
{
    public class GameCameraView
        : ViewBaseWithInput<IGameCameraPeek, GameCameraInput>
        , IGameCameraViewOrder
        , IUpdatableView
    {
        [Inject] private readonly GameSceneViewReferences references = null!;
        [Inject] private readonly ColonySetting setting = null!;
        [Inject] private readonly OmochBinder binder = null!;

        public GameCameraView()
        {
        }

        public void AfterInkect()
        {
            binder.BindViewWithInput<IGameCameraPeek, IGameCameraViewOrder, GameCameraInput>(this, LinkKey.GameCamera);
        }

        public void UpdateView()
        {
            var colonySize = setting.ColonySize;
            // カメラ位置反映
            var rootPosition = references.colonyRoot.localPosition;
            rootPosition.x = -Peek.CameraPosition.x + colonySize.Width / 2f;
            rootPosition.y = -Peek.CameraPosition.y + colonySize.Height;
            references.colonyRoot.localPosition = rootPosition * ColonyConsts.SizePerCell * Peek.CameraScale;
            var rootScale = references.colonyRoot.localScale;
            rootScale.Set(Peek.CameraScale, Peek.CameraScale, 1f);
            references.colonyRoot.localScale = rootScale;
        }

        public void Resize(ScreenInfo screen)
        {
            references.sky.SetSize(screen.DisplaySize);
        }
    }

    public interface IGameCameraViewOrder
    {
        public void Resize(ScreenInfo screen);
    }
}
