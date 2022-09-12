using System;
using System.Collections.Generic;
using System.Text;
using Revert.Core.Extensions;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Interpolations;

namespace Revert.Core.Graphics.Automata.Kernels
{
    public class PointKernel : Kernel
    {
        public PointKernel(int width, int height, Interpolation interpolation = null) : base(width, height, interpolation)
        {
        }

        public override int stepSize => Width;

        public override float[][] createMap()
        {
            var map = Maths.CreateJagged<float>(1, 1).fill(1f);
            return resize(map, Width, Height);
        }

        public static PointKernel Default = new PointKernel(1, 1);
    }
}
