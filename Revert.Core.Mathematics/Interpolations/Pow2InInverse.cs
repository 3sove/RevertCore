using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class Pow2InInverse : Interpolation
    {
        public override float apply(float a)
        {
            return (float)Math.Sqrt(a);
        }
    };
}
