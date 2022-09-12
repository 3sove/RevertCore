using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Interpolations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.Kernels
{
    public abstract class Kernel
    {

        public Kernel(int width, int height, Interpolation interpolation = null)
        {
            Width = width;
            Height = height;
            Interpolation = interpolation ?? Interpolation.exp5;
            map = createMap();
        }

        public float[][] map;

        public abstract float[][] createMap();

        public abstract int stepSize { get; }

        public int Width { get; set; }
        public int Height { get; set; }
        public Interpolation Interpolation { get; }

        public float[][] resize(float[][] array, int width, int height)
        {
            this.Width = width;
            this.Height = height;
            return Maths.scale(array, width, height);
        }

    }
}
