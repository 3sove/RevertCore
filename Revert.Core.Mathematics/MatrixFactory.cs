using System;
using System.Collections.Generic;
using System.Text;
using Revert.Core.Extensions;
using Revert.Core.Mathematics.Matrices;

namespace Revert.Core.Mathematics
{
    public class MatrixFactory
    {
        public static MatrixFactory Instance = new MatrixFactory();

        private Dictionary<KeyValuePair<int, int>, Queue<Matrix>> matrices = new Dictionary<KeyValuePair<int, int>, Queue<Matrix>>();

        public Matrix GetMatrix(double[] values)
        {
            var matrix = GetMatrix(values.Length, 1);
            for (var x = 0; x < values.Length; x++)
                matrix.Value[x][0] = values[x];
            return matrix;
        }

        public Matrix GetMatrix(int rows, int columns)
        {
            var kvp = new KeyValuePair<int, int>(rows, columns);
            Matrix matrix;
            if (!matrices.TryGetFromCollection(kvp, out matrix))
                matrix = new Matrix(rows, columns);
            return matrix;
        }

        public void PutMatrix(Matrix matrix)
        {
            var rows = matrix.Value.Length;
            var columns = matrix.Value[0].Length; 
            var kvp = new KeyValuePair<int, int>(rows, columns);
            matrices.AddToCollection(kvp, matrix);
        }


    }
}
