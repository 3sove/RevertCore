using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class ElasticIn : Elastic
    {
        public ElasticIn(float value, float power, int bounces, float scale) : base(value, power, bounces, scale)
        {
        }

        public override float apply(float a)
        {
            if (a >= 0.99) return 1;
            return (float)(Math.Pow(value, power * (a - 1)) * Math.Sin(a * bounces) * scale);
        }
    }
}
