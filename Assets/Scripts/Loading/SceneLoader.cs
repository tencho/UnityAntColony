using System;
using System.Collections;
using Omoch.Animations;
using Omoch.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace AntColony.Loading
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private UiRing progressRing;
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Camera canvasCamera;
        [SerializeField] private SceneKind currentSceneKind;

        /// <summary>シーンの読み込みのみで何%まで進行するか。残りの%はシーン初期化後の処理分になる。</summary>
        private const float MaxSceneLoadPercent = 0.95f;

        private bool isLoading;
        private AsyncOperationHandle<SceneInstance> currentSceneHandle;
        private FloatTracker progressTracker;
        private float sceneInitPercent;

        public Camera CanvasCamera => canvasCamera;
        public static SceneLoader Instance { get; private set; }

        private void Awake()
        {
            // 破棄されないシングルトン
            if (Instance is null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            sceneInitPercent = 0f;
            isLoading = false;
            Application.targetFrameRate = 30;
            progressTracker = new FloatTracker(1f, 1f, 4f);
            ApplyProgress(1f);
        }

        /// <summary>
        /// シーン読み込み後の初期化処理が進んだ割合を指定する
        /// </summary>
        public void SetSceneInitPercent(float percent)
        {
            sceneInitPercent = percent;
        }

        /// <summary>
        /// 次のシーンへ遷移
        /// </summary>
        public void LoadScene(SceneKind scene)
        {
            if (isLoading)
            {
                throw new Exception("現在ロード中です。多重ロードはできません。");
            }

            isLoading = true;
            sceneInitPercent = 0f;
            StartCoroutine(LoadSceneCoroutine(scene));
        }

        private IEnumerator LoadSceneCoroutine(SceneKind scene)
        {
            if (currentSceneHandle.IsValid() && currentSceneKind != scene)
            {
                Addressables.Release(currentSceneHandle);
            }
            currentSceneKind = scene;

            // フェードイン演出開始
            animator.SetTrigger(LoadingAnimationPhase.FadeIn);
            ApplyProgress(0f);

            // フェードイン演出完了待ち
            while (true)
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                if (state.shortNameHash == LoadingAnimationPhase.FadeIn && state.normalizedTime >= 1f)
                {
                    break;
                }
                yield return null;
            }

            animator.SetTrigger(LoadingAnimationPhase.Loading);

            // シーンの非同期読み込み開始
            currentSceneHandle = Addressables.LoadSceneAsync(scene.ToAddressKey(), LoadSceneMode.Single);
            progressTracker.JumpTo(0f);
            ;
            float totalPercent;
            do
            {
                float percent = currentSceneHandle.GetDownloadStatus().Percent;
                // シーンの読み込みとシーン初期化直後の処理を足して100%になるようにする
                totalPercent = percent * MaxSceneLoadPercent + sceneInitPercent * (1f - MaxSceneLoadPercent);
                progressTracker.MoveTo(totalPercent);
                progressTracker.AdvanceTime(Time.deltaTime);
                ApplyProgress(progressTracker.Current);
                yield return null;
            }
            // IsDone=trueかつpercent=0のケースがあるので、アニメーションがスキップされないようpercent≒1もチェック
            while (!(currentSceneHandle.IsDone && totalPercent >= 1f - Mathf.Epsilon && progressTracker.Completed));

            // フェードアウト演出開始
            animator.SetTrigger(LoadingAnimationPhase.FadeOut);
            isLoading = false;
        }

        private void ApplyProgress(float percent)
        {
            var percent100 = Mathf.RoundToInt(percent * 100);
            progressText.text = $"{percent100}%";
            progressRing.StartAngle = 0f;
            progressRing.EndAngle = percent * 360f;
            progressRing.Refresh();
        }
    }
}
