using System;
using System.Collections.Generic;
using System.Text;
using Revert.Core.Extensions;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Interpolations;

namespace Revert.Core.Graphics.Automata.Kernels
{
    public class FullKernel : Kernel
    {
        public FullKernel(int width, int height, Interpolation interpolation = null) : base(width, height, interpolation)
        {
        }

        public override int stepSize => Width;

        public override float[][] createMap()
        {
            return Maths.scale(Maths.CreateJagged<float>(3, 3).fill(1f), Width, Height);
        }
    }
}
