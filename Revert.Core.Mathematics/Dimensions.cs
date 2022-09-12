using Revert.Core.Mathematics.Matrices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Mathematics
{
    public class Dimensions
    {
        /// <summary>
        /// Columns
        /// </summary>
        public int X { get; set; }
        
        /// <summary>
        /// Rows
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Layers
        /// </summary>
        public int Z { get; set; }

        public Dimensions(int x, int y)
        {
            Y = y;
            X = x;
            Z = 1;
        }

        public Dimensions(int x, int y, int z)
        {
            Y = y;
            X = x;
            Z = z;
        }

        public static Dimensions CalculateDimensions(Matrix input, Matrix output)
        {
            //Calculating dimensions of activation matrix
            var sqrtInput = Math.Sqrt(input.Value.Length);
            var sqrtCalculated = Math.Sqrt(output.Value.Length);

            var sqrtRatio = sqrtInput / sqrtCalculated;
            var outputColumns = (int)(input.Dimensions.X / sqrtRatio);
            var outputRows = (int)(input.Dimensions.Y / sqrtRatio);
            return new Dimensions(outputColumns, outputRows);
        }

        public override string ToString()
        {
            return $"{Y}x{X}x{Z} (YxXxZ)";
        }
    }
}
