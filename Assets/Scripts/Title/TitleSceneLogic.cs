using System;
using AntColony.Loading;
using Omoch.Framework;
using UnityEngine;

namespace AntColony.Title
{
    public class TitleSceneLogic
        : MonoLogicBaseWithInput<IGameSceneViewOrder, GameSceneInput>
        , IGameScenePeek
    {
        [SerializeField] private OmochBinder binder;

        private void Start()
        {
            binder.BindLogicWithInput<IGameScenePeek, IGameSceneViewOrder, GameSceneInput>(this, LinkKey.TitleScene);
        }

        public override void Initialized()
        {
            Input.OnInput += OnInput;
        }

        private void OnInput(GameSceneInput input)
        {
            switch (input.Kind)
            {
                case TitleSceneInputKind.Click:
                    switch (input.Info.Click.Kind)
                    {
                        case TitleSceneInputClickKind.Background:
                            SceneLoader.Instance.LoadScene(SceneKind.Game);
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }

    public interface IGameScenePeek
    {
    }
}