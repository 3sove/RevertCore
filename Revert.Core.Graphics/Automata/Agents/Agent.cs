using Revert.Core.Graphics.Automata.Kernels;
using Revert.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.Agents
{
    public abstract class Agent<T, TModel> where T : Cell2D<T, TModel> where TModel : MatrixModel
    {
        public T[][] Cells { get; }
        public bool EdgeAwareX { get; }
        public bool EdgeAwareY { get; }

        protected float x = 0f;
        protected float y = 0f;

        public Kernel kernel = PointKernel.Default;

        public Agent(T[][] cells, bool edgeAwareX, bool edgeAwareY)
        {
            Cells = cells;
            EdgeAwareX = edgeAwareX;
            EdgeAwareY = edgeAwareY;
        }


        //public abstract T[] getEmptyArray(int size);
        public abstract int direction { get; }
        public abstract int stepSize { get; }


        public bool meander(int steps)
        {
            var startX = Maths.randomIndex(0, Cells[0].Length);
            var startY = Maths.randomIndex(0, Cells.Length / 2);
            return meander(startX, startY, steps);
        }

        public bool meander(int revolutions, int stepsPerRevolution)
        {
            for (int i = 0; i < revolutions; i++)
            {
                var startX = Maths.randomIndex(0, Cells[0].Length - 1);
                var startY = Maths.randomIndex(0, Cells.Length - 1);
                if (!meander(startX, startY, stepsPerRevolution)) return false;
            }
            return true;
        }

        public bool meander(int revolutions, int stepsPerRevolution, int startX, int startY)
        {
            for (int i = 0; i < revolutions; i++)
            {
                if (!meander(startX, startY, stepsPerRevolution)) return false;
            }
            return true;
        }

        public bool meander(int startX, int startY, int steps)
        {
            x = startX;
            y = startY;

            var brushMap = kernel.map;
            var step = 0;
            while (step < steps)
            {
                if (direction == -1)
                {
                    if (!pathIntersected()) break;
                    step += stepSize;
                    continue;
                }

                var targetX = (int)x;
                var targetY = (int)y;

                for (int brushY = 0; brushY < brushMap.Length; brushY++)
                {

                    var brushMapRow = brushMap[brushY];
                    var brushTargetY = targetY + brushY;

                    if (EdgeAwareY) brushTargetY %= Cells.Length;
                    if (brushTargetY >= Cells.Length) break;

                    var brushTargetRow = Cells[brushTargetY];
                    for (int brushX = 0; brushX < brushMapRow.Length; brushX++)
                    {

                        var targetImpact = brushMapRow[brushX];
                        if (targetImpact == 0f) continue;

                        var brushTargetX = targetX + brushX;
                        if (EdgeAwareX) brushTargetX %= brushTargetRow.Length;
                        if (brushTargetX >= brushTargetRow.Length) break;

                        var brushTarget = brushTargetRow[brushTargetX];
                        act(brushTarget, targetImpact);
                    }
                }

                var i = 0;
                while (i < stepSize)
                {
                    targetX = (Cells[0].Length + NeighborDirections.GetNeighborX(targetX, direction)) % Cells[0].Length;
                    targetY = (Cells.Length + NeighborDirections.GetNeighborY(targetY, direction)) % Cells.Length;
                    i++;
                }

                x = targetX;
                y = targetY;
                step += stepSize;
            }

            return true;
        }

        public abstract bool pathIntersected();

        public abstract void act(T target, float a);

    }
}
