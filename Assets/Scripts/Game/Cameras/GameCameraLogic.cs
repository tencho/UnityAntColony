using AntColony.Game.Colonies;
using AntColony.Game.Colonies.Ants;
using AntColony.Game.Gestures;
using AntColony.Settings;
using Omoch.Display;
using Omoch.Framework;
using Omoch.Geom;
using UnityEngine;
using VContainer;

#nullable enable

namespace AntColony.Game.Cameras
{
    public class GameCameraLogic
        : LogicBaseWithInput<IGameCameraViewOrder, GameCameraInput>
        , IGameCameraPeek
        , IUpdatableLogic
    {
        private Size2Int colonySize;

        /// <summary>追跡中の蟻</summary>
        private AntLogic? trackingAnt;

        public Vector2 CameraPosition { get; set; }
        public float CameraScale => gesture.Scale;

        [Inject] private readonly OmochBinder binder = null!;
        [Inject] private readonly ColonySetting setting = null!;
        [Inject] private readonly ColonyLogic colony = null!;
        [Inject] private readonly GestureLogic gesture = null!;
        [Inject] private readonly ScreenResizeDetector screenResizeDetector = null!;

        public GameCameraLogic()
        {
            trackingAnt = null;
            CameraPosition = Vector2.zero;
        }

        public void AfterInject()
        {
            binder.BindLogicWithInput<
                    IGameCameraPeek,
                    IGameCameraViewOrder,
                    GameCameraInput
                >(this, LinkKey.GameCamera);

            colonySize = setting.ColonySize;
            CameraPosition = new Vector2(colonySize.Width / 2f, colonySize.Height);
            gesture.OnClick += OnClickHandler;
            gesture.OnMove += OnMoveHandler;
        }

        private void OnMoveHandler(Vector2 moved)
        {
            trackingAnt = null;
            CameraPosition = new Vector2(
                Mathf.Clamp(CameraPosition.x - moved.x, 0f, colonySize.Width),
                Mathf.Clamp(CameraPosition.y - moved.y, 0f, colonySize.Height)
            );
        }

        private void OnClickHandler(Vector2 point)
        {
            // 蟻をクリックしたら追跡する
            if (colony.TryGetNearbyAnt(point, out AntLogic? ant))
            {
                trackingAnt = ant;
            }
        }

        public override void Initialized()
        {
            screenResizeDetector.OnResize.AddListener(ViewOrder.Resize);
            ViewOrder.Resize(screenResizeDetector.ScreenInfo);
        }

        public void UpdateLogic()
        {
            // 追跡中の蟻がいればカメラを蟻の位置にする
            if (trackingAnt is not null)
            {
                CameraPosition = new Vector2(trackingAnt.X, trackingAnt.Y);
            }
        }
    }

    public interface IGameCameraPeek
    {
        public Vector2 CameraPosition { get; }
        public float CameraScale { get; }
    }
}

