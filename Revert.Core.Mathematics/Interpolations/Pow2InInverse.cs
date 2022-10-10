using System;

namespace Revert.Core.Mathematics.Interpolations
{
    public class Pow2InInverse : Interpolation
    {
        public override float apply(float a)
        {
            return (float)Math.Sqrt(a);
        }
    };
}
