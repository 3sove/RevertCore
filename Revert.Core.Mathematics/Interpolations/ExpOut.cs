using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
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
