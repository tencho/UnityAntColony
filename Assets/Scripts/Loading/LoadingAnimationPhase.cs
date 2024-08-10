#nullable enable

using UnityEngine;

namespace AntColony.Loading
{
    public class LoadingAnimationPhase
    {
        public static readonly int FadeIn = Animator.StringToHash("FadeIn");
        public static readonly int FadeOut = Animator.StringToHash("FadeOut");
        public static readonly int Loading = Animator.StringToHash("Loading");
    }
}