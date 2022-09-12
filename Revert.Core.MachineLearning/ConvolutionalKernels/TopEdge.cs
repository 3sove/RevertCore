using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.ConvolutionalKernels
{
    public class TopEdge : Kernel
    {
        private static double[][] field = new double[][]
        {
            new double[] {  0,  0,  0 },
            new double[] {  1,  1,  1 },
            new double[] { .5, .5, .5 }
        };

        public TopEdge() : base(field)
        {
        }

        public static TopEdge Instance = new TopEdge();
    }
}
