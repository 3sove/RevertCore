using System;
using System.Linq;
using System.Threading.Tasks;

namespace Revert.Core.Mathematics.Matrices
{
    public class Matrix3
    {
        public double[][][] Value { get; }

        public Matrix3(int rows, int columns, int layers)
        {
            Value = new double[layers][][];

            for (var layer = 0; layer < layers; layer++)
            {
                Value[layer] = new double[rows][];
                for (var row = 0; row < rows; row++)
                    Value[layer][row] = new double[columns];
            }
        }

        public Matrix3(double[][][] array)
        {
            Value = array;
        }

        public Matrix3(double[][] oneWideLayers)
        {
            Value = new double[oneWideLayers.Length][][];

            for (var layer = 0; layer < oneWideLayers.Length; layer++)
            {
                for (var x = 0; x < oneWideLayers[layer].Length; x++)
                    Value[layer][x] = new[] { oneWideLayers[layer][x] };
            }
        }

        /// <summary>
        /// Returns a Matrix of dimensions mXn, with values between 0.0 and 1.0
        /// </summary>
        /// <param name="m">Rows</param>
        /// <param name="n">Columns</param>
        /// <returns></returns>
        public static Matrix3 random(int m, int n)
        {
            var values = Maths.CreateJagged<double>(m, n);
            for (int row = 0; row < values.Length; row++)
                for (int column = 0; column < values[row].Length; column++)
                    values[row][column] = Maths.random.NextDouble();
            return new Matrix3(values);
        }

        public int MaxIndex(int rank = 0)
        {
            double maxValue = 0.0;
            int maxIndex = 0;
            for (int k = 0; k < Value.Length; k++)
                for (int m = 0; m < Value[k].Length; m++)
                    for (int n = 0; n < Value[k][m].Length; n++)
                        if (Value[k][m][n] > maxValue)
                        {
                            maxValue = Value[k][m][n];

                            switch (rank)
                            {
                                case 2:
                                    maxIndex = n;
                                    break;
                                case 1:
                                default:
                                    maxIndex = m;
                                    break;
                                case 0:
                                    maxIndex = k;
                                    break;
                            }
                        }
            return maxIndex;
        }

        public double[] SoftMax(int layer)
        {
            return Maths.softMax(Value[layer]);
        }

        public Matrix3 Sigmoid()
        {
            return new Matrix3(Maths.Sigmoid(Value));
        }

        public Matrix3 ReLU()
        {
            return new Matrix3(Maths.ReLU(Value));
        }

        public double Sum()
        {
            double sum = 0.0;
            for (int layer = 0; layer < Value.Length; layer++)
                for (int row = 0; row < Value[layer].Length; row++)
                    for (int column = 0; column < Value[layer][row].Length; column++)
                        sum += Value[layer][row][column];
            return sum;
        }

        public Matrix3 Pow(double exponent)
        {
            for (int layer = 0; layer < Value.Length; layer++)
                for (int row = 0; row < Value[layer].Length; row++)
                    for (int column = 0; column < Value[layer][row].Length; column++)
                        Value[layer][row][column] *= Value[layer][row][column];
            return this;
        }

        public Matrix3 Mean()
        {
            var matrix = new Matrix3(Value.Length, Value[0].Length, 1);
            for (int layer = 0; layer < Value.Length; layer++)
                for (int row = 0; row < Value[layer].Length; row++)
                    for (int column = 0; column < Value[layer][row].Length; column++)
                        matrix.Value[layer][row][0] += Value[layer][row][column];

            for (int layer = 0; layer < Value.Length; layer++)
                for (int row = 0; row < matrix.Value.Length; row++)
                    matrix.Value[layer][row][0] /= Value[layer][row].Length;

            return matrix;
        }

        public void Initialize(Func<double> initializer)
        {
            for (int layer = 0; layer < Value.Length; layer++)
                for (var row = 0; row < Value.Length; row++)
                    for (var column = 0; column < Value[row].Length; column++)
                        Value[layer][row][column] = initializer();
        }

        public static Matrix3 operator -(Matrix3 a, Matrix3 b)
        {
            var newMatrix = Maths.CreateJagged<double>(a.Value.Length, a.Value[0].Length, a.Value[0][0].Length);

            for (int layer = 0; layer < a.Value.Length; layer++)
                for (var row = 0; row < a.Value[layer].Length; row++)
                    for (var column = 0; column < a.Value[layer][row].Length; column++)
                        newMatrix[layer][row][column] = a.Value[layer][row][column] - b.Value[layer][row][column];

            return new Matrix3(newMatrix);
        }

        public static Matrix3 operator +(Matrix3 a, Matrix3 b)
        {
            var newMatrix = Maths.CreateJagged<double>(a.Value.Length, a.Value[0].Length, a.Value[0][0].Length);

            for (int layer = 0; layer < a.Value.Length; layer++)
                for (var row = 0; row < a.Value[layer].Length; row++)
                    for (var column = 0; column < a.Value[layer][row].Length; column++)
                        newMatrix[layer][row][column] = a.Value[layer][row][column] + b.Value[layer][row][column];

            return new Matrix3(newMatrix);
        }

        public static Matrix3 operator +(Matrix3 a, double b)
        {
            var newMatrix = Maths.CreateJagged<double>(a.Value.Length, a.Value[0].Length, a.Value[0][0].Length);

            for (int layer = 0; layer < a.Value.Length; layer++)
                for (var row = 0; row < a.Value[layer].Length; row++)
                    for (var column = 0; column < a.Value[layer][row].Length; column++)
                        newMatrix[layer][row][column] = a.Value[layer][row][column] + b;

            return new Matrix3(newMatrix);
        }

        public static Matrix3 operator -(double a, Matrix3 m)
        {
            var newMatrix = Maths.CreateJagged<double>(m.Value.Length, m.Value[0].Length, m.Value[0][0].Length);

            for (int layer = 0; layer < m.Value.Length; layer++)
                for (var row = 0; row < m.Value[layer].Length; row++)
                    for (var column = 0; column < m.Value[layer][row].Length; column++)
                        newMatrix[layer][row][column] = a - m.Value[layer][row][column];

            return new Matrix3(newMatrix);
        }

        public static Matrix3 operator -(Matrix3 m, double a)
        {
            var newMatrix = Maths.CreateJagged<double>(m.Value.Length, m.Value[0].Length, m.Value[0][0].Length);

            for (int layer = 0; layer < m.Value.Length; layer++)
                for (var row = 0; row < m.Value[layer].Length; row++)
                    for (var column = 0; column < m.Value[layer][row].Length; column++)
                        newMatrix[layer][row][column] = m.Value[layer][row][column] - a;

            return new Matrix3(newMatrix);
        }

        public static Matrix3 GetHadamardProduct(Matrix3 a, Matrix3 b)
        {
            if (a.Value.Length != b.Value.Length || a.Value[0].Length != b.Value[0].Length)
                throw new ArgumentException($"Hadamard Products on {a.Value.Length}x{a.Value[0].Length} * {b.Value.Length}x{b.Value[0].Length} matrices are invalid.  Hadamard Products can only be performed on matrices of the same size.");

            var outputMatrix = new Matrix3(a.Value.Length, a.Value[0].Length, a.Value[0][0].Length);
            GetHadamardProduct(a, b, outputMatrix);
            return outputMatrix;
        }

        public static void GetHadamardProduct(Matrix3 a, Matrix3 b, Matrix3 outputMatrix)
        {
            if (a.Value.Length != b.Value.Length || a.Value[0].Length != b.Value[0].Length) //Hadamard Product
                throw new ArgumentException($"Hadamard Products on {a.Value.Length}x{a.Value[0].Length} * {b.Value.Length}x{b.Value[0].Length} matrices are invalid.  Hadamard Products can only be performed on matrices of the same size.");

            var values = outputMatrix.Value;

            Parallel.For(0, a.Value.Length, layer =>
            {
                Parallel.For(0, a.Value[layer].Length, row =>
                {
                    for (var column = 0; column < a.Value[layer][row].Length; column++)
                    {
                        values[layer][row][column] = a.Value[layer][row][column] * b.Value[layer][row][column];
                    }
                });
            });
        }

        public static Matrix3 GetHadamardQuotient(Matrix3 a, Matrix3 b)
        {
            if (a.Value.Length != b.Value.Length || a.Value[0].Length != b.Value[0].Length)
                throw new ArgumentException($"Hadamard Quotients on {a.Value.Length}x{a.Value[0].Length} * {b.Value.Length}x{b.Value[0].Length} matrices are invalid.  Hadamard Products can only be performed on matrices of the same size.");

            var outputMatrix = new Matrix3(a.Value.Length, a.Value[0].Length, a.Value[0][0].Length);
            GetHadamardQuotient(a, b, outputMatrix);
            return outputMatrix;
        }

        public static void GetHadamardQuotient(Matrix3 a, Matrix3 b, Matrix3 outputMatrix)
        {
            if (a.Value.Length != b.Value.Length || a.Value[0].Length != b.Value[0].Length) //Hadamard Quotient
                throw new ArgumentException($"Hadamard Quotients on {a.Value.Length}x{a.Value[0].Length} * {b.Value.Length}x{b.Value[0].Length} matrices are invalid.  Hadamard Products can only be performed on matrices of the same size.");

            var values = outputMatrix.Value;

            Parallel.For(0, a.Value.Length, layer =>
            {
                Parallel.For(0, a.Value[layer].Length, row =>
                {
                    for (var column = 0; column < a.Value[layer][row].Length; column++)
                    {
                        values[layer][row][column] = a.Value[layer][row][column] / b.Value[layer][row][column];
                    }
                });
            });
        }

        public static Matrix3 operator *(Matrix3 a, Matrix3 b)
        {
            if (a.Value.Length == b.Value.Length && a.Value[0].Length == b.Value[0].Length && a.Value[0][0].Length == b.Value[0][0].Length)
                return GetHadamardProduct(a, b);

            if (a.Value.Length != b.Value.Length)
                throw new ArgumentException("3 dimensional matrix (layer X row X column) multiplication requires for the number of layers within each Matrix to be the same.");

            var newMatrix = Maths.CreateJagged<double>(a.Value.Length, a.Value[0].Length, a.Value[0][0].Length);

            for (int layer = 0; layer < a.Value.Length; layer++)
            {
                if (a.Value[0].Length == b.Value.Length) //Matrix Product
                {
                    var length = a.Value[0].Length;

                    Parallel.For(0, a.Value.Length, i =>
                    {
                        for (var j = 0; j < b.Value[0].Length; j++)
                        {
                            var temp = 0.0;

                            for (var k = 0; k < length; k++)
                                temp += a.Value[layer][i][k] * b.Value[layer][k][j];

                            newMatrix[layer][i][j] = temp;
                        }
                    });
                }
                else
                {
                    throw new Exception($"Multiplication of these matrices is undefined ({a.Value.Length}x{a.Value[0].Length}x{a.Value[0][0].Length} * {b.Value.Length}x{b.Value[0].Length}x{b.Value[0][0].Length}).  LxAxB * LxAxB = LxAxB is (Hadamard Product) and LxAxB * LxBxC = LxAxC is (Matrix Product), but not LxAxB * LxAxC or LxAxB * LxCxD.");
                }
            }

            return new Matrix3(newMatrix);
        }

        public static Matrix3 operator /(Matrix3 a, Matrix3 b)
        {
            if (a.Value.Length == b.Value.Length && a.Value[0].Length == b.Value[0].Length && a.Value[0][0].Length == b.Value[0][0].Length)
                return GetHadamardQuotient(a, b);

            if (a.Value.Length != b.Value.Length)
                throw new ArgumentException("3 dimensional matrix (layer X row X column) multiplication requires for the number of layers within each Matrix to be the same.");

            var newMatrix = Maths.CreateJagged<double>(a.Value.Length, a.Value[0].Length, a.Value[0][0].Length);

            for (int layer = 0; layer < a.Value.Length; layer++)
            {
                if (a.Value[0].Length == b.Value.Length)
                {
                    var length = a.Value[0].Length;

                    Parallel.For(0, a.Value.Length, i =>
                    {
                        for (var j = 0; j < b.Value[0].Length; j++)
                        {
                            var temp = 0.0;

                            for (var k = 0; k < length; k++)
                                temp += a.Value[layer][i][k] / b.Value[layer][k][j];

                            newMatrix[layer][i][j] = temp;
                        }
                    });
                }
                else
                {
                    throw new Exception($"Division of these matrices is undefined ({a.Value.Length}x{a.Value[0].Length}x{a.Value[0][0].Length} * {b.Value.Length}x{b.Value[0].Length}x{b.Value[0][0].Length}).  LxAxB / LxAxB = LxAxB is (Hadamard Quotient) and LxAxB / LxBxC = LxAxC is (Matrix Quotient), but not LxAxB / LxAxC or LxAxB / LxCxD.");
                }
            }

            return new Matrix3(newMatrix);
        }

        public static Matrix3 operator *(double scalar, Matrix3 m)
        {
            var newMatrix = Maths.CreateJagged<double>(m.Value.Length, m.Value[0].Length, m.Value[0][0].Length);

            for (var layer = 0; layer < m.Value.Length; layer++)
                for (var row = 0; row < m.Value.Length; row++)
                    for (var column = 0; column < m.Value[row].Length; column++)
                        newMatrix[layer][row][column] = m.Value[layer][row][column] * scalar;

            return new Matrix3(newMatrix);
        }

        public static Matrix3 operator *(Matrix3 m, double scalar)
        {
            return scalar * m;
        }

        public static Matrix3 operator /(double scalar, Matrix3 m)
        {
            var newMatrix = Maths.CreateJagged<double>(m.Value.Length, m.Value[0].Length, m.Value[0][0].Length);

            for (var layer = 0; layer < m.Value.Length; layer++)
                for (var row = 0; row < m.Value.Length; row++)
                    for (var column = 0; column < m.Value[row].Length; column++)
                        newMatrix[layer][row][column] = scalar / m.Value[layer][row][column];

            return new Matrix3(newMatrix);
        }

        public static Matrix3 operator /(Matrix3 m, double scalar)
        {
            var newMatrix = Maths.CreateJagged<double>(m.Value.Length, m.Value[0].Length, m.Value[0][0].Length);

            for (var layer = 0; layer < m.Value.Length; layer++)
                for (var row = 0; row < m.Value.Length; row++)
                    for (var column = 0; column < m.Value[row].Length; column++)
                        newMatrix[layer][row][column] = m.Value[layer][row][column] / scalar;

            return new Matrix3(newMatrix);
        }

        public Matrix3 Transpose()
        {
            var newMatrix = Maths.CreateJagged<double>(Value.Length, Value[0].Length, Value[0][0].Length); //rows --> cols, cols --> rows

            for (var layer = 0; layer < Value.Length; layer++)
            {
                for (var row = 0; row < Value[layer].Length; row++)
                    for (var col = 0; col < Value[layer][row].Length; col++)
                        newMatrix[layer][col][row] = Value[layer][row][col];
            }

            return new Matrix3(newMatrix);
        }

        public override string ToString()
        {
            int columns = Value[0].Length;

            if (columns == 1)
                columns = (int)Math.Sqrt(Value.Length);

            int printedColumns = 0;

            for (int layerIndex = 0; layerIndex < Value.Length; layerIndex++)
            {
                Console.WriteLine($"Layer {layerIndex} / {Value.Length}");

                double[][] layer = Value[layerIndex];
                foreach (var row in layer)
                {
                    foreach (var column in row)
                    {
                        if (column == 0.0)
                            Console.Write(" ");
                        else if (column < 0.3)
                            Console.Write(".");
                        else if (column < 0.6)
                            Console.Write(";");
                        else
                            Console.Write("#");

                        printedColumns++;
                        if (printedColumns % columns == 0)
                        {
                            Console.WriteLine();
                        }
                    }
                }
            }

            Console.WriteLine();
            return base.ToString();
        }
    }
}
