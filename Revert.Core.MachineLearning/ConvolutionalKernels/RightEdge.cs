using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.ConvolutionalKernels
{
    public class RightEdge : Kernel
    {
        private static double[][] field = new double[][]
        {
            new double[] { .5,  1,  0 },
            new double[] { .5,  1,  0 },
            new double[] { .5,  1,  0 }
        };

        public RightEdge() : base(field)
        {
        }

        public static RightEdge Instance = new RightEdge();
    }
}
