using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Mathematics.Operations.floats
{
    public interface IFloatOperation
    {
        float perform(float a, float b, float impact);
    }

    public abstract class FloatOperation : Operation<float>, IFloatOperation
    {
        public FloatOperation(float value) : base(value)
        {
        }
    }
}
