using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Images
{
    public class ImageModel
    {
        public byte[] bytes { get; set; } = null;

        public int[][] pixelMap { get; set; } = null;

        public int width { get; set; }

        public int height { get; set; }

        public bool hasAlpha { get; set; } = false;

    }
}
