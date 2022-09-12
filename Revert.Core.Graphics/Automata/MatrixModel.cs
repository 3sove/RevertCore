using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata
{
    public class MatrixModel
    {

        public int Width { get; set; }
        public int Height { get; set; }
        public string matrixName { get; set; }


        public MatrixModel(int width, int height, string matrixName)
        {
            Width = width;
            Height = height;
            this.matrixName = matrixName;
        }

    }
}
