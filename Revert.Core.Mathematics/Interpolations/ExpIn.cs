using System;

namespace Revert.Core.Mathematics.Interpolations
{
    public class ExpIn : Exp
    {
        public ExpIn(float value, float power) : base(value, power)
        {
        }

        public override float apply(float a)
        {
            return ((float)Math.Pow(value, power * (a - 1)) - min) * scale;
        }
    }
}
