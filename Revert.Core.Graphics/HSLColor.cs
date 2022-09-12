using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Revert.Core.Graphics
{
    public class HSLColor
    {
        public Color rgb = Color.Empty;
        public float[] hsl;
        public float alpha = 0f;
        public float hue {  get { return hsl[0]; } }
        public float saturation { get { return hsl[1]; } }
        public float luminance { get { return hsl[2]; } }


        public HSLColor(Color rgb)
        {
            this.rgb = rgb;
            hsl = fromRGB(rgb);
            alpha = rgb.A / 255.0f;
    }

        public HSLColor(float hue, float saturation, float luminance, float alpha)
        {
            hsl = new[] { hue, saturation, luminance };
            this.alpha = alpha;
            rgb = toRGB(hsl, alpha);
    }

        public HSLColor(float[] hsl, float alpha = 1.0f)
        {
            this.hsl = hsl;
            this.alpha = alpha;
            rgb = toRGB(hsl, alpha);
        }


        public static float SaturationModifier = 1f;
        public static float LightnessModifier = 1f;

        public float[] fromRGB(Color color) {
            //  get RGB values in the range 0 - 1
            var rgb = new[] { color.R, color.G, color.B };
            var r = rgb[0];
            var g = rgb[1];
            var b = rgb[2];

            //	Minimum and Maximum RGB values are used in the HSL calculations
            var min = Math.Min(r, Math.Min(g, b));
            var max = Math.Max(r, Math.Max(g, b));

            //  Calculate the Hue

            var h = 0f;

            if (max == min) h = 0f;
            else if (max == r) h = (60 * (g - b) / (max - min) + 360) % 360;
            else if (max == g) h = 60 * (b - r) / (max - min) + 120;
            else if (max == b) h = 60 * (r - g) / (max - min) + 240;

            var l = (max + min) / 2;

            var s = 0f;

            if (max == min) s = 0f;
            else if (l <= .5f) s = (max - min) / (max + min);
            else s = (max - min) / (2f - max - min);
            return new[] { h, s * 100, l * 100 };
        }


        /**
         * Convert HSL values to a RGB Color.
         * H (Hue) is specified as degrees in the range 0 - 360.
         * S (Saturation) is specified as a percentage in the range 1 - 100.
         * L (Lumanance) is specified as a percentage in the range 1 - 100.
         *
         * @param hsl   an array containing the 3 HSL values
         * @param alpha the value value between 0 - 1
         * @returns the RGB Color object
         */

        public static Color toRGB(float[] hsl, float alpha = 1.0f) {
            return toRGB(hsl[0], hsl[1], hsl[2], alpha);
        }
        
        public Color rgbColor = Color.Empty;

        public static Color toRGB(float h, float s, float l, float alpha = 1.0f) {
            var hue = h;
            var saturation = s;
            var luminance = l;
            if (saturation< 0.0f || saturation> 100.0f) {
                var message = "Color parameter outside of expected range - Saturation";
                throw new Exception(message);
            }

            if (luminance< 0.0f || luminance> 100.0f) {
                var message = "Color parameter outside of expected range - Luminance";
                throw new ArgumentException(message);
            }

            if (alpha< 0.0f || alpha> 1.0f) {
                var message = "Color parameter outside of expected range - Alpha";
                throw new ArgumentException(message);
            }

            // Formula needs all values between 0 - 1.

            hue %= 360.0f;
            hue /= 360f;
            saturation /= 100f;
            luminance /= 100f;

            saturation *= SaturationModifier;
            luminance *= LightnessModifier;

            var q = 0f;

            if (luminance < 0.5)
                q = luminance * (1 + saturation);
            else
                q = luminance + saturation - saturation * luminance;

            var p = 2 * luminance - q;

            var r = Math.Max(0f, HueToRGB(p, q, hue + 1.0f / 3.0f));
            var g = Math.Max(0f, HueToRGB(p, q, hue));
            var b = Math.Max(0f, HueToRGB(p, q, hue - 1.0f / 3.0f));

            r = Math.Min(r, 1.0f);
            g = Math.Min(g, 1.0f);
            b = Math.Min(b, 1.0f);

            return Color.FromArgb((int)alpha, (int)r, (int)g, (int)b);
        }

        public static float HueToRGB(float p, float q, float h) {
            var hue = h;
            if (hue < 0) hue += 1f;
            if (hue > 1) hue -= 1f;

            if (6 * hue < 1) return p + (q - p) * 6f * hue;
            else if (2 * hue < 1) return q;
            else if (3 * hue < 2) return p + (q - p) * 6f * (2.0f / 3.0f - hue);
            else return p;
        }
    }

}







