﻿using System;

namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class SineOut : Interpolation
    {
        public override float apply(float a)
        {
            return (float)Math.Sin(a * Math.PI / 2);
        }
    };
}
