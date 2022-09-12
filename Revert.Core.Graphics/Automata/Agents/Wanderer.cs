using Revert.Core.Graphics.Clusters;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Operations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.Agents
{
    public abstract class Wanderer<T, TModel> : Agent<T, TModel> where T : Cell2D<T, TModel> where TModel : MatrixModel
    {
        public Wanderer(T[][] cells, Operations[] operationStack, float entropy, float impact, bool edgeAwareX, bool edgeAwareY) : base(cells, edgeAwareX, edgeAwareY)
        {
            OperationStack = operationStack;
            Entropy = entropy;
            Impact = impact;
            ClusterNeighbors = new T[8]; // getEmptyArray(8);
            WanderingCell = getNewWanderingCell();
        }

        public T WanderingCell { get; set; }

        public T[] ClusterNeighbors { get; set; }
        public override int direction
        {
            get
            {
                var neighbors = MapClusters.getNeighbors(Cells, (int)x, (int)y, EdgeAwareX, EdgeAwareY, ClusterNeighbors);
                var starting = Maths.randomIndex(0, neighbors.Length - 1);
                var i = starting;
                while (i - starting < neighbors.Length)
                {
                    var neighbor = neighbors[i % neighbors.Length];
                    if (neighbor != null) return i;
                    i++;
                }
                return -1;
            }
        }

        public Operations[] OperationStack { get; set; }
        public float Entropy { get; set; }
        public float Impact { get; }

        public override void act(T target, float a)
        {
            var cell = getNewWanderingCell();
            cell = modify(cell, Entropy);

            var totalImpact = Impact * a;
            foreach (var operation in OperationStack)
            {
                switch (operation)
                {
                    case Operations.ADD:
                        target.add(cell, totalImpact);
                        break;
                    case Operations.SUBTRACT:
                        target.subtract(cell, totalImpact);
                        break;
                    case Operations.MULTIPLY:
                        target.multiply(cell, totalImpact);
                        break;
                    case Operations.DIVIDE:
                        target.divide(cell, totalImpact);
                        break;
                    case Operations.AVERAGE:
                        target.average(cell, totalImpact);
                        break;
                    case Operations.SCREEN:
                        target.screen(cell, totalImpact);
                        break;
                }
            }
        }

        public override bool pathIntersected()
        {
            return true;
        }

        public abstract T getNewWanderingCell();

        public abstract T modify(T cell, float entropy);

    }
}
