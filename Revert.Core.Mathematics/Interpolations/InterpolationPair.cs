using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class InterpolationPair : Interpolation
    {
        public static InterpolationPair NormalDistribution = new InterpolationPair(pow2Out, pow2In);

        public InterpolationPair(Interpolation left, Interpolation right)
        {
            this.left = left;
            this.right = right;
        }

        public Interpolation left { get; private set; }
        public Interpolation right { get; private set; }

        public override float apply(float a)
        {
            if (a <= .5f) //if we're on the first half, use the 'left' interpolation
            {
                return left.apply(0f, 1f, a * 2) * .5f; //end at the midPoint
            }
            else //if we're on the second half, use the 'right' interpolation
            {
                return right.apply(0f, 1f, (a - .5f) * 2) * .5f + .5f; //start at the midPoint
            }
        }
    }
}
