using UnityEngine;

namespace AntColony.Game.Gestures
{
    public struct GestureInput
    {
        public GestureInputKind Kind { get; set; }
        public GestureInputInfo Info { get; set; }

        public static GestureInput TouchDown(Vector2 position)
        {
            return new GestureInput
            {
                Kind = GestureInputKind.TouchDown,
                Info = new GestureInputInfo
                {
                    TouchDown = new GestureInputInfoTouchDown(position),
                },
            };
        }

        public static GestureInput TouchUp()
        {
            return new GestureInput
            {
                Kind = GestureInputKind.TouchUp,
                Info = new GestureInputInfo
                {
                    TouchUp = new GestureInputInfoTouchUp(),
                },
            };
        }

        public static GestureInput TouchMove(Vector2 position)
        {
            return new GestureInput
            {
                Kind = GestureInputKind.TouchMove,
                Info = new GestureInputInfo
                {
                    TouchMove = new GestureInputInfoTouchMove(position),
                },
            };
        }

        public static GestureInput WheelZoom(float value)
        {
            return new GestureInput
            {
                Kind = GestureInputKind.WheelZoom,
                Info = new GestureInputInfo
                {
                    WheelZoom = new GestureInputInfoWheelZoom(value),
                },
            };
        }

        public static GestureInput PinchStart()
        {
            return new GestureInput
            {
                Kind = GestureInputKind.PinchStart,
                Info = new GestureInputInfo
                {
                    PinchStart = new GestureInputInfoPinchStart(),
                },
            };
        }

        public static GestureInput PinchMove(float scaleFactor)
        {
            return new GestureInput
            {
                Kind = GestureInputKind.PinchMove,
                Info = new GestureInputInfo
                {
                    PinchMove = new GestureInputInfoPinchMove(scaleFactor),
                },
            };
        }
    }

    public enum GestureInputKind : int
    {
        TouchDown = 1,
        TouchUp = 2,
        TouchMove = 3,
        WheelZoom = 4,
        PinchStart = 5,
        PinchMove = 6,
    }

    public struct GestureInputInfo
    {
        public GestureInputInfoTouchDown TouchDown { get; set; }
        public GestureInputInfoTouchUp TouchUp { get; set; }
        public GestureInputInfoTouchMove TouchMove { get; set; }
        public GestureInputInfoWheelZoom WheelZoom { get; set; }
        public GestureInputInfoPinchStart PinchStart { get; set; }
        public GestureInputInfoPinchMove PinchMove { get; set; }
    }

    public struct GestureInputInfoTouchDown
    {
        public Vector2 Position { get; set; }
        public GestureInputInfoTouchDown(Vector2 position)
        {
            Position = position;
        }
    }

    public struct GestureInputInfoTouchUp
    {
    }

    public struct GestureInputInfoTouchMove
    {
        public Vector2 Position { get; set; }
        public GestureInputInfoTouchMove(Vector2 position)
        {
            Position = position;
        }
    }

    public struct GestureInputInfoWheelZoom
    {
        public float Value { get; set; }
        public GestureInputInfoWheelZoom(float value)
        {
            Value = value;
        }
    }

    public struct GestureInputInfoPinchStart
    {
    }

    public struct GestureInputInfoPinchMove
    {
        public float ScaleFactor { get; set; }
        public GestureInputInfoPinchMove(float scaleFactor)
        {
            ScaleFactor = scaleFactor;
        }
    }
}