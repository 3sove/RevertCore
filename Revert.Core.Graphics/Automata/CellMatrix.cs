using Revert.Core.Extensions;
using Revert.Core.Graphics.Clusters;
using Revert.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata
{
    public abstract class CellMatrix<T, TModel> where T : Cell<T, TModel> where TModel : MatrixModel
    {
        public T[][] matrixCells;
        public virtual T[][] cells { get; set; }

        public TModel model { get; set; }

        public CellMatrix(TModel model)
        {
            this.model = model;
            matrixCells = generateMatrix(model.Width, model.Height);
            cells = matrixCells;
        }

        public CellMatrix(TModel model, T[][] cells) : this(model)
        {
            this.cells = cells;
        }

        public int count
        {
            get
            {
                return cells.Length * cells[0].Length;
            }
        }

        public Pixmap matrixPixmap = null;

        public void step(int stepCount, bool edgeAwareX, bool edgeAwareY, bool orderedSteps = false)
        {
            var i = 0;
            while (i < stepCount)
            {
                if (orderedSteps) orderedStep(edgeAwareX, edgeAwareY);
                else randomStep(edgeAwareX, edgeAwareY);
                i++;
            }
        }

        private void randomStep(bool edgeAwareX, bool edgeAwareY)
        {
            var matrix = this;
            var locations = Maths.getRandomMatrixLocations(model.Width, model.Height);
            foreach (var location in locations)
            {
                cells[(int)location.y][(int)location.x].step(matrix, edgeAwareX, edgeAwareY);
            }
        }

        private void orderedStep(bool edgeAwareX, bool edgeAwareY)
        {
            for (int y = 0; y < cells.Length; y++)
                for (int x = 0; x < cells[y].Length; x++)
                    cells[y][x].step(this, edgeAwareX, edgeAwareY);
        }

        //var pixmapFilter: Pixmap.Filter = Pixmap.Filter.BiLinear
        private const bool WRITE_TEXTURE = false;

        public Pixmap getPixmap()
        {
            

            if (matrixPixmap == null)
            {
                var matrixPixmap = new Pixmap(model.Width, model.Height);

                foreach (var column in cells)
                {
                    foreach (var cell in column)
                    {
                        cell.draw(this, matrixPixmap);
                    }
                }

                if (WRITE_TEXTURE)
                {
                    //var file = Gdx.files.local("$model.matrixName - ${System.currentTimeMillis()}.png");
                    //PixmapIO.writePNG(file, matrixPixmap);
                }
            }
            return matrixPixmap;
        }

        //    fun getDifferenceMap(matrix: CellMatrix<T, TModel>): Array<FloatArray> {
        //        var newItem = generate()
        //        var alphaMap = Array(height) { FloatArray(width) }
        //        var cells = cells
        //        var newCells = generateMatrix(width, height)
        //        return difference(cells, newCells, 1f)
        //    }

        public float[][] getAlphaMap(CellMatrix<T, TModel> matrix)
        {
            var alphaMap = Maths.CreateJagged<float>(model.Height, model.Width);

            for (int y = 0; y < cells.Length; y++)
            {
                var column = cells[y];
                for (int x = 0; x < column.Length; x++)
                {
                    var cell = column[x];
                    alphaMap[y][x] = alphaCompare(cell) ? 1f : 0f;
                    cell.draw(matrix, matrixPixmap);
                }
            }
            return alphaMap;
        }

        public float[][] difference(T[][] aCells, T[][] bCells, float impact)
        {
            var diffs = Maths.CreateJagged<float>(cells.Length, cells[0].Length);
            for (int y = 0; y < aCells.Length; y++)
                for (int x = 0; x < aCells[0].Length; x++)
                {
                    diffs[y][x] = aCells[y][x].difference(bCells[y][x]);
                }
            return diffs;
        }

        public void add(CellMatrix<T, TModel> b, float impact)
        {
            add(cells, b.cells, impact);
        }

        public T[][] add(T[][] a, T[][] b, float impact)
        {
            for (int y = 0; y < a.Length; y++)
            {
                for (int x = 0; x < a[y].Length; x++)
                {
                    a[y][x].add(b[y][x], impact);
                }
            }
            return a;
        }

        public void subtract(CellMatrix<T, TModel> b, float impact)
        {
            subtract(cells, b.cells, impact);
        }

        public T[][] subtract(T[][] a, T[][] b, float impact)
        {
            for (int y = 0; y < a.Length; y++)
                for (int x = 0; x < a[y].Length; x++)
                    a[y][x].subtract(b[y][x], impact);
            return a;
        }

        public void multiply(CellMatrix<T, TModel> b, float impact)
        {
            multiply(cells, b.cells, impact);
        }

        public T[][] multiply(T[][] a, T[][] b, float impact)
        {
            var result = generateMatrix(a[0].Length, a.Length);

            for (int aY = 0; aY < a.Length; aY++)
            {
                var aRow = a[aY];
                var aRowVector = getRowVector(a, aY);

                var relativeY = aY.relativePosition(a.Length);
                var yIndex = relativeY.getIndex(a);

                for (int aX = 0; aX < aRow.Length; aX++)
                {
                    var relativeX = aX.relativePosition(aRow.Length);
                    var xIndex = relativeX.getIndex(aRow);

                    var columnVector = getColumnVector(b, xIndex);
                    result[yIndex][xIndex] = dotProduct(aRowVector, columnVector, impact);
                }
            }
            //a[y][x].multiply(b[y][x], impact);
            return result;
        }

        public T dotProduct(T[] a, T[] b, float impact)
        {
            T result = null;
            for (int i = 0; i < a.Length; i++)
            {
                var aItem = a[i];

                for (var j = 0; j < b.Length; j++)
                {
                    var bItem = b[j];

                    aItem.multiply(bItem, impact);
                    if (result == null)
                        result = aItem;
                    else
                        result.add(aItem, impact);
                }
            }

            return result as T;
        }

        public T[] getColumnVector(T[][] matrix, int columnIndex)
        {
            var column = new T[matrix.Length];
            for (int y = 0; y < matrix.Length; y++)
            {
                var row = matrix[y];
                column[y] = row[columnIndex];
            }
            return column;
        }

        public T[] getRowVector(T[][] matrix, int y)
        {
            return matrix[y];
        }

        public void divide(CellMatrix<T, TModel> b, float impact)
        {
            divide(cells, b.cells, impact);
        }

        public T[][] divide(T[][] a, T[][] b, float impact)
        {
            for (int y = 0; y < a.Length; y++)
                for (int x = 0; x < a[0].Length; x++)
                    a[y][x].multiply(b[y][x], impact);

            return a;
        }

        public void screen(CellMatrix<T, TModel> b, float impact)
        {
            screen(cells, b.cells, impact);
        }

        public T[][] screen(T[][] a, T[][] b, float impact)
        {
            for (int y = 0; y < a.Length; y++)
                for (int x = 0; x < a[0].Length; x++)
                    a[y][x].screen(b[y][x], impact);

            return a;
        }

        public void mask(float[][] maskMatrix)
        {
            var relativeX = 0f;
            var relativeY = 0f;

            for (int y = 0; y < cells.Length; y++)
            {
                var row = cells[y];
                relativeY = (float)y / (float)cells.Length;
                var maskRow = maskMatrix[relativeY.getIndex(maskMatrix)];

                for (int x = 0; x < row.Length; x++)
                {
                    var cell = row[x];
                    relativeX = (float)x / (float)row.Length;
                    var maskValue = maskRow[relativeX.getIndex(maskRow)];
                    cell.mask(maskValue);
                }
            }
        }

        public abstract bool alphaCompare(T cell);

        protected abstract T generate();

        protected abstract T[][] generateMatrix(int width, int height);

        public T getCell(int x, int y, int direction)
        {
            return MapClusters.getNeighbor(cells, x, y, direction);
        }

        public abstract CellMatrix<T, TModel> clone();

        public T[][] cloneCells()
        {
            return cells.copy();
        }

    }
}
