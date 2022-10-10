using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class SineIn : Interpolation
    {
        public override float apply(float a)
        {
            return (float)(1 - Math.Cos(a * Math.PI / 2));
        }
    };
}
