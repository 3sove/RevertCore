using Revert.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.Alpha
{
    public class AlphaCell : Cell2D<AlphaCell, AlphaMatrixModel>
    {
        public float Value { get; set; }

        public float AlphaPermeability { get; set; } = 0.5f;

        public AlphaCell(float x, float y, float value) : base(x, y)
        {
            Value = value;
        }

        public override void add(AlphaCell cell, float impact)
        {
            Value += cell.Value;
            Value = Value.clamp(0f, 1f);
        }

        public override void average(AlphaCell cell, float impact)
        {
            Value = Maths.average(Value, cell.Value, impact);
            Value = Value.clamp(0f, 1f);
        }

        public override AlphaCell clone()
        {
            return new AlphaCell(x, y, Value);
        }

        public override float difference(AlphaCell cell)
        {
            return Value.difference(cell.Value);
        }

        public override void divide(AlphaCell cell, float impact)
        {
            Value = Value.interpolate(Value / cell.Value, impact);
            Value = Value.clamp(0f, 1f);
        }

        public override void draw(CellMatrix<AlphaCell, AlphaMatrixModel> matrix, Pixmap pixmap)
        {
            var color = (matrix as AlphaMatrix).color;
            pixmap.setColor(color.R, color.G, color.B, Value);
            pixmap.drawPixel((int)x, (int)y);
        }

        public override void mask(float alpha)
        {
            Value = alpha;
        }

        public override void multiply(AlphaCell cell, float impact)
        {
            Value = Value.interpolate(Value * cell.Value, impact);
            Value = Value.clamp(0f, 1f);
        }

        public override void push(AlphaCell cell, float impact)
        {
            average(cell, impact);
        }

        public override void screen(AlphaCell cell, float impact)
        {
            Value = Value.interpolate(Maths.screen(Value, cell.Value), impact);
            Value = Value.clamp(0f, 1f);
        }

        public override void subtract(AlphaCell cell, float impact)
        {
            Value -= cell.Value;
            Value = Value.clamp(0f, 1f);
        }
    }
}
