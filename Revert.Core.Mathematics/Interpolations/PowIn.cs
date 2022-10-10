using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class PowIn : Pow
    {
        public PowIn(int power) : base(power)
        {
        }

        public override float apply(float a)
        {
            return (float)Math.Pow(a, power);
        }
    }
}
