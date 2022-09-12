using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revert.Core.MachineLearning.ConvolutionalKernels;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Matrices;

namespace Revert.Core.MachineLearning.Layers
{
    public class StaticConvolutionalLayer : HiddenLayer
    {
        public Kernel[] kernels;
        private readonly int inputWidth;
        private readonly int inputHeight;

        public StaticConvolutionalLayer(Kernel[] kernels, int inputWidth, int inputHeight, double learningRate) : base(kernels.Sum(k => k.GetConvolvedNeuronCount(inputWidth, inputHeight)), inputWidth * inputHeight, learningRate)
        {
            this.kernels = kernels;
            this.inputWidth = inputWidth;
            this.inputHeight = inputHeight;
        }

        public override Matrix CalculateZ(Matrix inputs)
        {
            var kernelConvolutions = GetKernelConvolutions(inputs);

            var output = new double[kernelConvolutions.Sum(k => k.Length * k[0].Length)];
            int outputIndex = 0;
            foreach (var convolved in kernelConvolutions)
            {
                var columnCount = convolved[0].Length;
                for (int row = 0; row < convolved.Length; row++)
                    for (int column = 0; column < columnCount; column++)
                        output[outputIndex++] = convolved[row][column];
            }
            return new Matrix(output);
        }

        private List<double[][]> GetKernelConvolutions(Matrix inputs)
        {
            List<double[][]> kernelConvolutions = new List<double[][]>();
            foreach (var kernel in kernels)
            {
                var outputSize = kernel.GetConvolvedOutputSize(inputHeight, inputWidth);

                var kernelRfHeight = (double)kernel.ReceptiveField.Length;
                var kernelRfWidth = (double)kernel.ReceptiveField[0].Length;

                double rfPaddingHorizontal = (kernelRfWidth - 1.0) / 2.0;
                double rfPaddingVertical = (kernelRfHeight - 1.0) / 2.0;

                var kernelMatrix = new Matrix(kernel.ReceptiveField);
                var convolvedMatrix = new Matrix(kernel.ReceptiveField.Length, kernel.ReceptiveField[0].Length);
                var kernelConvolution = Maths.CreateJagged<double>(outputSize.Item1, outputSize.Item2);

                for (int inputRow = 0; inputRow < inputHeight - (kernelRfHeight - 1); inputRow++)
                {
                    for (int inputColumn = 0; inputColumn < inputWidth - (kernelRfWidth - 1); inputColumn++)
                    {
                        var slidingMatrix = new Matrix(kernel.ReceptiveField.Length, kernel.ReceptiveField[0].Length);
                        for (int row = 0; row < slidingMatrix.Value.Length; row++)
                            for (int column = 0; column < slidingMatrix.Value[row].Length; column++)
                                slidingMatrix.Value[row][column] = inputs.Value[((inputRow + row) * inputs.Dimensions.X) + (inputColumn + column)][0];

                        Matrix.GetHadamardProduct(slidingMatrix, kernelMatrix, convolvedMatrix);
                        kernelConvolution[inputRow][inputColumn] = convolvedMatrix.Sum();
                    }
                }
                kernelConvolutions.Add(kernelConvolution);

                var debugOutput = false;
                if (debugOutput) WriteDebugOutput(inputs, kernelConvolution, kernel);
            }
            return kernelConvolutions;
        }

        private static void WriteDebugOutput(Matrix inputs, double[][] convolvedInput, Kernel kernel)
        {
            Console.WriteLine("Convolution Inputs:");
            Console.WriteLine(inputs.ToString());

            Console.WriteLine($"Kernel Outputs ({kernel.GetType().Name}):");

            for (int rowIndex = 0; rowIndex < convolvedInput.Length; rowIndex++)
            {
                var row = convolvedInput[rowIndex];
                for (int columnIndex = 0; columnIndex < convolvedInput[rowIndex].Length; columnIndex++)
                {
                    var column = row[columnIndex];
                    if (column == 0.0)
                        Console.Write(" ");
                    else if (column < 0.3)
                        Console.Write(".");
                    else if (column < 0.6)
                        Console.Write(";");
                    else
                        Console.Write("#");
                }
                Console.WriteLine();
            }
            Console.ReadKey();
        }

    }
}
