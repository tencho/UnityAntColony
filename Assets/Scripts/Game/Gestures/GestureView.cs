using Omoch.Framework;
using Omoch.Inputs;
using UnityEngine;
using VContainer;

namespace AntColony.Game.Gestures
{
    public class GestureView
        : ViewBaseWithInput<IGesturePeek, GestureInput>
        , IGestureViewOrder
        , IUpdatableView
    {
        private Vector2 previousTouchPoint;
        private bool isTouchDown;
        private bool isPinching;
        private float guideLength;

        [Inject] private readonly OmochBinder binder = null!;
        [Inject] private readonly GameSceneViewReferences references = null!;

        public GestureView()
        {
            previousTouchPoint = new Vector2(float.MaxValue, float.MaxValue);
            isTouchDown = false;
            isPinching = false;
        }

        public void AfterInject()
        {
            binder.BindViewWithInput<IGesturePeek, IGestureViewOrder, GestureInput>(this, LinkKey.Gesture);
        }

        public void UpdateView()
        {
            float wheelScroll = GameInputSystem.GetWheelScroll();
            if (wheelScroll != 0f)
            {
                Input.Invoke(GestureInput.WheelZoom(wheelScroll));
            }

            Vector2[] screenPoints = GameInputSystem.GetTouchPositions();
            var touchCount = screenPoints.Length;
            if (touchCount > 0)
            {
                Vector2 touchPoint = references.mainCamera.ScreenToWorldPoint(screenPoints[0]);
                if (
                    !isTouchDown
                    && touchCount == 1
                    && GameInputSystem.IsTouchDown()
                    && !IsTouchIgnoreRect(screenPoints[0])
                )
                {
                    isTouchDown = true;
                    previousTouchPoint = touchPoint;
                    Input.Invoke(GestureInput.TouchDown(touchPoint));
                }

                if (isTouchDown && previousTouchPoint != touchPoint)
                {
                    previousTouchPoint = touchPoint;
                    Input.Invoke(GestureInput.TouchMove(touchPoint));
                }
            }

            if (isTouchDown && GameInputSystem.IsTouchUp())
            {
                isTouchDown = false;
                Input.Invoke(GestureInput.TouchUp());
            }

            if (screenPoints.Length >= 2)
            {
                if (!isPinching)
                {
                    isPinching = true;
                    isTouchDown = false;
                    guideLength = Vector2.Distance(screenPoints[0], screenPoints[1]);
                    Input.Invoke(GestureInput.PinchStart());
                }
                else
                {
                    float pinchScaleFactor = Vector2.Distance(screenPoints[0], screenPoints[1]) / guideLength;
                    Input.Invoke(GestureInput.PinchMove(pinchScaleFactor));
                }
            }
            else if (isPinching)
            {
                isPinching = false;
            }
        }

        private bool IsTouchIgnoreRect(Vector2 screenPoint)
        {
            foreach (RectTransform ignoreRect in references.ignoreClickRects)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(ignoreRect, screenPoint, references.canvasCamera, out var canvasPoint))
                {
                    if (ignoreRect.rect.Contains(canvasPoint))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public interface IGestureViewOrder
    {
    }
}