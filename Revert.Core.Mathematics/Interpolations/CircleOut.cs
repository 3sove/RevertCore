using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class CircleOut : Interpolation
    {
        public override float apply(float a)
        {
            a--;
            return (float)Math.Sqrt(1 - a * a);
        }
    };
}
