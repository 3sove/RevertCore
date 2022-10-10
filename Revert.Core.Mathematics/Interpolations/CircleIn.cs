using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class CircleIn : Interpolation
    {
        public override float apply(float a)
        {
            return 1 - (float)Math.Sqrt(1 - a * a);
        }
    };
}
