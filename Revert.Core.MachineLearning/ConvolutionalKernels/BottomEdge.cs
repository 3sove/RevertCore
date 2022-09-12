using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.ConvolutionalKernels
{
    public class BottomEdge : Kernel
    {
        private static double[][] field = new double[][]
        {
            new double[] { .5, .5, .5 },
            new double[] {  1,  1,  1 },
            new double[] {  0,  0,  0 }
        };

        public BottomEdge() : base(field)
        {
        }

        public static BottomEdge Instance = new BottomEdge();
    }
}
