using System.Collections.Generic;

namespace Revert.Core.IO.Files
{
    public class Images
    {
        private static HashSet<string> imageFileExtensions;
        public static HashSet<string> ImageFileExtensions
        {
            get { return imageFileExtensions ?? (imageFileExtensions = GetImageFileExtensions()); }
            set { imageFileExtensions = value; }
        }

        public static HashSet<string> GetImageFileExtensions()
        {
            return new HashSet<string>(new[]{
                "aai", "art", "arw", "avi", "avs", "bmp", "bmp2", "bmp3", "cals", "cgm", "cin", "cmyk", "cmyka", "cr2",
                "crw", "cur", "cut", "dcm", "dcr", "dcx", "dib", "djvu", "dng", "dot", "dpx", "emf", "epi",
                "eps", "eps2", "eps3", "epsf", "epsi", "ept", "exr", "fax", "fig", "fits", "fpx", "gif", "gplt",
                "gray", "hdr", "hpgl", "hrz", "ico", "jbig", "jng", "jp2", "jpc", "jpeg",
                "jxr", "man", "mat", "miff", "mono", "mng", "m2v", "mpeg", "mpc", "mpr", "mrw", "msl", "mtv", "mvg",
                "nef", "orf", "otb", "p7", "palm", "pam", "clipboard", "pbm", "pcd", "pcds", "pcl", "pcx",
                "pef", "pfa", "pfb", "pfm", "pgm", "picon", "pict", "pix", "png", "png8", "png00", "png24",
                "png32", "png48", "png64", "pnm", "ppm", "ps", "ps2", "ps3", "psb", "psd", "ptif", "pwp", "rad",
                "raf", "rgb", "rgba", "rla", "rle", "sct", "sfw", "sgi", "shtml", "sid", "mrsid", "sparse-color",
                "sun", "svg", "tga", "tiff", "tim", "ttf", "uil", "uyvy", "vicar", "viff", "wbmp", "wdp",
                "webp", "wmf", "wpg", "x", "xbm", "xcf", "xpm", "xwd", "x3f", "ycbcr", "ycbcra", "yuv"
            });
        }
    }
}
