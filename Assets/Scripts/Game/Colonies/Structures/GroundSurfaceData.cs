using System;
using Omoch.Animations;
using Omoch.Noises;
using Omoch.Randoms;
using UnityEngine;

namespace AntColony.Game.Colonies.Structures
{
    public class GroundSurfaceData
    {
        private const int NumSegments = 1024;
        private const float EntranceDepth = 0.5f;
        private const float EntranceRange = 0.035f;

        private readonly float[] depths;
        public float Width { get; }
        private float X { get; }

        public GroundSurfaceData(float x, float width, float depth)
        {
            Width = width;
            X = x;

            var noise = new PerlinNoise1(3, Randomizer.Next());
            depths = new float[NumSegments + 1];
            for (var i = 0; i < depths.Length; i++)
            {
                float percent = (float)i / (depths.Length - 1);
                float entranceWeight = Math.Clamp(1f - Math.Abs(percent - 0.5f) / EntranceRange, 0f, 1f);
                float noiseDepth = noise.GetNoise(percent * 30) * depth;
                depths[i] = Mathf.Lerp(noiseDepth, EntranceDepth, Easing.InOutCubic(entranceWeight));
            }
        }

        public float GetDepth(float px)
        {
            float ratio = (px - X) / Width;
            return GetDepthByRatio(ratio);
        }

        public float GetDepthByRatio(float ratio)
        {
            float floatIndex = ratio * depths.Length;
            int index = Mathf.FloorToInt(floatIndex);
            if (index < 0)
            {
                return depths[0];
            }
            else if (index >= depths.Length - 1)
            {
                return depths[^1];
            }
            float decimalIndex = floatIndex % 1;
            return Mathf.Lerp(depths[index], depths[index + 1], decimalIndex);
        }
    }
}