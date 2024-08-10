using System.Collections.Generic;
using UnityEngine;

namespace AntColony.Game.Backgrounds
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CombinedGrassView : MonoBehaviour
    {
        private void Start()
        {
            // 草メッシュを全て統合してdrawCallを減らす
            var combines = new List<CombineInstance>();
            foreach (MeshFilter filter in GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null)
                {
                    continue;
                }

                combines.Add(new CombineInstance
                {
                    mesh = filter.sharedMesh,
                    transform = filter.transform.localToWorldMatrix
                });
                filter.gameObject.SetActive(false);
            }

            var combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combines.ToArray());
            if (TryGetComponent(out MeshFilter meshFilter))
            {
                meshFilter.sharedMesh = combinedMesh;
            }
        }
    }
}
