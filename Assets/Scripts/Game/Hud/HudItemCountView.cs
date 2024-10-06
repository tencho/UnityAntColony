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

        public void SetCount(int number)
        {
            count.text = number.ToString();
        }
    }
}

