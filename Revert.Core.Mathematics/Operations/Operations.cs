using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Mathematics.Operations
{
    public enum Operations
    {
        ADD = 1,
        SUBTRACT = 2,
        MULTIPLY = 3,
        DIVIDE = 4,
        AVERAGE = 5,
        SCREEN = 6
    }

    public static class OperationsExtensions
    {
        public static Operations[] GetRandomAddOrSubtract(float addProbability = .5f)
        {
            if (Maths.randomBoolean(addProbability))
                return new Operations[] { Operations.ADD };
            else
                return new Operations[] { Operations.SUBTRACT };
        }

    }

}
