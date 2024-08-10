using Omoch.Geom;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AntColony.Game.Hud
{
    public class HudItemCountView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI count;

        private void Start()
        {
            // アイコンのリサイズ
            Vector2 frameSize = icon.rectTransform.sizeDelta;
            Vector2 imageSize = icon.sprite.textureRect.size;
            Vector2 resized = RectTools.ResizeVector2(imageSize, frameSize, ResizeMode.Contain);
            icon.rectTransform.sizeDelta = resized;
        }

        public void SetCount(int number)
        {
            count.text = number.ToString();
        }
    }
}

