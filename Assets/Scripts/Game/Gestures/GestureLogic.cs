using System;
using AntColony.Game.Colonies.Structures;
using AntColony.Settings;
using Omoch.Animations;
using Omoch.Framework;
using UnityEngine;
using VContainer;

#nullable enable

namespace AntColony.Game.Gestures
{
    public class GestureLogic
        : LogicBaseWithInput<IGestureViewOrder, GestureInput>
        , IGesturePeek
        , IUpdatableLogic
    {
        private const float ScalePow = 1.5f;
        private const float ScaleLevelMin = -2f;
        private const float ScaleLevelMax = 6f;

        private readonly FloatTracker scaleLevel;
        private bool isTouchMoved;
        private Vector2 touchDownPosition;
        private float pinchStartScale;
        private bool wheelEnabled;
        private Vector2 previousPosition;

        public float Scale { get; set; }
        public event Action<Vector2>? OnMove;
        public event Action<Vector2>? OnClick;

        [Inject] private readonly OmochBinder binder = null!;
        [Inject] private readonly ColonySetting setting = null!;
        [Inject] private readonly GameSceneLogicReferences references = null!;

        public GestureLogic()
        {
            touchDownPosition = Vector2.zero;
            previousPosition = Vector2.zero;
            scaleLevel = new FloatTracker(0.3f, 1f, 4f);
            scaleLevel.JumpTo(0f);
            Scale = Mathf.Pow(ScalePow, scaleLevel.Current);
            wheelEnabled = false;
        }

        public void AfterInject()
        {
            binder.BindLogicWithInput<IGesturePeek, IGestureViewOrder, GestureInput>(this, LinkKey.Gesture);
        }

        public override void Initialized()
        {
            Input.OnInput += OnInput;
        }

        private void OnInput(GestureInput input)
        {
            switch (input.Kind)
            {
                case GestureInputKind.TouchDown:
                    isTouchMoved = false;
                    previousPosition = touchDownPosition = input.Info.TouchDown.Position;
                    break;

                case GestureInputKind.TouchUp:
                    if (!isTouchMoved)
                    {
                        // タッチ位置のスクリーン座標をコロニー座標に変換する
                        var colonySize = setting.ColonySize;
                        Vector2 localPoint = (Vector2)references.ColonyRoot.InverseTransformPoint(touchDownPosition);
                        localPoint /= ColonyConsts.SizePerCell;
                        localPoint.x += colonySize.Width / 2f;
                        localPoint.y += colonySize.Height;
                        OnClick?.Invoke(localPoint);
                    }
                    break;

                case GestureInputKind.TouchMove:
                    isTouchMoved = true;
                    Vector2 moved = input.Info.TouchMove.Position - previousPosition;
                    moved /= ColonyConsts.SizePerCell * Scale;
                    previousPosition = input.Info.TouchMove.Position;
                    OnMove?.Invoke(moved);
                    break;

                case GestureInputKind.WheelZoom:
                    wheelEnabled = true;
                    var level = scaleLevel.Destination + ((input.Info.WheelZoom.Value > 0) ? 1f : -1f);
                    scaleLevel.MoveTo(Mathf.Clamp(level, ScaleLevelMin, ScaleLevelMax));
                    break;

                case GestureInputKind.PinchStart:
                    wheelEnabled = false;
                    pinchStartScale = Scale;
                    break;

                case GestureInputKind.PinchMove:
                    var pinchScale = input.Info.PinchMove.ScaleFactor * pinchStartScale;
                    Scale = Mathf.Clamp(pinchScale, Mathf.Pow(ScalePow, ScaleLevelMin), Mathf.Pow(ScalePow, ScaleLevelMax));
                    break;
            }
        }

        public void UpdateLogic()
        {
            if (wheelEnabled)
            {
                scaleLevel.AdvanceTime(Time.deltaTime);
                Scale = Mathf.Pow(ScalePow, scaleLevel.Current);
            }
        }
    }

    public interface IGesturePeek
    {
    }
}