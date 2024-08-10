using Omoch.Animations;
using Omoch.Framework.ButtonGroup;
using TMPro;
using UnityEngine;

namespace AntColony.Game.Hud
{
    public class HudSpeedButtonView : MonoButtonView
    {
        [SerializeField] private TextMeshProUGUI text;
        public void SetSpeed(int speed)
        {
            text.text = $"x{speed}";
        }

        public override void Draw(IButtonPeek peek)
        {
            float scale = Mathf.Lerp(1f, 1.2f, Easing.InOutCubic(peek.DownPercent));
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}