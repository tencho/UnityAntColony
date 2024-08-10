using Omoch.Display;
using Omoch.Framework;
using UnityEngine;

namespace AntColony.Title
{
    public class TitleSceneView
        : MonoViewBaseWithInput<IGameScenePeek, GameSceneInput>
        , IGameSceneViewOrder
    {
        [SerializeField] private OmochBinder binder;
        [SerializeField] private ScreenResizeDetector screenResizeDetector;
        [SerializeField] private SpriteRenderer background;

        private void Start()
        {
            binder.BindViewWithInput<IGameScenePeek, IGameSceneViewOrder, GameSceneInput>(this, LinkKey.TitleScene);
            screenResizeDetector.OnResize.AddListener(OnScreenResize);
            OnScreenResize(screenResizeDetector.ScreenInfo);
        }

        private void OnScreenResize(ScreenInfo screen)
        {
            background.size = screen.DisplaySize;
        }
    }

    public interface IGameSceneViewOrder
    {

    }
}
