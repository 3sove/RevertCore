using Revert.Core.Mathematics.Vectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.Vectors
{
    public class Vector2MatrixModel : MatrixModel
    {
        public Vector2MatrixModel(int width, int height, string matrixName) : base(width, height, matrixName)
        {
        }
    }

    public class Vector2Matrix : CellMatrix<Vector2Cell, MatrixModel>
    {

        public Vector2Matrix(MatrixModel model) : base(model)
        {
        }

        public override bool alphaCompare(Vector2Cell cell)
        {
            return false; //TODO: finish
        }

        public override CellMatrix<Vector2Cell, MatrixModel> clone()
        {
            var copy = new Vector2Matrix(model);
            copy.cells = cloneCells();
            return copy;
        }

        protected override Vector2Cell generate()
        {
            return new Vector2Cell(new Vector2(), 0f, 0f);
        }

        protected override Vector2Cell[][] generateMatrix(int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}
