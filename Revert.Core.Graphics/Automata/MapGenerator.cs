using Revert.Core.Graphics.Images;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Interpolations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revert.Core.Graphics.Automata
{
    public class MapGenerator
    {

        public float[] getHeightMap(int width, Interpolation interpolation)
        {
            var heights = new float[width];
            heights[0] = interpolation.apply(0f); //get the first item
                                                  //get everything between the first and last item
            for (var i = 1; i < width - 1; i++)
            {
                heights[i] = interpolation.apply(i / width);
            }
            heights[width - 1] = interpolation.apply(1f); //get the last item
            return heights;
        }

        public float[] getHeightDistributionMap(int width, int samples, Interpolation interpolation)
        {
            var heights = new float[width];
            for (int i = 0; i < samples; i++)
            {
                var randomIndex = (int)Maths.randomFloat(0f, heights.Length - 1f, interpolation);
                heights[randomIndex]++;
            }

            var max = heights.Max();

            for (int i = 0; i < heights.Length; i++)
            {
                heights[i] = heights[i] / max;
            }

            return heights;
        }

        public float[] getHeightMap(string imagePath, PngReader pngReader, float smoothingPower)
        {
            var map = pngReader.getAlphaMap(imagePath, .5f);

            var heights = new float[map[0].Length];
            for (var y = 0; y < map.Length; y++)
            {
                var abstractRow = map[y];

                for (var x = 0; x < abstractRow.Length; x++)
                {
                    if (abstractRow[x] != 0f)
                        heights[x] += abstractRow[x];
                }
            }

            for (int i = 0; i < heights.Length; i++)
            {
                heights[i] = (float)(Math.Pow(heights[i], smoothingPower) / map.Length);
            }

            return heights;
        }

        public float[][] getAlphaMap(float[] heights, int width, int height, float floor)
        {
            var alphaMap = new float[height][];

            for (int mapIndex = 0; mapIndex < alphaMap.Length; mapIndex++)
                alphaMap[mapIndex] = new float[width];

            for (int y = 0; y < alphaMap.Length; y++)
            {
                var mapRow = alphaMap[y];
                var relativeY = y.relativePosition(alphaMap.Length);
                for (int x = 0; x < mapRow.Length; x++)
                {
                    var relativeX = x.relativePosition(mapRow.Length);
                    var xIndex = relativeX.getIndex(heights);
                    if (relativeY <= heights[xIndex] || relativeY < floor)
                        alphaMap[y][x] = 1f;
                }
            }
            return alphaMap;
        }
    }
}
