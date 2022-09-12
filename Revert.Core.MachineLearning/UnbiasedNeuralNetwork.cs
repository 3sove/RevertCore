using Revert.Core.Mathematics.Matrices;
using System;
using System.Linq;

namespace Revert.Core.MachineLearning
{
    public class UnbiasedNeuralNetwork
    {
        private readonly double learningRate;
        private Matrix weightHiddenOutput;
        private Matrix weightInputHidden;
        private Random rnd = new Random();

        public UnbiasedNeuralNetwork(int numberOfInputNodes, int numberOfHiddenNodes, int numberOfOutputNodes, double learningRate)
        {
            this.learningRate = learningRate;
            weightInputHidden = new Matrix(numberOfHiddenNodes, numberOfInputNodes); //each cell of the matrices are Neurites
            weightHiddenOutput = new Matrix(numberOfOutputNodes, numberOfHiddenNodes);

            weightHiddenOutput.Initialize(() => rnd.NextDouble() - 0.5);
            weightInputHidden.Initialize(() => rnd.NextDouble() - 0.5);
        }

        public void Train(double[] inputs, double[] targets)
        {
            var inputSignals = new Matrix(inputs);
            var targetSignals = new Matrix(targets);

            var hiddenOutputs = (weightInputHidden * inputSignals).Sigmoid();
            var finalOutputs = (weightHiddenOutput * hiddenOutputs).Sigmoid();

            var outputErrors = targetSignals - finalOutputs;
            var hiddenErrors = weightHiddenOutput.Transpose() * outputErrors;

            weightHiddenOutput += learningRate * outputErrors * finalOutputs * (1.0 - finalOutputs) * hiddenOutputs.Transpose();
            weightInputHidden += learningRate * hiddenErrors * hiddenOutputs * (1.0 - hiddenOutputs) * inputSignals.Transpose();
        }

        public double[] Query(double[] inputs)
        {
            var inputSignals = new Matrix(inputs);

            var hiddenOutputs = (weightInputHidden * inputSignals).Sigmoid();
            var finalOutputs = (weightHiddenOutput * hiddenOutputs).Sigmoid();

            return finalOutputs.Value.SelectMany(x => x.Select(y => y)).ToArray();
        }
    }
}