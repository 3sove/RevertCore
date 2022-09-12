using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Interpolations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.Permeability
{
    public class PermeableValue
    {
        public int Id { get; }
        public float Minimum { get; }
        public float Maximum { get; }
        public Interpolation Interpolation { get; }
        public float PermeationProbability { get; }

        public float Value { get; set; }

        public PermeableValue(int id, float minimum, float maximum, Interpolation interpolation, float permeationProbability)
        {
            Id = id;
            Minimum = minimum;
            Maximum = maximum;
            Interpolation = interpolation;
            PermeationProbability = permeationProbability;
            Value = Maths.randomFloat(minimum, maximum, interpolation);
        }

        public PermeableValue clone()
        {
            return new PermeableValue(Id, Minimum, Maximum, Interpolation, PermeationProbability);
        }
    }
}
