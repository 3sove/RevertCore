using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.ConvolutionalKernels
{
    public class Emboss : Kernel
    {
        private static double[][] field = new double[][]
        {
            new double[] { -2, -1,  0 },
            new double[] { -1,  1,  1 },
            new double[] {  0,  1,  2 }
        };

        public Emboss() : base(field)
        {
        }
        public static Emboss Instance = new Emboss();
    }
}
