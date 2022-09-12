using Revert.Core.Common.Performance;
using Revert.Core.Extensions;
using Revert.Core.MachineLearning.ConvolutionalKernels;
using Revert.Core.MachineLearning.Data;
using Revert.Core.MachineLearning.Layers;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Matrices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Revert.Core.MachineLearning.ProblemSets
{
    public class MNIST
    {
        public static NetworkLayer[] GetConvolutionalLayers()
        {
            NetworkLayer[] layers = new NetworkLayer[2];
            //var kernels = new Kernel[] { Identity.Instance }; //, LeftEdge.Instance };
            layers[0] = new ConvolutionalLayer(new Dimensions(3, 3, 1), 28, 28, 0.1);
            layers[1] = new DenseLayer(layers[0].neurons / 4, layers[0].neurons, 0.1, Activations.SigmoidActivation.Instance);
            return layers;
        }

        public static NetworkLayer[] GetSimpleLayers()
        {
            NetworkLayer[] layers = new NetworkLayer[1];
            layers[0] = new DenseLayer(196, 784, 0.1, Activations.SigmoidActivation.Instance);
            return layers;
        }

        public static void Run(IdxFile trainingData, IdxFile testData)
        {
            NetworkLayer[] layers = GetConvolutionalLayers();
            //NetworkLayer[] layers = GetSimpleLayers();
            var network = new DeepNeuralNetwork(layers, 10, 0.1);
            //var network = new DeepNeuralNetwork(784, new[] { 196 }, 10, 0.1);
            Console.WriteLine($"Training network with {trainingData.Images.Count} training samples.");

            var normalizedInputs = trainingData.Images.Select(x => new
            {
                Answer = (int)x.label,
                Inputs = NormalizeInput(x.pixels)
            }).ToArray();

            var sw = new Stopwatch();
            sw.Start();

            var performanceMonitor = new PerformanceMonitor("Network Training", 100, normalizedInputs.Length);

            var targets = new double[10];
            for (int i = 0; i < targets.Length; i++)
                targets[i] = 0.01;

            var debugOutput = false;

            var imageDimensions = new Dimensions(trainingData.Images[0].columns, trainingData.Images[0].rows);

            foreach (var image in trainingData.Images)
            {
                var normalizedInput = NormalizeInput(image.pixels);

                for (int i = 0; i < targets.Length; i++)
                    targets[i] = 0.01;
                targets[image.label] = 0.99;

                network.Train(normalizedInput, imageDimensions, targets);
                performanceMonitor.Tick();

                if (debugOutput)
                {
                    Console.WriteLine(image.ToString());
                    Console.ReadKey();
                }

                if (performanceMonitor.Position >= 1000) break;
            }

            sw.Stop();
            Console.WriteLine($"Finished training in {sw.ElapsedMilliseconds}ms");

            var scoreCard = new List<bool>();
            foreach (var input in testData.Images)
            {
                double[] normalizedInput = NormalizeInput(input.pixels);
                var response = network.Query(new Matrix(normalizedInput, imageDimensions));

                var max = response.MaxIndex();
                var result = input.label == max;

                if (debugOutput)
                {
                    Console.WriteLine(input.ToString());
                    Console.WriteLine($"Guess: {max}");
                    Console.WriteLine(result);
                    Console.ReadKey();
                }

                if (scoreCard.Count % 100 == 0)
                {
                    Console.WriteLine($"Score: {scoreCard.Count(x => x).ToString("N0")} / {scoreCard.Count.ToString("N0")} || {(double)scoreCard.Count(x => x) / scoreCard.Count} %.");
                }

                scoreCard.Add(result);
            }

            Console.WriteLine();
            Console.WriteLine("Final Score");

            Console.WriteLine($"Score: {scoreCard.Count(x => x).ToString("N0")} / {scoreCard.Count.ToString("N0")}");
            Console.WriteLine($"Accuracy: {(double)scoreCard.Count(x => x) / scoreCard.Count} %.");
        }

        private static double[] NormalizeInput(byte[] bytes)
        {
            var normalized = new double[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
                normalized[i] = (bytes[i] / 255.00) * 0.99 + 0.01;
            return normalized;
        }
    }
}
