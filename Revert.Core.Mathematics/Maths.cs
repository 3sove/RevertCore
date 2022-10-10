using Revert.Core.Mathematics.Constraints;
using Revert.Core.Mathematics.Matrices;
using Revert.Port.LibGDX.Mathematics.Interpolations;
using Revert.Port.LibGDX.Mathematics.Vectors;
using System;
using System.Collections.Generic;

namespace Revert.Core.Mathematics
{
    public class Maths
    {
        private static DateTimeOffset dto = new DateTimeOffset(DateTime.Now);
        private static int seed = (int)(dto.ToUnixTimeMilliseconds() ^ (dto.ToUnixTimeMilliseconds() << 32));
        public static Random random = new Random(seed);
        private static float sqrt2 = (float)Math.Sqrt(2);
        public static float FLOAT_ROUNDING_ERROR = 0.000001f; // 32 bits
        public static float PI2 = (float)(Math.PI * 2f);
        public static float radiansToDegrees = (float)(180f / Math.PI);
        public static float degreesToRadians = 1f / radiansToDegrees; // (float)(180f / Math.PI);

        public static bool isEqual(float a, float b)
        {
            return Math.Abs(a - b) <= FLOAT_ROUNDING_ERROR;
        }

        public static bool isEqual(float a, float b, float tolerance)
        {
            return Math.Abs(a - b) <= tolerance;
        }

        /** Returns true if the value is zero (using the default tolerance as upper bound) */
        public static bool isZero(float value)
        {
            return Math.Abs(value) <= FLOAT_ROUNDING_ERROR;
        }

        /// <summary>
        /// Returns true if the value is zero.
        /// </summary>
        /// <param name="value">value to compare</param>
        /// <param name="tolerance">represent an upper bound below which the value is considered zero</param>
        public static bool isZero(float value, float tolerance)
        {
            return Math.Abs(value) <= tolerance;
        }

        public static double pow(float value, float power)
        {
            return Math.Pow(value, power);
        }

        public static double pow(double value, double power)
        {
            return Math.Pow(value, power);
        }

        public static double[][][] Sigmoid(double[][][] input)
        {
            var output = new double[input.Length][][];
            for (int layer = 0; layer < input.Length; layer++)
                output[layer] = Sigmoid(input[layer]);
            return output;
        }

        public static double[][] Sigmoid(double[][] input)
        {
            var output = new double[input.Length][];
            for (int row = 0; row < input.Length; row++)
                output[row] = Sigmoid(input[row]);
            return output;
        }

        public static double[] Sigmoid(double[] input)
        {
            var output = new double[input.Length];
            for (int item = 0; item < input.Length; item++)
                output[item] = 1 / (1 + Math.Pow(Math.E, -input[item]));
            return output;
        }

        public static double[][][] ReLU(double[][][] input)
        {
            var output = new double[input.Length][][];
            for (int layer = 0; layer < input.Length; layer++)
                output[layer] = ReLU(input[layer]);
            return output;
        }

        public static double[][] ReLU(double[][] input)
        {
            var output = new double[input.Length][];
            for (int row = 0; row < input.Length; row++)
                output[row] = ReLU(input[row]);
            return output;
        }

        public static double[] ReLU(double[] input)
        {
            var output = new double[input.Length];

            for (int item = 0; item < input.Length; item++)
            {
                var value = input[item];
                output[item] = value < 0.0 ? 0.0 : value;
            }

            return output;
        }

        public static T[][] CreateJagged<T>(int rows, int columns, Func<int, int, T> generator)
        {
            var jagged = new T[rows][];

            for (var i = 0; i < rows; i++)
            {
                jagged[i] = new T[columns];
                for (int j = 0; j < jagged.Length; j++)
                {
                    jagged[i][j] = generator(i, j);
                }
            }

            return jagged;
        }

        public static T[][][] CreateJagged<T>(int layers, int rows, int columns)
        {
            var jagged = new T[layers][][];
            for (int layer = 0; layer < layers; layer++)
                jagged[layer] = CreateJagged<T>(rows, columns);
            return jagged;
        }

        public static T[][] CreateJagged<T>(int rows, int columns)
        {
            var jagged = new T[rows][];

            for (var i = 0; i < rows; i++)
                jagged[i] = new T[columns];

            return jagged;
        }
        
        public static float nextFloat()
        {
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            // choose -149 instead of -126 to also generate subnormal floats (*)
            double exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }

        public static double CrossEntropy(int batch_size, double[][] A, double[][] B)
        {
            int m = B.Length;
            int n = B[0].Length;
            double[][] z = CreateJagged<double>(m, n);

            for (int row = 0; row < m; row++)
                for (int column = 0; column < n; column++)
                    z[row][column] = (A[row][column] * Math.Log(B[row][column])) + ((1 - A[row][column]) * Math.Log(1 - B[row][column]));

            double sum = 0;
            for (int row = 0; row < m; row++)
                for (int column = 0; column < n; column++)
                    sum += z[row][column];

            return -sum / batch_size;
        }

        public static float getSqrt2()
        {
            return sqrt2;
        }

        public static double[] softMax(Matrix values)
        {
            double[] result = new double[values.Value.Length];
            double sum = 0.0;
            for (int i = 0; i < values.Value.Length; i++)
                for (int j = 0; j < values.Value[i].Length; j++)
                    sum += Math.Exp(values.Value[i][j]);

            for (int i = 0; i < values.Value.Length; i++)
                for (int j = 0; j < values.Value[i].Length; j++)
                    result[i] = Math.Exp(values.Value[i][j]) / sum;

            return result;
        }

        public static double[] softMax(double[][] values)
        {
            double[] result = new double[values.Length];
            double sum = 0.0;
            for (int i = 0; i < values.Length; i++)
                for (int j = 0; j < values[i].Length; j++)
                    sum += Math.Exp(values[i][j]);

            for (int i = 0; i < values.Length; i++)
                for (int j = 0; j < values[i].Length; j++)
                    result[i] = Math.Exp(values[i][j]) / sum;

            return result;
        }

        public static float[] getProbabilityArray(int count)
        {
            float[] probabilities = new float[count];
            float defaultValue = 1f / count;
            int i = 0;

            for (int length = probabilities.Length; i < length; ++i)
            {
                probabilities[i] = defaultValue;
            }

            return probabilities;
        }

        public static float edgeAwareAdd(float value, float valueToAdd, float min, float max)
        {
            float range = max - min;
            float shiftedValue = value - min;

            float shiftedSum;
            for (shiftedSum = shiftedValue + valueToAdd; shiftedSum < min; shiftedSum += range)
            {
            }

            shiftedSum %= range;
            return min + shiftedSum;
        }

        public static bool randomBoolean(float probability)
        {
            return random.Next() < probability;
        }

        public static float distance(float fromX, float fromY, float toX, float toY)
        {
            return (float)Math.Sqrt(Math.Pow(toX - fromX, 2.0) + Math.Pow(toY - fromY, 2.0));
        }

        public static int randomIndex(int max, Interpolation interpolation)
        {
            return randomIndex(0, max, interpolation);
        }

        public static int randomIndex(int min, int max)
        {
            return randomIndex(min, max, Interpolation.linear);
        }

        public static int randomIndex(int min, int max, Interpolation interpolation)
        {
            float fmin = min - 0.5F + float.MinValue;
            float fmax = max + 0.5F - float.MinValue;
            float floatValue = fmin + interpolation.apply(nextFloat()) * (fmax - fmin);
            int value = (int)Math.Round(floatValue);
            return value >= min && value <= max ? value : value;
        }

        public static float randomFloat(float max, Interpolation interpolation)
        {
            return randomFloat(0f, max, interpolation);
        }

        public static float randomFloat(float min, float max, Interpolation interpolation)
        {
            return min + interpolation.apply(nextFloat()) * (max - min);
        }

        public static float randomFloat(Constraint<float> constraint)
        {
            return randomFloat(constraint.Minimum, constraint.Maximum);
        }

        public static float randomFloat(Interpolation interpolation)
        {
            return interpolation.apply(nextFloat());
        }

        public static float randomFloat()
        {
            return nextFloat();
        }

        public static float randomFloat(float min, float max)
        {
            return min + nextFloat() * (max - min);
        }


        public static ulong randomULong(ulong min, ulong max)
        {
            if (max <= min)
                throw new ArgumentOutOfRangeException("max", "max must be > min!");

            ulong uRange = (ulong)(max - min);

            //Prevent a modolo bias; see https://stackoverflow.com/a/10984975/238419 for more information.
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (ulongRand % uRange) + min;
        }

        public static ulong randomULong(ulong max)
        {
            return randomULong(0, max);
        }

        public static ulong randomULong()
        {
            return randomULong(ulong.MinValue, ulong.MaxValue);
        }


        public static long randomLong(long min, long max)
        {
            if (max <= min)
                throw new ArgumentOutOfRangeException("max", "max must be > min!");

            ulong uRange = (ulong)(max - min);

            //Prevent a modolo bias; see https://stackoverflow.com/a/10984975/238419 for more information.
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }

        public static long randomLong(long max)
        {
            return randomLong(0, max);
        }

        public static long randomLong()
        {
            return randomLong(long.MinValue, long.MaxValue);
        }


        public static int randomInt(int min, int max)
        {
            return random.Next(min, max);
        }

        public static int randomInt(float min, float max)
        {
            return random.Next((int)min, (int)max);
        }

        public static int randomInt(float min, float max, Interpolation interpolation)
        {
            return (int)Math.Round(randomFloat(min, max, interpolation));
        }

        public static float mean(int a, int b)
        {
            return (a + b) / 2.0F;
        }

        public static float mean(float a, float b)
        {
            return (a + b) / 2.0F;
        }

        public static float mean(float a, float b, float c)
        {
            return (a + b + c) / 3.0F;
        }

        public static float average(float a, float b, float impact)
        {
            float average = (a + b) / 2.0F;
            float difference = average - a;
            return a + difference * impact;
        }

        public static float screen(float a, float b)
        {
            return 1f - (1f - a) * (1f - b);
        }

        public static float screen(float a, float b, float impact)
        {
            return 1f - (1f - a) * (1f - b) * impact;
        }

        public static List<Vector2> getRandomMatrixLocations(int width, int height)
        {
            List<Vector2> locations = new List<Vector2>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    locations.Add(new Vector2(x, y));
                }
            }

            Shuffle(locations);
            return locations;
        }

        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static float[][] invert(float[][] matrix)
        {
            int y = 0;

            for (int var3 = matrix.Length; y < var3; ++y)
            {
                float[] row = matrix[y];
                int x = 0;

                for (int var6 = row.Length; x < var6; ++x)
                {
                    row[x] = 1 - row[x];
                }
            }

            return matrix;
        }

        public static float angle(float fromX, float fromY, float toX, float toY)
        {
            Vector2 from = new Vector2(fromX, fromY);
            Vector2 to = new Vector2(toX, toY);
            return angle(from, to);
        }

        public static float angle(Vector2 from, Vector2 to)
        {
            return (float)(57.29577951308232D * Math.Atan2(to.x - from.x, from.y - to.y));
        }

        public static float[][] scale(float[][] map, int width, int height)
        {
            float[][] newArray = CreateJagged<float>(height, width);
            int minX = Math.Min(map.GetLength(0), newArray.GetLength(0));
            int minY = Math.Min(map.GetLength(1), newArray.GetLength(1));

            for (int i = 0; i < minY; ++i)
                Array.Copy(map, i * map.GetLength(0), newArray, i * newArray.GetLength(0), minX);

            return newArray;
        }


        public static short clamp(short value, short min, short max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static long clamp(long value, long min, long max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static double clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

    }
}
