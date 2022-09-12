using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Drawing;

namespace Revert.Core.Graphics.Images
{
    public abstract class PngReader
    {

        public abstract ImageModel getImageModel(System.IO.FileStream file, float scalingFactor);

        public int[][] getColorMap(string path, float scalingFactor)
        {
            var imageModel = getImageModel(System.IO.File.OpenRead(path), scalingFactor);
            return imageModel.pixelMap.Reverse().ToArray();
        }

        public float[][] getAlphaMap(string path, float scalingFactor)
        {
            var imageModel = getImageModel(System.IO.File.OpenRead(path), scalingFactor);
            var pixelMap = imageModel.pixelMap;
            var reversedMap = pixelMap.Reverse();
            var map = new float[pixelMap.Length][];


            for (int row = 0; row < pixelMap.Length; row++)
            {
                map[row] = new float[pixelMap[row].Length];
                for (int column = 0; column < pixelMap[row].Length; column++)
                {
                    var color = Color.FromArgb(pixelMap[row][column]);
                    map[row][column] = color.A;
                }
            }

            return map;
        }
    }
}
