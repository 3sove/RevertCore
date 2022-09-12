using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.ConvolutionalKernels
{
    public class Identity : Kernel
    {
        private static double[][] field = new double[][]
        {
            new double[] {  0,  0,  0 },
            new double[] {  0,  1,  0 },
            new double[] {  0,  0,  0 }
        };

        public Identity() : base(field)
        {
        }

        public static Identity Instance = new Identity();
    }
}
