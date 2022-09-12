using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.HSL
{
    public class HSLMatrixModel : MatrixModel
    {
        public HSLMatrixModel(int width, int height, string matrixName, float lightnessAlphaThreshold = 0.5f) : base(width, height, matrixName)
        {
            LightnessAlphaThreshold = lightnessAlphaThreshold;
        }

        public float LightnessAlphaThreshold { get; }
    }
}
