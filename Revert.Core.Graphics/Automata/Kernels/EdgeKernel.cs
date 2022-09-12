using System;
using System.Collections.Generic;
using System.Text;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Interpolations;

namespace Revert.Core.Graphics.Automata.Kernels
{
    public class EdgeKernel : Kernel
    {
        public EdgeKernel(int width, int height, Interpolation interpolation = null) : base(width, height, interpolation)
        {
        }

        public override int stepSize => Width;

        public override float[][] createMap()
        {
            var map = new float[3][];
            map[0] = new float[3] { 1f, 0f, 0f };
            map[1] = new float[3] { 1f, 1f, 0f };
            map[2] = new float[3] { 1f, 1f, 1f };

            return Maths.scale(map, Width, Height);
        }
    }
}
