using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Mathematics.Operations.floats
{
    public class Average : FloatOperation
    {
        public Average(float value) : base(value)
        {
        }

        public override float perform(float a, float b, float impact)
        {
            return Maths.average(a, b, impact);
        }
    }
}
