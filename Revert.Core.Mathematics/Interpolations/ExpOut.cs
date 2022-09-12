using System;

namespace Revert.Core.Mathematics.Interpolations
{
    public class ExpOut : Exp
    {
        public ExpOut(float value, float power) : base(value, power)
        {
        }

        public override float apply(float a)
        {
            return 1 - ((float)Math.Pow(value, -power * a) - min) * scale;
        }
    }
}
