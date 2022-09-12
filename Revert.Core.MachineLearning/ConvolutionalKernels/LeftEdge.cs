using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.ConvolutionalKernels
{
    public class LeftEdge : Kernel
    {
        private static double[][] field = new double[][]
        {
            new double[] {  0,  1, .5 },
            new double[] {  0,  1, .5 },
            new double[] {  0,  1, .5 }
        };

        public LeftEdge() : base(field)
        {
        }

        public static LeftEdge Instance = new LeftEdge();
    }
}
