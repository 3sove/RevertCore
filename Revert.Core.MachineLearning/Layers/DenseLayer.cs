using System;
using Revert.Core.Mathematics.Matrices;

namespace Revert.Core.MachineLearning.Layers
{
    public class DenseLayer : HiddenLayer
    {
        public DenseLayer(int neurons, int inputNeurons, double learningRate, Activations.ActivationFunction activation) : base(neurons, inputNeurons, learningRate)
        {
            Activation = activation;
        }

        public Activations.ActivationFunction Activation { get; }

        public override Matrix CalculateZ(Matrix inputs)
        {
            return Activation.Activate(base.CalculateZ(inputs));
        }
    }
}