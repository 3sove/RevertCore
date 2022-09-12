using System;

namespace Revert.Core.Mathematics.Interpolations
{
    public class Pow3OutInverse : Interpolation
    {
        public override float apply(float a)
        {
            return 1 - (float)Math.Pow(-(a - 1), 1.0 / 3.0);
        }
    };
}
