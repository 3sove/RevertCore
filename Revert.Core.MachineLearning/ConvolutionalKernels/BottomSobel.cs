using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.ConvolutionalKernels
{
    public class BottomSobel : Kernel
    {
        private static double[][] field = new double[][]
        {
            new double[] { -1, -2, -1 },
            new double[] {  0,  0,  0 },
            new double[] {  1,  2,  1 }
        };

        public BottomSobel() : base(field)
        {
        }

        public static BottomSobel Instance = new BottomSobel();
    }
}
