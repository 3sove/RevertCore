using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.ConvolutionalKernels
{
    public class Outline : Kernel
    {
        private static double[][] field = new double[][]
        {
            new double[] { -1, -1, -1 },
            new double[] { -1,  8, -1 },
            new double[] { -1, -1, -1 }
        };

        public Outline() : base(field)
        {
        }

        public static Outline Instance = new Outline();
    }
}
