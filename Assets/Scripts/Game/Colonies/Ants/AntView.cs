using AntColony.Game.Colonies.Structures;
using AntColony.Settings;
using Omoch.Framework;
using UnityEngine;

namespace AntColony.Game.Colonies.Ants
{

    public class AntView : MonoViewBase<IAntPeek>, IAntViewOrder, IUpdatableView
    {
        [SerializeField] private ColonySetting setting;

        public override void Initialized()
        {
            // 女王蟻は大きくする
            if (Peek.Kind == AntKind.Queen)
            {
                transform.localScale *= 2f;
            }

            UpdateView();
        }

        public void UpdateView()
        {
            var position = transform.localPosition;
            position.x = (Peek.X - setting.ColonySize.Width / 2f) * ColonyConsts.SizePerCell;
            position.y = (Peek.Y - setting.ColonySize.Height) * ColonyConsts.SizePerCell;
            transform.localPosition = position;

            var rotation = Peek.Rotation * Mathf.Rad2Deg + 90f;
            transform.localEulerAngles = new Vector3(0f, 0f, rotation);
        }
    }

    public interface IAntViewOrder
    {
    }
}