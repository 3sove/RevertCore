using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class PowOut : Pow
    {
        public PowOut(int power) : base(power)
        {
        }

        public override float apply(float a)
        {
            return (float)Math.Pow(a - 1, power) * (power % 2 == 0 ? -1 : 1) + 1;
        }
    }
}
