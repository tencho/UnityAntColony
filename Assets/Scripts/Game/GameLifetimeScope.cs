using AntColony.Game.Backgrounds;
using AntColony.Game.Cameras;
using AntColony.Game.Colonies;
using AntColony.Game.Colonies.Builders;
using AntColony.Game.Gestures;
using AntColony.Game.Hud;
using AntColony.Settings;
using Omoch.Display;
using Omoch.Framework;
using UnityEngine;
using VContainer;
using VContainer.Unity;

#nullable enable

namespace AntColony.Game
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private ColonySetting setting = null!;
        [SerializeField] private GameSceneLogicReferences logicReferences = null!;
        [SerializeField] private GameSceneViewReferences viewReferences = null!;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<OmochBinder>();
            builder.RegisterComponentInHierarchy<ScreenResizeDetector>();
            builder.RegisterComponentInHierarchy<GroundSurfaceShape>();

            builder.RegisterInstance(setting);
            builder.RegisterInstance(logicReferences);
            builder.RegisterInstance(viewReferences);

            builder.Register<ColonyBuilder>(Lifetime.Singleton);
            builder.Register<ColonyLogic>(Lifetime.Singleton);
            builder.Register<GestureLogic>(Lifetime.Singleton);
            builder.Register<GameCameraLogic>(Lifetime.Singleton);
            builder.Register<HudLogic>(Lifetime.Singleton);

            builder.Register<ColonyView>(Lifetime.Singleton);
            builder.Register<ColonyWallView>(Lifetime.Singleton);
            builder.Register<GestureView>(Lifetime.Singleton);
            builder.Register<GameCameraView>(Lifetime.Singleton);
        }
    }
}