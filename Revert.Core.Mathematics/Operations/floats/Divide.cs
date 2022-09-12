using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Mathematics.Operations.floats
{
    public class Divide : FloatOperation
    {
        public Divide(float value) : base(value)
        {
        }

        public override float perform(float a, float b, float impact)
        {
            return a.interpolate(a / b, impact);
        }
    }
}
