using Revert.Core.Graphics.Automata.Agents;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Operations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.HSL
{
    public class HSLWanderer : DirectionBiasedWanderer<HSLCell, HSLMatrixModel>
    {
        public HSLWanderer(HSLCell[][] cells, Operations[] operationStack, float entropy, float impact, bool edgeAwareX, bool edgeAwareY) : base(cells, operationStack, entropy, impact, edgeAwareX, edgeAwareY)
        {
        }

        public HSLWanderer(HSLCell[][] cells, Operations[] operationStack, float entropy, float impact, float[] directionBias, bool edgeAwareX, bool edgeAwareY) : base(cells, operationStack, entropy, impact, directionBias, edgeAwareX, edgeAwareY)
        {
        }

        public override int stepSize => kernel.stepSize;

        public override HSLCell getNewWanderingCell()
        {
            if (WanderingCell == null) WanderingCell = new HSLCell(0f, 0f, Maths.randomFloat(), Maths.randomFloat(.4f, .6f), Maths.randomFloat(.4f, .6f), 1f);
            return WanderingCell;
        }

        public override HSLCell modify(HSLCell cell, float entropy)
        {
            cell.Hue = Maths.edgeAwareAdd(cell.Hue, Maths.randomFloat(-entropy, entropy), 0f, 1f);
            cell.Saturation = (cell.Saturation + Maths.randomFloat(-entropy, entropy)).clamp(0f, 1f);
            cell.Lightness = (cell.Lightness + Maths.randomFloat(-entropy, entropy)).clamp(0f, 1f);
            return cell;
        }
    }
}
