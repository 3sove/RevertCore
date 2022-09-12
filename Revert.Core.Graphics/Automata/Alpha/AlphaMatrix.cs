using Microsoft.Xna.Framework;
using Revert.Core.Extensions;
using Revert.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.Alpha
{
    public class AlphaMatrix : CellMatrix<AlphaCell, AlphaMatrixModel>
    {
        public Color color { get; set; } = Color.Black;

        public AlphaMatrix(AlphaMatrixModel model) : base(model)
        {
        }

        public override bool alphaCompare(AlphaCell cell)
        {
            return cell.Value >= model.AlphaThreshold;
        }

        public override CellMatrix<AlphaCell, AlphaMatrixModel> clone()
        {
            var copy = new AlphaMatrix(model);
            copy.cells = cloneCells();
            return copy;
        }

        protected override AlphaCell generate()
        {
            return new AlphaCell(0f, 0f, 0f);
        }

        protected override AlphaCell[][] generateMatrix(int width, int height)
        {
            var matrix = Maths.CreateJagged<AlphaCell>(height, width);
            //matrix.fill(generateCell);
            matrix.fill((x, y) => new AlphaCell(x, y, model.Map[y][x]));
            return matrix;
        }
    }
}
