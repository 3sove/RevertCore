using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Mathematics.Operations
{
    public abstract class Operation<T>
    {
        public T presetB;

        public Operation(T b)
        {
            presetB = b;
        }

        public abstract T perform(T a, T b, float impact);

        public T perform(T a)
        {
            return perform(a, presetB, 1f);
        }

        public T perform(T a, float impact)
        {
            return perform(a, presetB, impact);
        }


    }
}
