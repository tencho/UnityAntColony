using System.Collections.Generic;
using AntColony.Game.Colonies.Structures;
using Omoch.Noises;
using Omoch.Randoms;
using Omoch.Tools;
using UnityEngine;

namespace AntColony.Game.Backgrounds
{
    /// <summary>
    /// 地表の凸凹メッシュ
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class GroundSurfaceShape : MonoBehaviour
    {
        private const float BackgroundHeight = 80f;

        public void CreateMesh(GroundSurfaceData data, int numSegments, Color color)
        {
            if (!TryGetComponent(out MeshFilter meshFilter))
            {
                return;
            }

            var mesh = new Mesh();

            var backgoroundNoise = new PerlinNoise1(4, Randomizer.Next());

            var vertices = new List<Vector3>();
            for (var i = 0; i <= numSegments; i++)
            {
                var percent = (float)i / numSegments;
                var px = (percent - 0.5f) * data.Width * ColonyConsts.SizePerCell;
                var surfaceDepth = -data.GetDepthByRatio(percent) * ColonyConsts.SizePerCell;
                var backgoroundHeight = backgoroundNoise.GetNoise(percent * 20) * BackgroundHeight;
                vertices.Add(new Vector3(px, backgoroundHeight, 0));
                vertices.Add(new Vector3(px, surfaceDepth, 0));
            }

            var triangles = new List<int>();
            for (var i = 0; i < numSegments; i++)
            {
                var a = i * 2;
                var b = i * 2 + 1;
                var c = i * 2 + 2;
                var d = i * 2 + 3;
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);

                triangles.Add(c);
                triangles.Add(d);
                triangles.Add(b);
            }

            var colors = new Color[vertices.Count];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors = colors;

            if (meshFilter.sharedMesh != null)
            {
                GameObjectTools.SafeDestroy(meshFilter.sharedMesh);
            }
            meshFilter.sharedMesh = mesh;
        }
    }
}