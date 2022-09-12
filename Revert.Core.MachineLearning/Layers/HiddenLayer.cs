using System;
using Revert.Core.Mathematics.Matrices;

namespace Revert.Core.MachineLearning.Layers
{
    public class HiddenLayer : NetworkLayer
    {
        public HiddenLayer(int neurons, int inputNeurons, double learningRate) : base(neurons, inputNeurons, learningRate)
        {
        }

        public override Matrix Train(Matrix inputs, double[] targets, NetworkLayer[] layers, int index)
        {
            activations = CalculateZ(inputs);

            var forwardErrors = layers[index + 1].Train(activations, targets, layers, index + 1);

            var inputErrors = weightInput.Transpose() * forwardErrors;
            var biasErrors = forwardErrors - biases;
            var transposedInputs = inputs.Transpose();

            var activationsXinverse = learningRate * activations * (1.0 - activations);

            //weightInput += learningRate * forwardErrors * activations * (1.0 - activations) * transposedInputs;
            weightInput += forwardErrors * activationsXinverse * transposedInputs;
            biases += biasErrors * activationsXinverse;
            return inputErrors;
        }

        public override Matrix Query(Matrix inputs, NetworkLayer[] layers, int index)
        {
            var queryValues = CalculateZ(inputs);
            return layers[index + 1].Query(queryValues, layers, index + 1);

        }
    }
}