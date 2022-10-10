using System;

namespace Revert.Core.Mathematics.Interpolations
{
    public class Pow2OutInverse : Interpolation
    {
        public override float apply(float a)
        {
            return 1 - (float)Math.Sqrt(-(a - 1));
        }
    };
}
