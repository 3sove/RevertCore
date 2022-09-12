using Microsoft.Xna.Framework.Graphics;
using Revert.Core.Extensions;
using Revert.Core.Graphics.Automata.HSL;
using Revert.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Revert.Core.Graphics
{
    public class Pixmap
    {
        public int width;
        public int height;
        public int[][] map;

        public Pixmap(int width, int height)
        {
            this.width = width;
            this.height = height;
            map = Maths.CreateJagged<int>(height, width);
        }

        public Color color = Color.Black;

        public void setColor(float r, float g, float b, float a)
        {
            color = Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }

        public void setColor(Color color)
        {
            this.color = color;
        }

        public void drawPixel(int x, int y)
        {
            map[y][x] = color.ToArgb();
        }

        public void fillRectangle(int y, int x, int height, int width)
        {
            for (int my = y; my < y + height; my++)
            {
                for (int mx = x; mx < x + width; mx++)
                {
                    map[my][mx] = color.ToArgb();
                }
            }
        }

        internal int getPixel(int x, int y)
        {
            return map[y][x];
        }


        public static Pixmap mask(Pixmap pixmap, float[][] maskMatrix)
        {
            var pixmapHeight = pixmap.height;
            var pixmapWidth = pixmap.width;

            var target = new Pixmap(pixmap.width, pixmap.height);
            Color pixelColor;

            var y = 0;

            while (y < pixmapHeight)
            {
                var relativeY = y.relativePosition(pixmapHeight);
                var yIndex = relativeY.getIndex(maskMatrix);
                var maskRow = maskMatrix[yIndex];

                var x = 0;
                while (x < pixmapWidth)
                {
                    var relativeX = x.relativePosition(pixmapWidth);
                    var xIndex = relativeX.getIndex(maskRow);
                    var maskValue = maskRow[xIndex];

                    pixelColor = Color.FromArgb(pixmap.getPixel(x, y));
                    pixelColor = Color.FromArgb((int)(pixelColor.A * maskValue), pixelColor.R, pixelColor.G, pixelColor.B);
                    target.setColor(pixelColor);
                    target.drawPixel(x, y);
                    x++;
                }
                y++;
            }
            return target;
        }

        public static Pixmap outline(Pixmap pixmap)
        {
            var matrix = new HSLMatrix(new HSLMatrixModel(pixmap.width, pixmap.height, "Outline"));
            matrix.populateCells(pixmap);

            var blurred = matrix.clone() as HSLMatrix;
            blurred.step(1, false, false);

            return pixmap;
        }


        public Texture2D GetTexture()
        {
            GraphicsDevice newGraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, new PresentationParameters());
            var texture = new Texture2D(newGraphicsDevice, map[0].Length, map.Length);

            var flatMap = map.Flatten();
            var mapColors = flatMap.Select(f => Color.FromArgb(f)).ToArray();

            var rect = new Microsoft.Xna.Framework.Rectangle(0, 0, map[0].Length, map.Length);
            texture.SetData(0, rect, mapColors, 0, mapColors.Length);
            return texture;
        }
    }
}
