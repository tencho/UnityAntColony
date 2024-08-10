using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable enable

namespace AntColony.Loading
{
    public class LoaderCameraAttacher : MonoBehaviour
    {
        private void Start()
        {
            // シーンを跨いで表示するロード画面用のCanvasカメラをシーンのBaseカメラへstackしなおしている
            var loaderCamera = SceneLoader.Instance.CanvasCamera;
            if (TryGetComponent(out UniversalAdditionalCameraData cameraData))
            {
                var cameraStack = cameraData.cameraStack;
                cameraStack.RemoveAll(camera => camera.name == loaderCamera.name);
                cameraStack.Add(loaderCamera);
            }
            else
            {
                throw new Exception("UniversalAdditionalCameraDataコンポーネントが見つかりません");
            }
        }
    }
}