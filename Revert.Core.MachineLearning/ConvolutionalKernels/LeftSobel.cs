using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.ConvolutionalKernels
{
    public class LeftSobel : Kernel
    {
        private static double[][] field = new double[][]
        {
            new double[] {  1,  0, -1 },
            new double[] {  2,  0, -2 },
            new double[] {  1,  0, -1 }
        };

        public LeftSobel() : base(field)
        {
        }

        public static LeftSobel Instance = new LeftSobel();
    }
}
