using Omoch.Collections;
using Omoch.Display;
using Omoch.Framework;
using Omoch.Framework.ButtonGroup;
using UnityEngine;
using VContainer;

namespace AntColony.Game.Hud
{
    public class HudView
        : MonoViewBaseWithInput<IHudPeek, HudInput>
        , IHudViewOrder
        , IUpdatableView
    {
        [Inject] private readonly OmochBinder binder = null!;

        /// <summary>
        /// 統計数Prefab
        /// </summary>
        [SerializeField] private SerializableDictionary<HudItemKind, HudItemCountView> statsIcons;

        /// <summary>
        /// ButtonGroupView用ボタン
        /// </summary>
        [SerializeField] private SerializableDictionary<HudButtonKind, MonoButtonView> buttons;

        /// <summary>
        /// 端末のセーフエリアとなる矩形
        /// </summary>
        [SerializeField] private RectTransform safeArea;

        private ButtonGroupView<HudButtonKind> buttonGroup;
        private HudSpeedButtonView speedButton;

        public void Awake()
        {
            binder.BindViewWithInput<IHudPeek, IHudViewOrder, HudInput>(this, LinkKey.StatusWindow);

            buttonGroup = new ButtonGroupView<HudButtonKind>();
            binder.BindViewWithInput<
                IButtonGroupPeek<HudButtonKind>,
                IButtonGroupViewOrder<HudButtonKind>,
                ButtonGroupInput<HudButtonKind>
            >(buttonGroup, LinkKey.HudButtonGroup);
        }

        public override void Initialized()
        {
            foreach (HudButtonKind buttonKind in buttons.Keys)
            {
                buttonGroup.Add(buttons[buttonKind], buttonKind);
            }
            speedButton = (HudSpeedButtonView)buttons[HudButtonKind.Speed];
        }

        public void UpdateView()
        {
            foreach (HudItemKind kind in statsIcons.Keys)
            {
                statsIcons[kind].SetCount(Peek.GetItemCount(kind));
            }

            speedButton.SetSpeed(Peek.Speed);
        }

        public void Resize(ScreenInfo screen)
        {
            safeArea.offsetMin = new Vector2(screen.UnsafePadding.Left, screen.UnsafePadding.Bottom);
            safeArea.offsetMax = new Vector2(-screen.UnsafePadding.Right, -screen.UnsafePadding.Top);
        }
    }

    public interface IHudViewOrder
    {
        /// <summary>
        /// 画面リサイズ
        /// </summary>
        public void Resize(ScreenInfo screen);
    }
}
