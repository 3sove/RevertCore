using System;
using System.Collections.Generic;
using System.Text;
using Revert.Core.Graphics.Clusters;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Operations;

namespace Revert.Core.Graphics.Automata.Agents
{
    public abstract class DirectionBiasedWanderer<T, TModel> : Wanderer<T, TModel> where T : Cell2D<T, TModel> where TModel : MatrixModel
    {
        private float[] directionBias;
        private static float defaultBias = 1f / 8f;
        //bottom left counter clockwise
        private static float[] DEFAULT_BIAS = new[] { defaultBias, defaultBias, defaultBias, defaultBias, defaultBias, defaultBias, defaultBias, defaultBias };

        public DirectionBiasedWanderer(T[][] cells, Operations[] operationStack, float entropy, float impact, bool edgeAwareX, bool edgeAwareY) : base(cells, operationStack, entropy, impact, edgeAwareX, edgeAwareY)
        {
            directionBias = DEFAULT_BIAS;
            biasedClusterNeighbors = new T[8];
        }

        public DirectionBiasedWanderer(T[][] cells, Operations[] operationStack, float entropy, float impact, float[] directionBias, bool edgeAwareX, bool edgeAwareY) : base(cells, operationStack, entropy, impact, edgeAwareX, edgeAwareY)
        {
            this.directionBias = directionBias;
            biasedClusterNeighbors = new T[8];
        }

        public bool rememberDirection { get; set; } = false;

        public float directionShiftProbability { get; set; } = 0f;

        public T[] biasedClusterNeighbors;

        public override int direction
        {
            get
            {
                var neighbors = MapClusters.getNeighbors(Cells, (int)x, (int)y, EdgeAwareX, EdgeAwareY, biasedClusterNeighbors);
                var starting = directionBias.randomIndex();
                if (Maths.randomBoolean(directionShiftProbability)) directionBias = directionBias.rotate(1);

                var i = starting;
                while (i - starting < neighbors.Length)
                {
                    var awareI = i % neighbors.Length;
                    var neighbor = neighbors[awareI];
                    if (neighbor != null)
                    {
                        if (rememberDirection)
                        {
                            var bias = directionBias[awareI];
                            directionBias.setProbability(awareI, bias + (1 - bias) * .2f);
                        }
                        return i;
                    }
                    i++;
                }
                return -1;
            }
        }


    }
}
