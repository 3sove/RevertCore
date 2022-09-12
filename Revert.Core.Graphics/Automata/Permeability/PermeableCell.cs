using Revert.Core.Graphics.Automata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.Permeability
{
    public class PermeableCell : Cell2D<PermeableCell, PermeableMatrixModel>
    {
        public Dictionary<int, PermeableValue> ValueMap = new Dictionary<int, PermeableValue>();

        //public PermeableCell(float x, float y) : base(x, y)
        //{
        //}

        public PermeableCell(float x, float y, ICollection<PermeableValue> collection) : base(x, y)
        {
            foreach (var value in collection) ValueMap[value.Id] = value;
        }

        public override void add(PermeableCell cell, float impact)
        {
            throw new NotImplementedException();
        }

        public override void average(PermeableCell cell, float impact)
        {
            throw new NotImplementedException();
        }

        public override PermeableCell clone()
        {
            var values = new List<PermeableValue>();

            foreach (var value in ValueMap)
            {
                values.Add(value.Value);
            }

            return new PermeableCell(x, y, values);
        }

        public override float difference(PermeableCell cell)
        {
            throw new NotImplementedException();
        }

        public override void divide(PermeableCell cell, float impact)
        {
            throw new NotImplementedException();
        }

        public override void draw(CellMatrix<PermeableCell, PermeableMatrixModel> matrix, Pixmap pixmap)
        {
            throw new NotImplementedException();
        }

        public override void mask(float alpha)
        {
            throw new NotImplementedException();
        }

        public override void multiply(PermeableCell cell, float impact)
        {
            throw new NotImplementedException();
        }

        public override void push(PermeableCell cell, float impact)
        {
            throw new NotImplementedException();
        }

        public override void screen(PermeableCell cell, float impact)
        {
            throw new NotImplementedException();
        }

        public override void subtract(PermeableCell cell, float impact)
        {
            throw new NotImplementedException();
        }
    }
}
