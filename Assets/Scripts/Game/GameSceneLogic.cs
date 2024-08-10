using System;
using AntColony.Game.Gestures;
using AntColony.Game.Colonies;
using AntColony.Game.Colonies.Builders;
using AntColony.Loading;
using Omoch.Framework;
using AntColony.Game.Hud;
using AntColony.Game.Colonies.Finders;
using AntColony.Game.Cameras;
using Cysharp.Threading.Tasks;
using Omoch.Randoms;
using VContainer;

namespace AntColony.Game
{
    public class GameSceneLogic
        : MonoLogicBaseWithInput<IGameSceneViewOrder, GameSceneInput>
        , IGameScenePeek
    {
        [Inject] private readonly OmochBinder binder;
        [Inject] private readonly GestureLogic gesture;
        [Inject] private readonly ColonyLogic colony;
        [Inject] private readonly GameCameraLogic gameCamera;
        [Inject] private readonly HudLogic hud;
        [Inject] private readonly ColonyBuilder colonyBuilder;

        public IColonyPeek Colony { get => colony; }

        private void Awake()
        {
            var randomSeed = Environment.TickCount;
            Randomizer.Init(randomSeed);

            binder.BindLogicWithInput<IGameScenePeek, IGameSceneViewOrder, GameSceneInput>(this, LinkKey.GameScene);
        }

        private async void Start()
        {
            await UniTask.WaitUntil(() => IsInitialized);

            colony.AfterInject();
            gameCamera.AfterInject();
            gesture.AfterInject();
            hud.AfterInject();

            OnDispose += () =>
            {
                colony.Dispose();
                gameCamera.Dispose();
                gesture.Dispose();
                hud.Dispose();
            };

            // コロニーデータの非同期生成
            var colonyData = await colonyBuilder.BuildAsync();
            colony.Begin(colonyData);

            // 埋まった通路を最短経路検索しておく
            await colony.PathFinder.FindPathAllAsync(PathFindMode.Shortest);
            await colony.PathFinder.FindPathAllAsync(PathFindMode.Detour);

            // 初期化処理完了
            SceneLoader.Instance.SetSceneInitPercent(1f);
        }
    }

    public interface IGameScenePeek
    {
        public IColonyPeek Colony { get; }
    }
}