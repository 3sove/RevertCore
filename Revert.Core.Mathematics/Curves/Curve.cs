using Revert.Core.Mathematics.Extensions;
using Revert.Core.Mathematics.Factories;
using Revert.Core.Mathematics.Vectors;
using System.Collections.Generic;

namespace Revert.Core.Mathematics.Curves
{
    public static class Curve
    {
        public static int MAX_INTERPOLATION_RANGE { get; } = 128;

        private static List<Vector2> getSegments(Vector2 prePoint, Vector2 startPoint, Vector2 endPoint, Vector2 postPoint, int numSegments)
        {
            var result = new List<Vector2>();
            var segmentPercentage = (1.0 / numSegments);

            for (int i = 1; i < numSegments; i++)
            {
                var value = segmentPercentage * (float)i;
                var next = getPointOnCurve(prePoint, startPoint, endPoint, postPoint, (float)value);
                result.Add(next);
            }

            //if (endPoint.y < 32) endPoint.y = 0;
            result.Add(endPoint);
            return result;
        }

        // Calculates interpolated point between two points using Catmull-Rom Spline
        // From: http://tehc0dez.blogspot.com/2010/04/nice-curves-catmullrom-spline-in-c.html
        private static Vector2 getPointOnCurve(Vector2 smooth1, Vector2 anchor1, Vector2 anchor2, Vector2 smooth2, float t)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var x = 0.5f * (2.0f * anchor1.x
                    + (-smooth1.x + anchor2.x) * t
                    + (2.0f * smooth1.x - 5.0f * anchor1.x + 4 * anchor2.x - smooth2.x) * t2
                    + (-smooth1.x + 3.0f * anchor1.x - 3.0f * anchor2.x + smooth2.x) * t3);

            var y = 0.5f * (2.0f * anchor1.y
                    + (-smooth1.y + anchor2.y) * t
                    + (2.0f * smooth1.y - 5.0f * anchor1.y + 4 * anchor2.y - smooth2.y) * t2
                    + (-smooth1.y + 3.0f * anchor1.y - 3.0f * anchor2.y + smooth2.y) * t3);

            return Vec2Factory.Instance.get(x, y);
        }

        public static List<Vector2> simplifyPoints(List<Vector2> points)
        {
            var v1 = Vec2Factory.Instance.get();
            var v2 = Vec2Factory.Instance.get();
            var v3 = Vec2Factory.Instance.get();
            var a1 = 0f;
            var a2 = 0f;
            var a3 = 0f;
            var i = 0;
            while (i < points.Count)
            {
                v1 = points[i];
                if (v3 == null)
                {
                    v3 = v1;
                    i++;
                    continue;
                }

                if (v2 == null)
                {
                    v2 = v1;
                    a2 = v2.angle(v1);
                    i++;
                    continue;
                }

                a1 = v1.angle(v2);

                if (a1 == a2 && a1 == a3)
                {
                    points.RemoveAt(i - 1);
                    i--;
                }

                v3 = v2;
                v2 = v1;

                a3 = a2;
                a2 = a1;
                i++;
            }
            return points;
        }

        public static List<Vector2> reducePoints(List<Vector2> points, float removalProbability)
        {
            var pointRectangle = points.getRectangle();

            var i = 0;
            while (i < points.Count)
            {
                var point = points[i];
                if (point.x == pointRectangle.x || point.x == pointRectangle.x + pointRectangle.width)
                {
                    i++;
                    continue;
                }
                if (point.y == pointRectangle.y || point.y == pointRectangle.y + pointRectangle.height)
                {
                    i++;
                    continue;
                }

                if (Maths.randomBoolean(removalProbability))
                {
                    points.RemoveAt(i);
                    i--;
                }
                i++;
            }

            return points;
        }

        public static List<Vector2> interpolate(List<Vector2> items, float alpha, int loopCount, bool interpolateEdges)
        {
            for (int i = 0; i < loopCount; i++)
            {
                Vector2 p1 = Vec2Factory.Instance.get();
                var p2 = Vec2Factory.Instance.get();
                foreach (var p3 in items)
                {
                    if (p1 != null && p2 != null)
                    {
                        if (interpolateEdges || p2.y != 0f && p2.x != 0f)
                            p2.set((p1.x + p2.x + p3.x) / 3, (p1.y + p2.y + p3.y) / 3);
                    }
                    p1 = p2;
                    p2 = p3;
                }
            }
            return items;
        }

        public static List<Vector2> constrainedInterpolate(this List<Vector2> vectors, int loopCount, bool interpolateEdges)
        {
            for (int i = 0; i < loopCount; i++)
            {
                var p1 = Vec2Factory.Instance.get();
                var p2 = Vec2Factory.Instance.get();
                foreach (var p3 in vectors)
                {
                    if (p1 != null && p2 != null)
                    {
                        if (Vector2.distance(p1, p2) < Curve.MAX_INTERPOLATION_RANGE && Vector2.distance(p2, p3) < Curve.MAX_INTERPOLATION_RANGE)
                            if (interpolateEdges || p2.y != 0f && p2.x != 0f)
                                p2.set((p1.x + p2.x + p3.x) / 3, (p1.y + p2.y + p3.y) / 3f);
                    }
                    p1 = p2;
                    p2 = p3;
                }
            }
            return vectors;
        }
    }
}
