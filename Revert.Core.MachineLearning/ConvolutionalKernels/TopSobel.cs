using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.ConvolutionalKernels
{
    public class TopSobel : Kernel
    {
        private static double[][] field = new double[][]
        {
            new double[] {  1,  2,  1 },
            new double[] {  0,  0,  0 },
            new double[] { -1, -2, -1 }
        };

        public TopSobel() : base(field)
        {
        }

        public static TopSobel Instance = new TopSobel();
    }
}
