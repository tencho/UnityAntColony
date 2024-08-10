using System;
using AntColony.Game.Colonies;
using Omoch.Display;
using Omoch.Framework;
using Omoch.Framework.ButtonGroup;
using VContainer;

#nullable enable

namespace AntColony.Game.Hud
{
    public class HudLogic
        : LogicBaseWithInput<IHudViewOrder, HudInput>
        , IHudPeek
    {
        [Inject] private readonly OmochBinder binder = null!;
        [Inject] private readonly ColonyLogic colony = null!;
        [Inject] private readonly ScreenResizeDetector screenResizeDetector = null!;

        private readonly ButtonGroupLogic<HudButtonKind> buttonGroup;
        private readonly int[] speedLevels;
        private int speedLevelIndex;
        public int Speed => colony.Speed;

        public HudLogic()
        {
            speedLevels = new int[] { 0, 1, 5, 20 };
            speedLevelIndex = 1;
            buttonGroup = new ButtonGroupLogic<HudButtonKind>();
            buttonGroup.OnClick += OnButtonClickHandler;
            buttonGroup.Add(HudButtonKind.Speed);
        }

        public void AfterInject()
        {
            binder.BindLogicWithInput<
                    IHudPeek,
                    IHudViewOrder,
                    HudInput
                >(this, LinkKey.StatusWindow);
            binder.BindLogicWithInput<
                    IButtonGroupPeek<HudButtonKind>,
                    IButtonGroupViewOrder<HudButtonKind>,
                    ButtonGroupInput<HudButtonKind>
                >(buttonGroup, LinkKey.HudButtonGroup);
        }

        public override void Initialized()
        {
            ViewOrder.Resize(screenResizeDetector.ScreenInfo);
            screenResizeDetector.OnResize.AddListener(ViewOrder.Resize);
        }

        private void OnButtonClickHandler(HudButtonKind kind)
        {
            switch (kind)
            {
                case HudButtonKind.Speed:
                    speedLevelIndex = (speedLevelIndex + 1) % speedLevels.Length;
                    colony.Speed = speedLevels[speedLevelIndex];
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public int GetItemCount(HudItemKind kind)
        {
            return kind switch
            {
                HudItemKind.Ant => colony.TotalAnts,
                HudItemKind.Egg => colony.TotalEggs,
                HudItemKind.Food => colony.TotalFoods,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public interface IHudPeek
    {
        public int Speed { get; }

        public int GetItemCount(HudItemKind kind);
    }
}