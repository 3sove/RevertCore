using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Mathematics.Constraints
{
    public class Constraint<T>
    {
        public T Minimum { get; set; }
        public T Maximum { get; set; }

        public void setConstraint(Constraint<T> constraint)
        {
            Minimum = constraint.Minimum;
            Maximum = constraint.Maximum;
        }
    }
}
