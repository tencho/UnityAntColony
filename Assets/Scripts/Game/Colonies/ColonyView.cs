using AntColony.Game.Backgrounds;
using AntColony.Game.Colonies.Ants;
using AntColony.Game.Colonies.Items;
using Omoch.Framework;
using UnityEngine;
using VContainer;

#nullable enable

namespace AntColony.Game.Colonies
{
    public class ColonyView
        : ViewBaseWithInput<IColonyPeek, ColonyInput>
        , IColonyViewOrder
    {
        [Inject] private readonly GroundSurfaceShape surfaceShape = null!;
        [Inject] private readonly GameSceneViewReferences references = null!;
        [Inject] private readonly ColonyWallView colonyWall = null!;
        [Inject] private readonly OmochBinder binder = null!;

        public ColonyView()
        {
        }

        public void AfterInject()
        {
            binder.BindViewWithInput<IColonyPeek, IColonyViewOrder, ColonyInput>(this, LinkKey.Colony);
            colonyWall.AfterInjedct();
            OnDispose += colonyWall.Dispose;
        }

        public void DrawSurface()
        {
            surfaceShape.CreateMesh(Peek.DataPeek.Surface, 1024, Color.black);
        }

        public void DrawWall()
        {
            colonyWall.Draw(Peek.DataPeek);
        }

        public void AddAnt(int linkIndex)
        {
            var ant = GameObject.Instantiate(references.antObject, references.antContainer);
            if (ant.TryGetComponent(out AntView antView))
            {
                binder.BindView<IAntPeek, IAntViewOrder>(antView, LinkKey.Ant(linkIndex));
            }
        }

        public void AddItem(int linkIndex)
        {
            var item = GameObject.Instantiate(references.itemObject, references.itemContainer);
            if (item.TryGetComponent(out ItemView itemView))
            {
                binder.BindViewWithInput<IItemPeek, IItemViewOrder, ItemInput>(itemView, LinkKey.Item(linkIndex));
            }
        }

        public void Dig(int x, int y)
        {
            colonyWall.Dig(x, y);
        }
    }

    public interface IColonyViewOrder
    {
        public void AddAnt(int linkIndex);
        public void AddItem(int linkIndex);
        public void Dig(int x, int y);
        public void DrawSurface();
        public void DrawWall();
    }
}