using Revert.Core.Graphics.Clusters;
using Revert.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Automata.HSL
{
    public class HSLCell : Cell2D<HSLCell, HSLMatrixModel>
    {
        public HSLCell(float x, float y, float hue, float saturation, float lightness, float alpha) : base(x, y)
        {
            Hue = hue;
            Saturation = saturation;
            Lightness = lightness;
            Alpha = alpha;
        }

        public HSLCell(float x, float y, float Hue, float huePermeability, float Saturation,
            float saturationPermeability, float Lightness, float lightnessPermeability, float Alpha) :
            this(x, y, Hue, Saturation, Lightness, Alpha)
        {
            HuePermeability = huePermeability;
            SaturationPermeability = saturationPermeability;
            LightnessPermeability = lightnessPermeability;
        }

        public float Hue { get; set; }
        public float Saturation { get; set; }
        public float Lightness { get; set; }
        public float Alpha { get; set; }
        public float HuePermeability { get; set; } = Maths.randomFloat(.3f, .7f);
        public float SaturationPermeability { get; set; } = Maths.randomFloat(.7f, .9f);
        public float LightnessPermeability { get; set; } = Maths.randomFloat(.6f, .8f);

        public float AlphaPermeability { get; set; } = 0f;

        public override void add(HSLCell cell, float impact)
        {
            var HueDiff = Hue.difference(cell.Hue, 0f, 1f) * impact * HuePermeability * cell.HuePermeability;
            Hue = Maths.edgeAwareAdd(Hue, HueDiff, 0f, 1f);
            Saturation = (Saturation + cell.Saturation * impact * SaturationPermeability * cell.SaturationPermeability).clamp(0f, 1f);
            Lightness = (Lightness + cell.Lightness * impact * LightnessPermeability * cell.LightnessPermeability).clamp(0f, 1f);
            Alpha = (Alpha + cell.Alpha * impact * AlphaPermeability * cell.AlphaPermeability).clamp(0f, 1f);
        }

        public override void subtract(HSLCell cell, float impact)
        {
            var HueDiff = Hue.difference(cell.Hue, 0f, 1f) * impact * HuePermeability * cell.HuePermeability;
            Hue = Maths.edgeAwareAdd(Hue, -HueDiff, 0f, 1f);
            Saturation = (Saturation - cell.Saturation * impact * SaturationPermeability * cell.SaturationPermeability).clamp(0f, 1f);
            Lightness = (Lightness - cell.Lightness * impact * LightnessPermeability * cell.LightnessPermeability).clamp(0f, 1f);
            Alpha = (Alpha - cell.Alpha * impact * AlphaPermeability * cell.AlphaPermeability).clamp(0f, 1f);
        }

        public override void multiply(HSLCell cell, float impact)
        {
            Hue = Hue.interpolate(Hue * cell.Hue, impact * HuePermeability * cell.HuePermeability);
            Saturation = Saturation.interpolate(Saturation * cell.Saturation, impact * SaturationPermeability * cell.SaturationPermeability);
            Lightness = Lightness.interpolate(Lightness * cell.Lightness, impact * LightnessPermeability * cell.LightnessPermeability);
            Alpha = Alpha.interpolate(Alpha * cell.Alpha, impact * AlphaPermeability * cell.AlphaPermeability);
        }

        public override void divide(HSLCell cell, float impact)
        {
            Hue = Hue.interpolate(Hue / cell.Hue, impact * HuePermeability * cell.HuePermeability);
            Saturation = Saturation.interpolate(Saturation / cell.Saturation, impact * SaturationPermeability * cell.SaturationPermeability);
            Lightness = Lightness.interpolate(Lightness / cell.Lightness, impact * LightnessPermeability * cell.LightnessPermeability);
            Alpha = Alpha.interpolate(Alpha / cell.Alpha, impact * AlphaPermeability * cell.AlphaPermeability);
        }

        public override void average(HSLCell cell, float impact)
        {
            Hue = Hue.interpolate(cell.Hue, impact * HuePermeability * cell.HuePermeability);
            Saturation = Saturation.interpolate(cell.Saturation, impact * SaturationPermeability * cell.SaturationPermeability);
            Lightness = Lightness.interpolate(cell.Lightness, impact * LightnessPermeability * cell.LightnessPermeability);
            Alpha = Alpha.interpolate(cell.Alpha, impact * AlphaPermeability * cell.AlphaPermeability);
        }

        public override float difference(HSLCell cell)
        {
            var h = cell.Hue.difference(Hue, 0f, 1f);
            var s = cell.Saturation - Saturation;
            var l = cell.Lightness - Lightness;
            var a = cell.Alpha - Alpha;
            return (h + s + l + a) / 4;
        }

        public override void screen(HSLCell cell, float impact)
        {
            Hue = Hue.interpolate(Maths.screen(Hue, cell.Hue), impact * HuePermeability * cell.HuePermeability);
            Saturation = Saturation.interpolate(Maths.screen(Saturation, cell.Saturation), impact * SaturationPermeability * cell.SaturationPermeability);
            Lightness = Lightness.interpolate(Maths.screen(Lightness, cell.Lightness), impact * LightnessPermeability * cell.LightnessPermeability);
            Alpha = Alpha.interpolate(Maths.screen(Alpha, cell.Alpha), impact * AlphaPermeability * cell.AlphaPermeability);
        }

        public override HSLCell clone()
        {
            return new HSLCell(x, y, Hue, Saturation, Lightness, Alpha);
        }

        public override void draw(CellMatrix<HSLCell, HSLMatrixModel> matrix, Pixmap pixmap)
        {
            var pixelColor = HSLColor.toRGB(Hue % 1 * 360, Saturation.clamp(0f, 1f) * 100, Lightness.clamp(0f, 1f) * 100, Alpha);
            pixmap.setColor(pixelColor.R, pixelColor.G, pixelColor.B, pixelColor.A);
            pixmap.drawPixel((int)x, (int)y);
        }

        public override void mask(float Alpha)
        {
            this.Alpha = Alpha;
        }

        public override void push(HSLCell cell, float impact)
        {
            var HueDifference = Hue.difference(cell.Hue, 0f, 1f);
            var effectiveDifference = HueDifference * HuePermeability * impact;
            Hue = Maths.edgeAwareAdd(Hue, effectiveDifference, 0f, 1f);
            Saturation = Saturation.interpolate(cell.Saturation, SaturationPermeability * impact);
            Lightness = Lightness.interpolate(cell.Lightness, LightnessPermeability * impact);
        }

        private void drawRectangle(Pixmap pixmap, int cellSize)
        {
            var rectColor = HSLColor.toRGB(Hue % 1 * 360, Saturation.clamp(0f, 1f) * 100, Lightness.clamp(0f, 1f) * 100, Alpha);
            pixmap.setColor(rectColor.R, rectColor.G, rectColor.B, rectColor.A);
            pixmap.fillRectangle((int)(x * cellSize), (int)(y * cellSize), cellSize, cellSize);
        }

        public void drawPixels(CellMatrix<HSLCell, HSLMatrixModel> cellMatrix, Pixmap pixmap, int cellSize)
        {
            var clusterNeighbors = new HSLCell[8];
            var neighborList = MapClusters.getNeighbors(cellMatrix.cells, (int)x, (int)y, false, false, clusterNeighbors);
            var neighbors = new HSLCell[8];
            for (int i = 0; i < neighborList.Length; i++)
            {
                neighbors[i] = neighborList[i];
            }

            var fromX = x * cellSize;
            var toX = fromX + cellSize;
            var fromY = y * cellSize;
            var toY = fromY + cellSize;

            var py = fromY;
            while (py < toY)
            {
                var px = fromX;
                while (px < toX)
                {
                    var totalDistance = 0f;

                    for (var direction = 0; direction < NeighborDirections.DIRECTION_COUNT; direction++)
                    {
                        var neighbor = neighbors[direction];
                        if (neighbor == null) continue;
                        totalDistance += Maths.distance(px, py, neighbor.x * cellSize, neighbor.y * cellSize);
                    }

                    var pixelHue = Hue;
                    var pixelSaturation = Saturation;
                    var pixelLightness = Lightness;
                    var pixelAlpha = Alpha;

                    for (var direction = 0; direction < NeighborDirections.DIRECTION_COUNT; direction++)
                    {
                        var neighbor = neighbors[direction];
                        if (neighbor == null) continue;

                        var distance = Maths.distance(px, py, neighbor.x * cellSize, neighbor.y * cellSize);
                        var impact = 1 - distance / totalDistance;

                        pixelHue = Hue.interpolate(neighbor.Hue, impact);
                        pixelSaturation = Saturation.interpolate(neighbor.Saturation, impact);
                        pixelLightness = Lightness.interpolate(neighbor.Lightness, impact);
                        pixelAlpha = Alpha.interpolate(neighbor.Alpha, impact);
                    }

                    var rgb = HSLColor.toRGB(pixelHue * 360, pixelSaturation * 100, pixelLightness * 100, pixelAlpha);
                    pixmap.setColor(rgb.R, rgb.G, rgb.B, rgb.A);
                    pixmap.drawPixel((int)px, (int)py);
                    px++;
                }
                py++;
            }
        }

    }
}
