using Revert.Core.Mathematics.Matrices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.Activations
{
    public abstract class ActivationFunction<TSelf> : ActivationFunction where TSelf : new()
    {
        public static readonly TSelf Instance = new TSelf();
    }

    public abstract class ActivationFunction
    {
        public abstract Matrix Activate(Matrix matrix);
    }
}
