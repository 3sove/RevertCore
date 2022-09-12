using System;

namespace Revert.Core.Mathematics.Interpolations
{
    public class Exp : Interpolation
    {
        protected float value, power, min, scale;

        public Exp(float value, float power)
        {
            this.value = value;
            this.power = power;
            min = (float)Math.Pow(value, -power);
            scale = 1 / (1 - min);
        }

        public override float apply(float a)
        {
            if (a <= 0.5f) return ((float)Math.Pow(value, power * (a * 2 - 1)) - min) * scale / 2;
            return (2 - ((float)Math.Pow(value, -power * (a * 2 - 1)) - min) * scale) / 2;
        }
    };
}
