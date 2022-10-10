using System;

namespace Revert.Core.Mathematics.Interpolations
{
    public class Sine : Interpolation
    {
        public override float apply(float a)
        {
            return (float)(1 - Math.Cos(a * Math.PI)) / 2;
        }
    };
}
