using System;
using System.Collections.Generic;
using Omoch.Geom;
using Omoch.Randoms;
using UnityEngine;

#nullable enable

namespace AntColony.Game.Colonies.Builders
{
    /// <summary>
    /// コロニーを構成する際に主に交差判定に使う通路データ
    /// </summary>
    public class ColonyPathway : Line2Group<PathwayLine>
    {
        /// <summary>通路を分割する際の断片の基準サイズ</summary>
        private const int SegmentBaseSize = 5;

        public ColonyPathway() : base()
        {
        }

        /// <summary>
        /// 線上のランダムな座標を取得する
        /// </summary>
        public Vector2 GetRandomPointOnLine()
        {
            if (Lines.Count == 0)
            {
                throw new Exception("線分が1つもありません");
            }
            var line = Randomizer.Pick(Lines);
            return line.Lerp(Randomizer.NextFloat());
        }

        /// <summary>
        /// 引数の通路データを、既存の通路にぶつかるまで追加し続ける
        /// </summary>
        public void AddPathway(ColonyPathway pathway)
        {
            var newLines = new List<PathwayLine>();
            foreach (PathwayLine line in pathway.Lines)
            {
                var intersections = TryGetIntersections(line);
                if (intersections.Length == 0)
                {
                    // 既存の通路と交差しなければそのまま追加
                    newLines.Add(line);
                }
                else
                {
                    // 既存の通路と交差したら交差点までのラインを追加して終了
                    newLines.Add(new PathwayLine(line.Start, intersections[0], line.Thisness, line.IsDug));
                    break;
                }
            }
            Lines.AddRange(newLines);
        }

        /// <summary>
        /// startからendにかけてランダムに曲がった通路を生成する
        /// </summary>
        public void Create(Vector2 start, Vector2 end, float bendAngle, int thickness, bool isDug)
        {
            // 直線距離から分割数を求める
            var straightLine = end - start;
            var straightSize = straightLine.magnitude;
            var numSegments = Mathf.Max(1, Mathf.CeilToInt(straightSize / SegmentBaseSize));
            var segmentSize = straightSize / numSegments;

            var rotation = 0f;
            var point = Vector2.zero;
            for (int i = 0; i < numSegments; i++)
            {
                var randomRotation = Mathf.Lerp(-bendAngle, bendAngle, Randomizer.NextFloat());
                rotation += randomRotation * Mathf.Deg2Rad;
                var delta = new Vector2(segmentSize * Mathf.Cos(rotation), segmentSize * Mathf.Sin(rotation));
                Add(new PathwayLine(point, point + delta, thickness, isDug));
                point += delta;
            }
            Rotate(Mathf.Atan2(straightLine.y, straightLine.x) - Mathf.Atan2(point.y, point.x), Vector2.zero);
            var scale = straightSize / point.magnitude;
            Scale(scale, scale);
            Translate(start.x, start.y);
        }

        public void Clamp(Size2Int size, MarginInt padding)
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];
                line.Start = new Vector2(
                    Mathf.Clamp(line.Start.x, padding.Left, size.Width - 1 - padding.Right),
                    Mathf.Clamp(line.Start.y, padding.Bottom, size.Height - 1 - padding.Top)
                );
                line.End = new Vector2(
                    Mathf.Clamp(line.End.x, padding.Left, size.Width - 1 - padding.Right),
                    Mathf.Clamp(line.End.y, padding.Bottom, size.Height - 1 - padding.Top)
                );
                Lines[i] = line;
            }
        }
    }
}