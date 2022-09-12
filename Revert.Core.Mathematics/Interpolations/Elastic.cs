using System;

namespace Revert.Core.Mathematics.Interpolations
{
    public class Elastic : Interpolation
    {
        protected float value, power, scale, bounces;

        public Elastic(float value, float power, int bounces, float scale)
        {
            this.value = value;
            this.power = power;
            this.scale = scale;
            this.bounces = (float)(bounces * Math.PI * (bounces % 2 == 0 ? 1 : -1));
        }

        public override float apply(float a)
        {
            if (a <= 0.5f)
            {
                a *= 2;
                return (float)(Math.Pow(value, power * (a - 1)) * Math.Sin(a * bounces) * scale / 2);
            }
            a = 1 - a;
            a *= 2;
            return 1 - (float)(Math.Pow(value, power * (a - 1)) * Math.Sin(a * bounces) * scale / 2);
        }
    }
}
