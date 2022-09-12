using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Matrices;

namespace Revert.Core.MachineLearning.Layers
{
    public abstract class NetworkLayer
    {
        protected Matrix weightInput;
        public Matrix activations;
        protected Matrix biases;
        protected double learningRate;
        public int neurons;

        public NetworkLayer(int neurons, int inputNeurons, double learningRate)
        {
            this.neurons = neurons;
            weightInput = new Matrix(neurons, inputNeurons, () => Maths.random.NextDouble() - 0.5);
            biases = new Matrix(neurons, 1, () => Maths.random.NextDouble() - 0.5);
            this.learningRate = learningRate;
        }

        public virtual int GetOutputCount()
        {
            return neurons;
        }

        public virtual Matrix CalculateZ(Matrix inputs)
        {
            return (weightInput * inputs + biases);
        }

        public abstract Matrix Train(Matrix inputs, double[] targets, NetworkLayer[] layers, int index);

        public abstract Matrix Query(Matrix inputs, NetworkLayer[] layers, int index);

        //public abstract Matrix Calculate(Matrix inputs);
    }
}