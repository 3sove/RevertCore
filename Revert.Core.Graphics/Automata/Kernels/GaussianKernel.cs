using System;
using System.Collections.Generic;
using System.Text;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Interpolations;

namespace Revert.Core.Graphics.Automata.Kernels
{
    public class GaussianKernel : Kernel
    {
        public GaussianKernel Kernel = new GaussianKernel(3, 3, Interpolation.exp5);

        public GaussianKernel(int width, int height, Interpolation interpolation = null) : base(width, height, interpolation)
        {
        }

        public override int stepSize => (int)(Width / 2f);

        public override float[][] createMap()
        {
            var map = Maths.CreateJagged<float>(Height, Width);

            for (int y = 0; y < map.Length; y++)
            {
                var row = map[y];
                var relativeY = y.relativePosition(map.Length);
                var distanceFromCenterY = relativeY.distance(.5f) * 2f;

                for (int x = 0; x < row.Length; x++)
                {
                    var relativeX = x.relativePosition(row.Length);
                    var distanceFromCenterX = relativeX.distance(.5f) * 2f;
                    var distanceFromCenter = (float)Math.Sqrt(distanceFromCenterY.pow(2) + distanceFromCenterX.pow(2)) / Maths.getSqrt2();
                    distanceFromCenter = Interpolation.apply(distanceFromCenter, 0f, 1f);
                    row[x] = (1f - distanceFromCenter);
                }
            }

            return map;
        }
    }
}
