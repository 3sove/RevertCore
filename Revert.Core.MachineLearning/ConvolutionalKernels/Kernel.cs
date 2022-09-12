using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.MachineLearning.ConvolutionalKernels
{
    public class Kernel
    {
        public double[][] ReceptiveField;

        public Kernel(double[][] receptiveField)
        {
            ReceptiveField = receptiveField;
            var rfHeight = receptiveField.Length;
            var rfWidth = receptiveField[0].Length;

            if (rfHeight % 2 == 0 || rfWidth % 2 == 0)
            {
                throw new ArgumentException("Kernel ReceptiveField width and height must be odd.");
            }
        }

        private Tuple<int, int> receptiveFieldPadding = null;
        public Tuple<int, int> GetReceptiveFieldPadding()
        {
            if (receptiveFieldPadding == null)
            {
                var rfRows = ReceptiveField.Length;
                var rfColumns = ReceptiveField[0].Length;
                var rfPaddingRows = (rfRows - 1) / 2;
                var rfPaddingColumns = (rfColumns - 1) / 2;
                receptiveFieldPadding = new Tuple<int, int>(rfPaddingRows, rfPaddingColumns);
            }
            return receptiveFieldPadding;
        }

        private Tuple<int, int> convolvedOutputSize = null;
        public Tuple<int, int> GetConvolvedOutputSize(int rows, int columns)
        {
            if (convolvedOutputSize == null)
            {
                var padding = GetReceptiveFieldPadding();
                var width = columns - padding.Item1 * 2;
                var height = rows - padding.Item2 * 2;
                convolvedOutputSize = new Tuple<int, int>(width, height);
            }
            return convolvedOutputSize;
        }

        public int GetConvolvedNeuronCount(int inputWidth, int inputHeight)
        {
            var outputSize = GetConvolvedOutputSize(inputWidth, inputHeight);
            return outputSize.Item1 * outputSize.Item2;
        }
    }
}
