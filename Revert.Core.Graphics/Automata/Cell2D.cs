using Revert.Core.Graphics.Clusters;
using Revert.Core.Mathematics.Operations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata
{
    public abstract class Cell2D<T, TModel> : Cell<T, TModel> where T : Cell2D<T, TModel> where TModel : MatrixModel
    {
        public float x { get; set; }
        public float y { get; set; }

        public Cell2D(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public T[] neighborArray { get; set; } = new T[8];

        public override void step(CellMatrix<T, TModel> matrix, Queue<Operation<T>> operationQueue, float impact, bool edgeAwareX, bool edgeAwareY)
        {
            var neighbors = MapClusters.getNeighbors(matrix.cells, (int)x, (int)y, edgeAwareX, edgeAwareY, neighborArray);

            for (int i = 0; i < neighbors.Length; i++)
            {
                var neighbor = neighbors[i];
                if (neighbor == null) continue;
                foreach (var operation in operationQueue)
                {
                    operation.perform(this as T, neighbor, impact);
                }
            }
        }

        //diffuses the values from the passed Cell onto the values of this cell
        public override void step(CellMatrix<T, TModel> matrix, bool edgeAwareX, bool edgeAwareY)
        {
            var neighbors = MapClusters.getNeighbors(matrix.cells, (int)x, (int)y, edgeAwareX, edgeAwareY, neighborArray);

            for (int i = 0; i < neighbors.Length; i++)
            {
                var neighbor = neighbors[i];
                if (neighbor == null) continue;
                neighbor.push((T)this, 1f);
            }
        }

    }
}
