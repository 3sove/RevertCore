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
    public class ConvolutionalLayer : HiddenLayer
    {
        public Dimensions kernelDimensions;
        public Matrix kernelMatrix;
        public Matrix flatKernelMatrix;

        public Matrix kernelWeights;

        private readonly int inputWidth;
        private readonly int inputHeight;

        public ConvolutionalLayer(Dimensions kernelDimensions, int inputWidth, int inputHeight, double learningRate) : base(inputWidth * inputHeight, inputWidth * inputHeight, learningRate)
        {
            this.kernelDimensions = kernelDimensions;

            kernelMatrix = Matrix.random(kernelDimensions.Y, kernelDimensions.X);
            kernelWeights = Matrix.random(kernelMatrix.Size, inputWidth * inputHeight);

            flatKernelMatrix = kernelMatrix.flatten();

            this.inputWidth = inputWidth;
            this.inputHeight = inputHeight;
        }

        public override Matrix Train(Matrix inputs, double[] targets, NetworkLayer[] layers, int index)
        {
            activations = CalculateZ(inputs);

            var forwardErrors = layers[index + 1].Train(activations, targets, layers, index + 1);

            var inputErrors = weightInput.Transpose() * forwardErrors;
            var biasErrors = forwardErrors - biases;

            var kernelErrors = (kernelWeights * forwardErrors).Sigmoid();

            var kernelXinverse = learningRate * flatKernelMatrix * (1.0 - flatKernelMatrix);

            var activationsXinverse = learningRate * activations * (1.0 - activations);

            var transposedInputs = inputs.Transpose();
            weightInput += forwardErrors * activationsXinverse * transposedInputs;
            biases += biasErrors * activationsXinverse;

            kernelWeights += kernelErrors * kernelXinverse;
 
            return inputErrors;
        }

        public override Matrix CalculateZ(Matrix inputs)
        {
            var kernelConvolution = Convolve(inputs);

            var output = new double[kernelConvolution.Length * kernelConvolution[0].Length];
            int outputIndex = 0;

            for (int row = 0; row < kernelConvolution.Length; row++)
                for (int column = 0; column < kernelConvolution[0].Length; column++)
                    output[outputIndex++] = kernelConvolution[row][column];

            return new Matrix(output);
        }

        private double[][] Convolve(Matrix input)
        {
            var slidingInput = new Matrix(kernelMatrix.Dimensions.Y, kernelMatrix.Dimensions.X);
            var slidingOutput = new Matrix(kernelMatrix.Dimensions.Y, kernelMatrix.Dimensions.X);
            var convolved = Maths.CreateJagged<double>(input.Dimensions.Y, input.Dimensions.X);

            var paddingY = (kernelMatrix.Dimensions.Y - 1) / 2;
            var paddingX = (kernelMatrix.Dimensions.X - 1) / 2;

            for (int inputRow = 0; inputRow < inputHeight - (kernelMatrix.Dimensions.Y - 1); inputRow++)
            {
                for (int inputColumn = 0; inputColumn < inputWidth - (kernelMatrix.Dimensions.X - 1); inputColumn++)
                {
                    for (int sampleY = -paddingY; sampleY <= paddingY; sampleY++)
                        for (int sampleX = -paddingX; sampleX <= paddingX; sampleX++)
                        {
                            double value = 0.0;
                            int flattenedPosition;
                            if (inputRow + sampleY >= 0 && inputRow + sampleY < input.Dimensions.Y &&
                                inputColumn + sampleX >= 0 && inputColumn + sampleX < input.Dimensions.X)
                            {
                                flattenedPosition = ((inputRow + sampleY) * input.Dimensions.X) + (inputColumn + sampleX);
                                value = input.Value[flattenedPosition][0];
                            }

                            slidingInput.Value[sampleY + paddingY][sampleX + paddingX] = value;
                        }

                    Matrix.GetHadamardProduct(slidingInput, kernelMatrix, slidingOutput);
                    convolved[inputRow][inputColumn] = slidingOutput.Sum();
                }
            }

            var debugOutput = false;
            if (debugOutput) WriteDebugOutput(input, kernelMatrix, convolved);

            return convolved;
        }

        private static void WriteDebugOutput(Matrix inputs, Matrix kernelMatrix, double[][] convolved)
        {
            Console.WriteLine("Convolution Inputs:");
            Console.WriteLine(inputs.ToString());

            Console.WriteLine($"Kernel:");
            Console.WriteLine(kernelMatrix.ToString());

            for (int rowIndex = 0; rowIndex < convolved.Length; rowIndex++)
            {
                var row = convolved[rowIndex];
                for (int columnIndex = 0; columnIndex < convolved[rowIndex].Length; columnIndex++)
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
