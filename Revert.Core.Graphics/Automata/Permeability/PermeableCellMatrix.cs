using Revert.Core.Graphics.Automata;
using System;
using System.Collections.Generic;

namespace Revert.Core.Graphics.Automata.Permeability
{
    public class PermeableCellMatrix : CellMatrix<PermeableCell, PermeableMatrixModel>
    {
        public PermeableCellMatrix(PermeableMatrixModel model) : base(model)
        {
        }

        public override bool alphaCompare(PermeableCell cell)
        {
            return cell.ValueMap[(int)PermeableValues.Alpha].Value > 0f;
        }

        public override CellMatrix<PermeableCell, PermeableMatrixModel> clone()
        {
            var copy = new PermeableCellMatrix(model);
            copy.cells = cloneCells();
            return copy;
        }

        protected override PermeableCell generate()
        {
            return new PermeableCell(0f, 0f, new List<PermeableValue>());
        }

        protected override PermeableCell[][] generateMatrix(int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}
