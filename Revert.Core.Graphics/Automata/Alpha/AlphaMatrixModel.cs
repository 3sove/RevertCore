using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.Alpha
{
    public class AlphaMatrixModel : MatrixModel
    {
        public float AlphaThreshold { get; }
        public float[][] Map { get; }

        public AlphaMatrixModel(int width, int height, float alphaThreshold, float[][] map) : base(width, height, "Alpha")
        {
            AlphaThreshold = alphaThreshold;
            Map = map;
        }
    }
}
