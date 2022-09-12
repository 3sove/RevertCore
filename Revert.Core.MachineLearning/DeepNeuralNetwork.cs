using Revert.Core.MachineLearning.Layers;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Matrices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.MachineLearning
{
    public class DeepNeuralNetwork
    {
        private readonly double learningRate;
        private NetworkLayer[] layers;

        public DeepNeuralNetwork(NetworkLayer[] hiddenLayers, int numberOfOutputNodes, double learningRate)
        {
            this.learningRate = learningRate;
            layers = new NetworkLayer[hiddenLayers.Length + 1];
            Array.Copy(hiddenLayers, 0, layers, 0, hiddenLayers.Length);
            layers[layers.Length - 1] = new OutputLayer(numberOfOutputNodes, hiddenLayers.Last().GetOutputCount(), this.learningRate);
        }

        public DeepNeuralNetwork(int numberOfInputNodes, int[] hiddenLayerSizes, int numberOfOutputNodes, double learningRate)
        {
            this.learningRate = learningRate;

            layers = new NetworkLayer[hiddenLayerSizes.Length + 1];
            int previousLayerSize = numberOfInputNodes;
            for (int i = 0; i < hiddenLayerSizes.Length; i++)
            {
                layers[i] = new HiddenLayer(hiddenLayerSizes[i], previousLayerSize, learningRate);
                previousLayerSize = hiddenLayerSizes[i];
            }

            layers[layers.Length - 1] = new OutputLayer(numberOfOutputNodes, hiddenLayerSizes[hiddenLayerSizes.Length - 1], this.learningRate);
        }

        public void Train(double[] inputs, Dimensions dimensions, double[] targets)
        {
            layers.First().Train(new Matrix(inputs, dimensions), targets, layers, 0);
        }

        public Matrix Query(Matrix inputs)
        {
            return layers[0].Query(inputs, layers, 0);
        }
    }
}