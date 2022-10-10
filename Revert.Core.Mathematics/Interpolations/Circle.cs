using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class Circle : Interpolation
    {
        public override float apply(float a)
        {
            if (a <= 0.5f)
            {
                a *= 2;
                return (1 - (float)Math.Sqrt(1 - a * a)) / 2;
            }
            a--;
            a *= 2;
            return ((float)Math.Sqrt(1 - a * a) + 1) / 2;
        }
    };
}
