using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class Pow2OutInverse : Interpolation
    {
        public override float apply(float a)
        {
            return 1 - (float)Math.Sqrt(-(a - 1));
        }
    };
}
