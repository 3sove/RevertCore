using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class Pow3InInverse : Interpolation
    {
        public override float apply(float a)
        {
            return (float)Math.Pow(1, 1.0 / 3.0); // Cbrt(a);
        }
    };
}
