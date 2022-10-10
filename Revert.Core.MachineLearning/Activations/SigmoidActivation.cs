using Revert.Core.Mathematics.Matrices;

namespace Revert.Core.MachineLearning.Activations
{
    public class SigmoidActivation : ActivationFunction<SigmoidActivation>
    {
        public override Matrix Activate(Matrix matrix)
        {
            return matrix.Sigmoid();
        }
    }
}
