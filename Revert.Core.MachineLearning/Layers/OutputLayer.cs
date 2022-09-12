using System.Linq;
using Revert.Core.Extensions;
using Revert.Core.Mathematics.Matrices;

namespace Revert.Core.MachineLearning.Layers
{
    public class OutputLayer : NetworkLayer
    {
        public OutputLayer(int neurons, int inputNeurons, double learningRate) : base(neurons, inputNeurons, learningRate)
        {
        }

        public override Matrix Train(Matrix inputs, double[] targets, NetworkLayer[] layers, int index)
        {
            //non linearity
            activations = CalculateZ(inputs).SoftMax();

            var targetMatrix = new Matrix(targets);
            var outputErrors = targetMatrix - activations;
            var inputErrors = weightInput.Transpose() * outputErrors;

            var activationsXinverse = learningRate * activations * (1.0 - activations);

            weightInput += outputErrors * activationsXinverse * inputs.Transpose();

            var biasErrors = targetMatrix - biases;
            biases += biasErrors * activationsXinverse;

            return inputErrors;
        }

        public override Matrix Query(Matrix inputs, NetworkLayer[] layers, int index)
        {
            return CalculateZ(inputs).SoftMax();
        }
    }
}