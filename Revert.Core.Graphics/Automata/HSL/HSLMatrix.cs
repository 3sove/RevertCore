using Revert.Core.Extensions;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Interpolations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Revert.Core.Graphics.Automata.HSL
{
    public class HSLMatrix : CellMatrix<HSLCell, HSLMatrixModel>
    {
        public HSLMatrix(HSLMatrixModel model) : base(model)
        {
        }

        public override bool alphaCompare(HSLCell cell)
        {
            return cell.Lightness <= model.LightnessAlphaThreshold;
        }

        public override CellMatrix<HSLCell, HSLMatrixModel> clone()
        {
            var copy = new HSLMatrix(model);
            copy.populateCells(this);
            return copy;
        }

        protected override HSLCell generate()
        {
            return new HSLCell(0f, 0f, 0f, 0f, 0f, 0f);
        }

        protected override HSLCell[][] generateMatrix(int width, int height)
        {
            return Maths.CreateJagged<HSLCell>(height, width).fill(generate());
        }

        public void populateCells(Pixmap pixmap)
        {
            Color pixel = new Color();
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    pixel = Color.FromArgb(pixmap.getPixel(x, y));
                    var hslColor = new HSLColor(pixel);
                    var cell = new HSLCell(x, y, hslColor.hue, hslColor.saturation, hslColor.luminance, hslColor.alpha);
                    matrixCells[y][x] = cell;
                }
            }
        }

        private void populateCells(HSLMatrix hslMatrix)
        {
            var hslCells = hslMatrix.cells;
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    matrixCells[y][x] = hslCells[y][x].clone();
                }
            }
        }

        public void populateCells(HSLColor color, float noise, float alpha)
        {
            populateCells(color.hue / 360, color.saturation / 100, color.luminance / 100, noise, alpha);
        }

        public void populateCells(float hue, float saturation, float lightness, float noise, float alpha)
        {
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var cell = new HSLCell(x, y, hue.vary(noise) % 1,
                        saturation.vary(noise, 0f, 1f), lightness.vary(noise, 0f, 1f), alpha);
                    matrixCells[y][x] = cell;
                }
            }
        }

        public void populateCells(float hue, float huePermeability, float saturation, float saturationPermeability, float lightness, float lightnessPermeability, float noise, float alpha)
        {
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var cell = new HSLCell(x, y,
                            hue.vary(noise) % 1, huePermeability,
                            saturation.vary(noise, 0f, 1f), saturationPermeability,
                            lightness.vary(noise, 0f, 1f), lightnessPermeability,
                            alpha);
                    matrixCells[y][x] = cell;
                }
            }
        }

        public void lightnessDescent(float[] heights, float from, float to, Interpolation interpolation)
        {
            for (int x = 0; x < model.Width; x++)
            {
                var relativeX = x.relativePosition(model.Width);
                var groundHeight = heights.getRelative(relativeX);

                var groundY = (1f - groundHeight).getIndex(model.Height); //Matrix 0,0 is top left

                for (int y = 0; y < model.Height; y++)
                {
                    var relativePosition = y.relativePosition(groundY, model.Height);
                    var targetLightness = interpolation.apply(from, to, relativePosition);
                    matrixCells[y][x].Lightness = targetLightness;
                }
            }
        }

    }
}
