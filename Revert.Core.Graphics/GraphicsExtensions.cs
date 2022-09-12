using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Revert.Core.Graphics
{
    public static class GraphicsExtensions
    {
        public static float toFloatBits(this Color color)
        {
            int colorNumber = ((int)(255 * color.A) << 24) | ((int)(255 * color.B) << 16) | ((int)(255 * color.G) << 8) | ((int)(255 * color.R));
            return intToFloatColor(colorNumber);
        }

        public static float intToFloatColor(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static Color set(this Color color, Color newColor)
        {
            color.R = newColor.R;
            color.G = newColor.G;
            color.B = newColor.B;
            color.A = newColor.A;
            return color;
        }

        public static Color set(this Color color, byte R, byte G, byte B, byte A)
        {
            color.R = R;
            color.G = G;
            color.B = B;
            color.A = A;
            return color;
        }

        public static Color set(this Color color, float R, float G, float B, float A)
        {
            color.R = (byte)R;
            color.G = (byte)G;
            color.B = (byte)B;
            color.A = (byte)A;
            return color;
        }

        public static int floatToIntColor(float value)
        {
            byte[] intBits = BitConverter.GetBytes(value);
            return BitConverter.ToInt32(intBits, 0);
        }
        


    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FloatToIntConverter
    {
        [FieldOffset(0)]
        public int IntValue;
        [FieldOffset(0)]
        public float FloatValue;
    }
}
