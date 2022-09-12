using System;
using System.Linq;
using System.Threading.Tasks;

namespace Revert.Core.Mathematics.Matrices
{
    public class Matrix
    {
        public double[][] Value { get; }

        public Dimensions Dimensions { get; set; }

        public Matrix(int rows, int columns)
        {
            Value = new double[rows][];

            for (var i = 0; i < rows; i++)
                Value[i] = new double[columns];
            Dimensions = new Dimensions(columns, rows);
        }

        public Matrix(int rows, int columns, Func<double> valueInitializer)
        {
            Value = new double[rows][];

            for (var i = 0; i < rows; i++)
                Value[i] = new double[columns];

            for (var i = 0; i < rows; i++)
                for (var j = 0; j < columns; j++)
                    Value[i][j] = valueInitializer();

            Dimensions = new Dimensions(columns, rows);
        }

        public Matrix(double[][] array)
        {
            Value = array;
            Dimensions = new Dimensions(array[0].Length, array.Length);
        }

        public Matrix(double[] inputList, Dimensions dimensions)
        {
            Value = new double[inputList.Length][];

            for (var x = 0; x < Value.Length; x++)
                Value[x] = new[] { inputList[x] };

            Dimensions = dimensions;
        }

        public Matrix(double[] inputList)
        {
            Value = new double[inputList.Length][];

            for (var x = 0; x < Value.Length; x++)
                Value[x] = new[] { inputList[x] };

            Dimensions = new Dimensions(inputList.Length, 1);
        }

        public void SetValues(double[] input)
        {
            if (input.Length != Size) throw new ArgumentException($"Can't add {input.Length} values to a {Size} length Matrix of dimensions {Dimensions}.  Lengths must match.");

            int row = 0;
            int column = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (i != 0 && i % Dimensions.X == 0)
                {
                    row++;
                    column = 0;
                }

                Value[row][column] = input[i];
                column++;
            }
        }

        public Matrix flatten()
        {
            var flatValues = new double[Size];
            var index = 0;
            for (int i = 0; i < Value.Length; i++)
                for (int j = 0; j < Value[i].Length; j++)
                    flatValues[index++] = Value[i][j];
            return new Matrix(flatValues);
        }

        public int Size
        {
            get { return Value.Length * Value[0].Length; }
        }

        /// <summary>
        /// Returns a Matrix of dimensions mXn, with values between 0.0 and 1.0
        /// </summary>
        public static Matrix random(int rows, int columns)
        {
            var values = Maths.CreateJagged<double>(rows, columns);
            for (int row = 0; row < values.Length; row++)
                for (int column = 0; column < values[row].Length; column++)
                    values[row][column] = Maths.random.NextDouble();
            return new Matrix(values);
        }

        public int MaxIndex(int rank = 0)
        {
            double maxValue = 0.0;
            int maxIndex = 0;
            for (int m = 0; m < Value.Length; m++)
                for (int n = 0; n < Value[m].Length; n++)
                    if (Value[m][n] > maxValue)
                    {
                        maxValue = Value[m][n];
                        switch (rank)
                        {
                            case 1:
                                maxIndex = n;
                                break;
                            case 0:
                            default:
                                maxIndex = m;
                                break;
                        }
                    }
            return maxIndex;
        }

        public double CrossEntropy(Matrix B, int batch_size)
        {
            return Maths.CrossEntropy(batch_size, Value, B.Value);
        }

        public Matrix SoftMax()
        {
            return new Matrix(Maths.softMax(this));
        }

        public Matrix Sigmoid()
        {
            return new Matrix(Maths.Sigmoid(Value));
        }

        public Matrix ReLU()
        {
            return new Matrix(Maths.ReLU(Value));
        }

        public double Sum()
        {
            double sum = 0.0;
            for (int row = 0; row < Value.Length; row++)
                for (int column = 0; column < Value[row].Length; column++)
                    sum += Value[row][column];
            return sum;
        }

        public Matrix Pow(double exponent)
        {
            for (int row = 0; row < Value.Length; row++)
                for (int column = 0; column < Value[row].Length; column++)
                    Value[row][column] *= Value[row][column];
            return this;
        }

        public Matrix Mean()
        {
            var matrix = new Matrix(Value.Length, 1);
            for (int row = 0; row < Value.Length; row++)
                for (int column = 0; column < Value[row].Length; column++)
                    matrix.Value[row][0] += Value[row][column];

            for (int row = 0; row < matrix.Value.Length; row++)
                matrix.Value[row][0] /= Value[row].Length;

            return matrix;
        }

        public void Initialize(Func<double> initializer)
        {
            for (var x = 0; x < Value.Length; x++)
                for (var y = 0; y < Value[x].Length; y++)
                    Value[x][y] = initializer();
        }

        public static Matrix operator -(Matrix a, Matrix b)
        {
            var newMatrix = Maths.CreateJagged<double>(a.Value.Length, b.Value[0].Length);

            for (var x = 0; x < a.Value.Length; x++)
                for (var y = 0; y < a.Value[x].Length; y++)
                    newMatrix[x][y] = a.Value[x][y] - b.Value[x][y];

            return new Matrix(newMatrix);
        }

        public static Matrix operator +(Matrix a, Matrix b)
        {
            var newMatrix = Maths.CreateJagged<double>(a.Value.Length, b.Value[0].Length);

            for (var x = 0; x < a.Value.Length; x++)
                for (var y = 0; y < a.Value[x].Length; y++)
                    newMatrix[x][y] = a.Value[x][y] + b.Value[x][y];

            return new Matrix(newMatrix);
        }

        public static Matrix operator +(Matrix a, double b)
        {
            var output = Maths.CreateJagged<double>(a.Value.Length, a.Value[0].Length);
            for (var x = 0; x < a.Value.Length; x++)
                for (var y = 0; y < a.Value[x].Length; y++)
                    output[x][y] = a.Value[x][y] + b;

            return new Matrix(output);
        }

        public static Matrix operator -(double a, Matrix m)
        {
            var output = Maths.CreateJagged<double>(m.Value.Length, m.Value[0].Length);
            for (var x = 0; x < m.Value.Length; x++)
                for (var y = 0; y < m.Value[x].Length; y++)
                    output[x][y] = a - m.Value[x][y];

            return new Matrix(output);
        }

        public static Matrix operator -(Matrix m, double a)
        {
            var output = Maths.CreateJagged<double>(m.Value.Length, m.Value[0].Length);
            for (var x = 0; x < m.Value.Length; x++)
                for (var y = 0; y < m.Value[x].Length; y++)
                    output[x][y] = m.Value[x][y] - a;

            return new Matrix(output);
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.Value.Length == b.Value.Length && a.Value[0].Length == b.Value[0].Length) //Hadamard Product
                return GetHadamardProduct(a, b);

            if (a.Value[0].Length == b.Value.Length) //Matrix Product
            {
                var newMatrix = Maths.CreateJagged<double>(a.Value.Length, b.Value[0].Length);
                var length = a.Value[0].Length;

                Parallel.For(0, a.Value.Length, i =>
                {
                    for (var j = 0; j < b.Value[0].Length; j++)
                    {
                        var temp = 0.0;

                        for (var k = 0; k < length; k++)
                            temp += a.Value[i][k] * b.Value[k][j];

                        newMatrix[i][j] = temp;
                    }
                });
                return new Matrix(newMatrix);
            }

            throw new Exception($"Multiplication of these matrices is undefined ({a.Value.Length}x{a.Value[0].Length} * {b.Value.Length}x{b.Value[0].Length}).  AxB * AxB = AxB is (Hadamard Product) and AxB * BxC = AxC is (Matrix Product), but not AxB * AxC or AxB * CxD.");
        }

        public static Matrix operator /(Matrix a, Matrix b)
        {
            if (a.Value.Length == b.Value.Length && a.Value[0].Length == b.Value[0].Length) //Hadamard Quotient
                return GetHadamardQuotient(a, b);

            if (a.Value[0].Length == b.Value.Length) //Matrix quotient
            {
                var newMatrix = Maths.CreateJagged<double>(a.Value.Length, b.Value[0].Length);
                var length = a.Value[0].Length;

                Parallel.For(0, a.Value.Length, i =>
                {
                    for (var j = 0; j < b.Value[0].Length; j++)
                    {
                        var temp = 0.0;

                        for (var k = 0; k < length; k++)
                            temp += a.Value[i][k] / b.Value[k][j];

                        newMatrix[i][j] = temp;
                    }
                });
                return new Matrix(newMatrix);
            }

            throw new Exception($"Division of these matrices is undefined ({a.Value.Length}x{a.Value[0].Length} * {b.Value.Length}x{b.Value[0].Length}).  AxB / AxB = AxB is (Hadamard Product) and AxB / BxC = AxC is (Matrix Product), but not AxB / AxC or AxB / CxD.");
        }

        public static Matrix operator *(double scalar, Matrix m)
        {
            var newMatrix = Maths.CreateJagged<double>(m.Value.Length, m.Value[0].Length);

            for (var x = 0; x < m.Value.Length; x++)
                for (var y = 0; y < m.Value[x].Length; y++)
                    newMatrix[x][y] = m.Value[x][y] * scalar;

            return new Matrix(newMatrix);
        }

        public static Matrix operator *(Matrix m, double scalar)
        {
            return scalar * m;
        }

        public static Matrix operator /(double scalar, Matrix m)
        {
            var newMatrix = Maths.CreateJagged<double>(m.Value.Length, m.Value[0].Length);

            for (var x = 0; x < m.Value.Length; x++)
                for (var y = 0; y < m.Value[x].Length; y++)
                    newMatrix[x][y] = scalar / m.Value[x][y];

            return new Matrix(newMatrix);
        }

        public static Matrix operator /(Matrix m, double scalar)
        {
            var newMatrix = Maths.CreateJagged<double>(m.Value.Length, m.Value[0].Length);

            for (var x = 0; x < m.Value.Length; x++)
                for (var y = 0; y < m.Value[x].Length; y++)
                    newMatrix[x][y] = m.Value[x][y] / scalar;

            return new Matrix(newMatrix);
        }

        public Matrix ToRectangularMatrix(int rows, int columns)
        {
            if (Value[0].Length > 1) throw new Exception("Only nX1 matrices can be converted to rectangular matrices with ToRectangularMatrix");

            double[][] output = Maths.CreateJagged<double>(rows, columns);
            double[] valueArray = Value.Select(v => v[0]).ToArray();
            for (int i = 0; i < Value.Length; i += columns)
            {
                Array.Copy(valueArray, i, output[i / columns], 0, columns);
            }
            return new Matrix(output);
        }

        public static Matrix GetHadamardProduct(Matrix a, Matrix b)
        {
            if (a.Value.Length != b.Value.Length || a.Value[0].Length != b.Value[0].Length)
                throw new ArgumentException($"Hadamard Products on {a.Value.Length}x{a.Value[0].Length} * {b.Value.Length}x{b.Value[0].Length} matrices are invalid.  Hadamard Products can only be performed on matrices of the same size.");

            var outputMatrix = new Matrix(a.Value.Length, a.Value[0].Length);
            GetHadamardProduct(a, b, outputMatrix);
            return outputMatrix;
        }

        public static void GetHadamardProduct(Matrix a, Matrix b, Matrix outputMatrix)
        {
            if (a.Value.Length != b.Value.Length || a.Value[0].Length != b.Value[0].Length)
                throw new ArgumentException($"Hadamard Products on {a.Value.Length}x{a.Value[0].Length} * {b.Value.Length}x{b.Value[0].Length} matrices are invalid.  Hadamard Products can only be performed on matrices of the same size.");

            var values = outputMatrix.Value;

            Parallel.For(0, a.Value.Length, i =>
            {
                for (var j = 0; j < a.Value[i].Length; j++)
                    values[i][j] = a.Value[i][j] * b.Value[i][j];
            });
        }

        public static Matrix GetHadamardQuotient(Matrix a, Matrix b)
        {
            if (a.Value.Length != b.Value.Length || a.Value[0].Length != b.Value[0].Length)
                throw new ArgumentException($"Hadamard Quotient on {a.Value.Length}x{a.Value[0].Length} * {b.Value.Length}x{b.Value[0].Length} matrices are invalid.  Hadamard Products can only be performed on matrices of the same size.");

            var outputMatrix = new Matrix(a.Value.Length, a.Value[0].Length);
            GetHadamardQuotient(a, b, outputMatrix);
            return outputMatrix;
        }

        public static void GetHadamardQuotient(Matrix a, Matrix b, Matrix outputMatrix)
        {
            if (a.Value.Length != b.Value.Length || a.Value[0].Length != b.Value[0].Length)
                throw new ArgumentException($"Hadamard Quotient on {a.Value.Length}x{a.Value[0].Length} * {b.Value.Length}x{b.Value[0].Length} matrices are invalid.  Hadamard Quotients can only be performed on matrices of the same size.");

            var values = outputMatrix.Value;
            Parallel.For(0, a.Value.Length, i =>
            {
                for (var j = 0; j < a.Value[i].Length; j++)
                    values[i][j] = a.Value[i][j] / b.Value[i][j];
            });
        }

        public Matrix Transpose()
        {
            var newMatrix = Maths.CreateJagged<double>(Value[0].Length, Value.Length); //rows --> cols, cols --> rows

            for (var row = 0; row < Value.Length; row++)
                for (var col = 0; col < Value[row].Length; col++)
                    newMatrix[col][row] = Value[row][col];

            return new Matrix(newMatrix);
        }

        public override string ToString()
        {
            int columns = Value[0].Length;

            if (columns == 1)
                columns = (int)Math.Sqrt(Value.Length);

            int printedColumns = 0;

            foreach (var row in Value)
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

            Console.WriteLine();
            return base.ToString();
        }
    }
}
