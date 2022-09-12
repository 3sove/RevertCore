using Revert.Core.Mathematics.Geometry;
using Revert.Core.Mathematics.Interpolations;
using Revert.Core.Mathematics.Matrices;
using Revert.Core.Mathematics.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Revert.Core.Mathematics
{
    public static class MathsExtensions
    {
        public static float[] flatten(this IEnumerable<Vector2> vectors)
        {
            float[] items = new float[vectors.Count() * 2];
            int i = 0;
            foreach (var vectorItem in vectors)
            {
                items[i * 2] = vectorItem.x;
                items[i * 2 + 1] = vectorItem.y;
                i++;
            }
            return items;
        }


        public static int numberOfTrailingZeros(this int value)
        {
            if (value == 0)
            {
                return 32;
            }
            else
            {
                int var2 = 31;
                int var1 = value << 16;
                if (var1 != 0)
                {
                    var2 -= 16;
                    value = var1;
                }

                var1 = value << 8;
                if (var1 != 0)
                {
                    var2 -= 8;
                    value = var1;
                }

                var1 = value << 4;
                if (var1 != 0)
                {
                    var2 -= 4;
                    value = var1;
                }

                var1 = value << 2;
                if (var1 != 0)
                {
                    var2 -= 2;
                    value = var1;
                }

                return var2 - (int)(uint)(value << 1) >> 31;
            }
        }

        public static Rectangle getRectangle(this List<Vector2> vertices)
        {
            var top = -float.MaxValue;
            var bottom = float.MaxValue;
            var left = float.MaxValue;
            var right = -float.MaxValue;

            foreach (var point in vertices)
            {
                if (point.x < left) left = point.x;
                if (point.x > right) right = point.x;
                if (point.y < bottom) bottom = point.y;
                if (point.y > top) top = point.y;
            }

            return new Rectangle(left, bottom, right - left, top - bottom);
        }

        /// <summary>
        /// return the angle in degrees of this vector (point) relative to the given vector. Angles are towards the positive y-axis (typically counter-clockwise.) between -180 and +180
        /// </summary>
        public static float angle(this Vector2 source, Vector2 reference)
        {
            return (float)Math.Atan2(source.crs(reference), dot(source, reference)) * Maths.radiansToDegrees;
        }

        /// <summary>
        /// Calculates the 2D cross product between this and the given vector
        /// </summary>
        /// <param name="source">the origin vector</param>
        /// <param name="v">the other vector</param>
        /// <returns>the cross product</returns>
        public static float crs(this Vector2 source, Vector2 v)
        {
            return source.x * v.y - source.y * v.x;
        }

        //Calculates the 2D cross product between this and the given vector.
        public static float crs(this Vector2 source, float x, float y)
        {
            return source.x * y - source.y * x;
        }

        public static float dot(this Vector2 a, Vector2 b)
        {
            return Vector2.dot(a, b);
        }

        public static Vector2 sub(this Vector2 vector, Vector2 valueToSubtract)
        {
            vector.x -= valueToSubtract.x;
            vector.y -= valueToSubtract.y;
            return vector;
        }

        public static Vector2 sub(this Vector2 vector, float x, float y)
        {
            vector.x -= x;
            vector.y -= y;
            return vector;
        }

        public static Vector2 set(this Vector2 vector, Vector2 values)
        {
            vector.x = values.x;
            vector.y = values.y;
            return vector;
        }

        public static Vector2 set(this Vector2 vector, float x, float y)
        {
            vector.x = x;
            vector.y = y;
            return vector;
        }

        public static int Product(this List<int> factors)
        {
            var result = 1;
            foreach (var factor in factors) result *= factor;
            return result;
        }

        public static long Product(this List<long> factors)
        {
            long result = 1;
            foreach (var factor in factors) result *= factor;
            return result;
        }

        public static double Product(this List<double> factors)
        {
            var result = 1.0;
            foreach (var factor in factors) result *= factor;
            return result;
        }

        public static float Product(this List<float> factors)
        {
            float result = 1.0f;
            foreach (var factor in factors) result *= factor;
            return result;
        }



        public static Matrix toMatrix(this double[] value)
        {
            return new Matrix(value);
        }

        public static Matrix toMatrix(this double[][] value)
        {
            return new Matrix(value);
        }

        public static float relativePosition(this int value, int size)
        {
            if (size <= 0) return 0f;
            if (size == 1) return 1f;
            return (float)value / size;
        }


        public static int getIndex(this float value, float min, float max)
        {
            var range = max - min;
            return (int)Math.Round(min + range * value);
        }

        public static int getIndex(this float value, float[] array)
        {
            return (int)Math.Round(value * (array.Length - 1));
        }

        public static int getIndex<T>(this float value, T[] array)
        {
            return (int)Math.Round(value * (array.Length - 1));
        }

        public static int getIndex(this float value, float arraySize)
        {
            return (int)Math.Round(value * (arraySize - 1));
        }

        public static int getIndex(this float value, int arraySize)
        {
            return (int)Math.Round(value * (arraySize - 1));
        }


        public static float interpolate(this Interpolation interpolation, float from, float to, float a)
        {
            var range = to - from;
            return from + range * interpolation.apply(a);
        }

        public static float interpolate(this float from, float to, float a)
        {
            var range = to - from;
            return from + range * a;
        }

        public static float interpolate(this float from, float to, float a, float min, float max)
        {
            var range = to - from;// 100 - 20 = 80
            var edgeRange = max - min; //40 - 5 = 35
            var shiftedValue = from - min; //20 - 5 = 15;

            var valueToAdd = range * a; //80 * .5 = 40

            var shiftedSum = shiftedValue + valueToAdd; // 15 + 40 = 55
            while (shiftedSum < min) shiftedSum += range;
            shiftedSum %= edgeRange; //55 %= 35 = 20

            return min + shiftedSum; // 10 + 20 = 30
        }

        public static float sqrt(this int value)
        {
            return (float)Math.Sqrt(value);
        }

        public static float sqrt(this float value)
        {
            return (float)Math.Sqrt(value);
        }

        public static double sqrt(this double value)
        {
            return Math.Sqrt(value);
        }

        public static float distance(this float a, float b)
        {
            return Math.Abs(b - a);
        }

        public static float difference(this float a, float b)
        {
            return b - a;
        }

        public static float difference(this float a, float b, float edgeMin, float edgeMax)
        {
            var aToEdge = Math.Min(a - edgeMin, edgeMax - a); // 10 - 0 vs 100 - 10
            var bToEdge = Math.Min(b - edgeMin, edgeMax - b); // 50 - 0 vs 100 - 50

            var difference = b - a;

            if (Math.Abs(difference) < aToEdge + bToEdge) return difference;

            if (a < b)
                return -(aToEdge + bToEdge);
            else
                return aToEdge + bToEdge;
        }

        public static float relativePosition(this float position, float size)
        {
            if (size <= 0) return 0f;
            if (size == 1f) return 1f;
            else return position / size;
        }

        public static float relativePosition(this float position, int size)
        {
            if (size <= 0) return 0f;
            if (size == 1) return 1f;
            return position / size;
        }

        public static float relativePosition(this float position, float from, float to)
        {
            var range = to - from;
            return (position - from) / range;
        }

        //public static float relativePosition(this int position, int size)
        //{
        //    if (size <= 0) return 0f;
        //    if (size == 1) return 1f;
        //    return position / size;
        //}

        public static float relativePosition(this int position, int from, int to)
        {
            var range = (float)(to - from);
            return (position - from) / range;
        }

        public static int randomIndex(this float[] collection)
        {
            var probabilities = collection.ToArray();
            probabilities = probabilities.fillProbabilities();
            var value = Maths.randomFloat();
            var totalProbability = 0f;
            for (int i = 0; i < probabilities.Length; i++)
            {
                totalProbability += probabilities[i];
                if (totalProbability >= value) return i;
            }
            return 0;
        }

        public static int getWeightedIndex(float[] weights, float value)
        {
            var probabilities = weights.ToArray();
            probabilities = probabilities.fillProbabilities();
            var totalProbability = 0f;
            for (int i = 0; i < probabilities.Length; i++)
            {
                totalProbability += probabilities[i];
                if (totalProbability >= value) return i;
            }
            return 0;
        }

        public static T getRelative<T>(this T[] items, float relativePosition)
        {
            return items[relativePosition.getIndex(items)];
        }

        public static float[] relativeValues(this float[] items)
        {
            var relativeValues = new float[items.Length];
            var maxValue = items.Max();

            for (int i = 0; i < items.Length; i++)
            {
                var value = items[i];
                relativeValues[i] = value / maxValue;
            }
            return relativeValues;
        }

        public static int floor(this float value)
        {
            return (int)Math.Floor(value);
        }

        public static int ceiling(this float value)
        {
            return (int)Math.Ceiling(value);
        }

        public static float[] scale(this float[] values, float scalar)
        {
            var scaled = new float[(int)(values.Length * scalar)];

            for (int i = 0; i < values.Length; i++)
            {
                scaled[i] = (int)(values.Length * scalar);
            }

            for (int i = 0; i < scaled.Length; i++)
            {
                var fl = values[i];
                scaled[(int)(i / scalar)] += fl;
            }
            return scaled;
        }

        public static float getRelative(this float[] values, float relativePosition)
        {
            return values[relativePosition.getIndex(values)];
        }

        public static int vary(this int value, float maxSwing)
        {
            return (int)Maths.randomFloat(value - value * maxSwing, value + value * maxSwing);
        }

        public static float vary(this float value, float maxSwing)
        {
            return Maths.randomFloat(value - value * maxSwing, value + value * maxSwing);
        }

        public static float vary(this float value, float maxSwing, float floor, float ceiling)
        {
            return value.vary(maxSwing, Interpolation.linear, floor, ceiling, false);
        }

        public static int vary(this float value, float maxSwing, int floor, int ceiling)
        {
            return (int)value.vary(maxSwing, Interpolation.linear, floor, ceiling, false);
        }

        public static int vary(this int value, float maxSwing, int floor, int ceiling, bool edgeAware)
        {
            return value.vary(maxSwing, Interpolation.linear, floor, ceiling, edgeAware);
        }

        public static float add(this float value, float addend)
        {
            return value + addend;
        }

        public static float add(this float value, float addend, float floor, float ceiling, bool edgeAware)
        {
            var sum = value + addend;
            return sum.clamp(floor, ceiling, edgeAware);
        }

        public static float vary(this float value, float maxSwing, float floor, float ceiling, bool edgeAware)
        {
            return value.vary(maxSwing, Interpolation.linear, floor, ceiling, edgeAware);
        }

        public static float vary(this float value, float maxSwing, Interpolation interpolation, float floor, float ceiling, bool edgeAware)
        {
            var from = value * (1f - maxSwing * .5f);
            var to = value * (1f + maxSwing * .5f);

            if (edgeAware)
            {
                var result = Maths.randomFloat(from, to, interpolation);
                var size = ceiling - floor;
                return (size + result) % size;
            }

            if (from > ceiling) from = floor;
            if (from < floor) from = floor;
            if (to < floor) to = floor;
            if (to > ceiling) to = ceiling;


            return Maths.randomFloat(from, to, interpolation);
        }

        public static int vary(this int value, float maxSwing, Interpolation interpolation, int floor, int ceiling, bool edgeAware)
        {
            var from = (int)(value * (1f - maxSwing * .5f));
            var to = (int)(value * (1f + maxSwing * .5f));

            if (edgeAware)
            {
                var result = Maths.randomInt(from, to, interpolation);
                var size = ceiling - floor;
                return (size + result) % size;
            }

            if (from > ceiling) from = floor;
            if (from < floor) from = floor;
            if (to < floor) to = floor;
            if (to > ceiling) to = ceiling;

            return Maths.randomInt(from, to, interpolation);
        }

        public static float[] rotate(this float[] values, int steps)
        {
            var rotated = new float[values.Length];
            steps = (values.Length + steps) % values.Length;
            var tempIndex = 0;
            for (int i = 0; i < steps + values.Length; i++)
            {
                rotated[tempIndex++] = values[i % values.Length];
            }
            return rotated;
        }

        public static T[] rotate<T>(this T[] values, int steps)
        {
            var rotated = values.ToArray();
            steps = (values.Length + steps) % values.Length;

            var tempIndex = 0;
            for (int i = 0; i < steps + values.Length; i++)
            {
                rotated[tempIndex++] = values[i % values.Length];
            }
            return rotated;
        }

        public static float[] fillProbabilities(this float[] probabilities)
        {
            var totalProbability = 0f;
            var zeroes = 0;
            for (int i = 0; i < probabilities.Length; i++)
            {
                totalProbability += probabilities[i];
                if (probabilities[i] == 0f) zeroes++;
            }

            var difference = totalProbability.difference(1f);
            if (difference == 0f) return probabilities;

            float offset = 0;

            if (zeroes != 0)
            {
                offset = difference / zeroes;
                for (int i = 0; i < probabilities.Length; i++)
                {
                    if (probabilities[i] == 0f)
                    {
                        probabilities[i] = offset;
                    }
                }
                return probabilities;
            }

            offset = difference / probabilities.Length;
            for (int i = 0; i < probabilities.Length; i++)
                probabilities[i] += offset;

            return probabilities;
        }

        public static float[] setProbability(this float[] values, int index, float value)
        {
            var currentValue = values[index];
            var difference = currentValue.difference(value); // .5f,  .75f  = .25f
            var offset = difference / (values.Length - 1f); //don't offset the value which is being set

            for (int i = 0; i < values.Length; i++)
            {
                if (i == index) continue;
                values[i] -= offset;
            }

            values[index] = value;
            return values;
        }

        public static float pow(this float value, float exponent)
        {
            return (float)Math.Pow(value, exponent);
        }

        public static float pow(this int value, float exponent)
        {
            return (float)Math.Pow(value, exponent);
        }

        public static int clamp(this int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public static float clamp(this float value, float min, float max, bool edgeAware = false)
        {
            if (!edgeAware) return Math.Min(Math.Max(value, min), max);
            var range = max - min;
            return (range + value) % range;
        }

        public static bool inRange(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static double clamp(this double value, double min, double max)
        {
            return Math.Min(Math.Max(value, min), max);
        }


    }
}
