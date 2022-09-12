using Revert.Core.Graphics.Automata.Agents;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Operations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.Alpha
{
    public class AlphaWanderer : Wanderer<AlphaCell, AlphaMatrixModel>
    {
        public AlphaWanderer(AlphaCell[][] cells, Operations[] operationStack, float entropy, float impact, bool edgeAwareX, bool edgeAwareY) : base(cells, operationStack, entropy, impact, edgeAwareX, edgeAwareY)
        {
        }

        public override int stepSize => kernel.stepSize;

        public override AlphaCell getNewWanderingCell()
        {
            if (WanderingCell == null) WanderingCell = new AlphaCell((int)x, (int)y, .5f);
            return WanderingCell;
        }

        public override AlphaCell modify(AlphaCell cell, float entropy)
        {
            var value = cell.Value + Maths.randomFloat(-entropy, entropy) * cell.AlphaPermeability;
            cell.Value = value.clamp(0f, 1f);
            return cell;
        }
    }
}
