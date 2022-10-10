using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class Pow : Interpolation
    {
        protected int power;

        public Pow(int power)
        {
            this.power = power;
        }

        public override float apply(float a)
        {
            if (a <= 0.5f) return (float)Math.Pow(a * 2, power) / 2;
            return (float)Math.Pow((a - 1) * 2, power) / (power % 2 == 0 ? -2 : 2) + 1;
        }
    }
}
