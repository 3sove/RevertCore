using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class ElasticOut : Elastic
    {
        public ElasticOut(float value, float power, int bounces, float scale) : base(value, power, bounces, scale)
        {
        }

        public override float apply(float a)
        {
            if (a == 0) return 0;
            a = 1 - a;
            return 1 - (float)(Math.Pow(value, power * (a - 1)) * Math.Sin(a * bounces) * scale);
        }
    }
}
