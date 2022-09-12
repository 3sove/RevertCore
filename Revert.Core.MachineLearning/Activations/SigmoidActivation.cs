using System;
using System.Collections.Generic;
using System.Text;
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
