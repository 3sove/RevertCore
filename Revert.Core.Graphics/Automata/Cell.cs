using Revert.Core.Mathematics.Operations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata
{
    public abstract class Cell<T, TModel> where T : Cell<T, TModel> where TModel : MatrixModel
    {
        public abstract void step(CellMatrix<T, TModel> matrix, Queue<Operation<T>> operationQueue, float impact, bool edgeAwareX, bool edgeAwareY);

        //diffuses the values from the passed Cell onto the values of this cell
        public abstract void step(CellMatrix<T, TModel> matrix, bool edgeAwareX, bool edgeAwareY);

        public void Push(T cell)
        {
            push(cell, 1f);
        }

        public abstract T clone();

        public abstract void push(T cell, float impact);

        public abstract void add(T cell, float impact);

        public abstract void subtract(T cell, float impact);

        public abstract void multiply(T cell, float impact);

        public abstract void divide(T cell, float impact);

        public abstract void average(T cell, float impact);

        public abstract void screen(T cell, float impact);

        public abstract float difference(T cell);

        public abstract void draw(CellMatrix<T, TModel> matrix, Pixmap pixmap);

        public abstract void mask(float alpha);
    }
}
