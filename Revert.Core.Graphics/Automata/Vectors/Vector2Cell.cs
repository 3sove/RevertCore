using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Vectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.Vectors
{
    public class Vector2Cell : Cell2D<Vector2Cell, MatrixModel>
    {
        public Vector2 vector { get; set; }
        public Vector2Cell(Vector2 vector, float x, float y) : base(x, y)
        {
            this.vector = vector;
        }

        public override void add(Vector2Cell cell, float impact)
        {
            vector.set(vector.x + cell.vector.x * impact, vector.y + cell.vector.y * impact);
        }

        public override void subtract(Vector2Cell cell, float impact)
        {
            vector.set(vector.x - cell.vector.x * impact, vector.y - cell.vector.y * impact);
        }

        public override void multiply(Vector2Cell cell, float impact)
        {
            x = x * cell.x * impact;
            y = y * cell.y * impact;
        }

        public override void divide(Vector2Cell cell, float impact)
        {
            vector.x = vector.x.interpolate(vector.x / cell.x, impact);
            vector.y = vector.y.interpolate(vector.y / cell.y, impact);
        }

        public override void average(Vector2Cell cell, float impact)
        {
            vector.set(Maths.average(vector.x, cell.vector.x, impact), Maths.average(vector.y, cell.vector.y, impact));
        }

        public override Vector2Cell clone()
        {
            return new Vector2Cell(vector, x, y);
        }

        public override float difference(Vector2Cell cell)
        {
            return (float)Math.Sqrt((vector.x.difference(cell.vector.x).pow(2f) + vector.y.difference(cell.vector.y).pow(2f)));
        }

        public override void draw(CellMatrix<Vector2Cell, MatrixModel> matrix, Pixmap pixmap)
        {
            //TODO: Draw vector arrows
        }
        
        public override void mask(float alpha)
        {
            x *= alpha;
            y *= alpha;
        }

        public override void push(Vector2Cell cell, float impact)
        {
            multiply(cell, impact);
        }

        public override void screen(Vector2Cell cell, float impact)
        {
            x = x.interpolate(Maths.screen(x, cell.x), impact);
            y = y.interpolate(Maths.screen(y, cell.y), impact);
        }
    }
}
