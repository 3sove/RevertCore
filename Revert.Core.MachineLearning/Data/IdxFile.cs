using Revert.Core.IO;
using Revert.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Revert.Core.MachineLearning.Data
{
    public class IdxMagicNumber
    {
        public byte zeroPadding1;
        public byte zeroPadding2;
        public byte dataType;
        public byte dimensionCount;
        public IdxMagicNumber(int magicNumber)
        {
            zeroPadding1 = (byte)(magicNumber << 24);
            zeroPadding2 = (byte)(magicNumber << 16);
            dataType = (byte)(magicNumber << 8);
            //0x08: unsigned byte
            //0x09: signed byte
            //0x0B: short (2 bytes)
            //0x0C: int (4 bytes)
            //0x0D: float (4 bytes)
            //0x0E: double (8 bytes)
            dimensionCount = (byte)magicNumber;
        }
    }

    public class IdxFile
    {
        public string ImageFilePath { get; }
        public string LabelFilePath { get; }
        private List<MnistImage> images = null;
        public List<MnistImage> Images
        {
            get
            {
                if (images == null) images = Load();
                return images;
            }
        }

        public IdxFile(string imageFilePath, string labelFilePath)
        {
            ImageFilePath = imageFilePath;
            LabelFilePath = labelFilePath;
        }

        private List<MnistImage> Load()
        {
            var images = new List<MnistImage>();
            FileStream ifsImages = null;
            FileStream ifsLabels = null;
            BigEndianBinaryReader brImages = null;
            BigEndianBinaryReader brLabels = null;
            try
            {
                ifsImages = new FileStream(ImageFilePath, FileMode.Open);
                ifsLabels = new FileStream(LabelFilePath, FileMode.Open);

                brImages = new BigEndianBinaryReader(ifsImages);
                brLabels = new BigEndianBinaryReader(ifsLabels);

                int magicIntImages = brImages.ReadInt32();
                var magicNumberImages = new IdxMagicNumber(magicIntImages);

                int numImages = brImages.ReadInt32();
                int numRows = brImages.ReadInt32();
                int numCols = brImages.ReadInt32();

                int magicIntLabels = brLabels.ReadInt32();
                var magicNumberLabels = new IdxMagicNumber(magicIntLabels);
                int numLabels = brLabels.ReadInt32();

                for (int di = 0; di < numImages; ++di)
                {
                    byte[] pixels = new byte[numRows * numCols];
                    for (int i = 0; i < pixels.Length; ++i)
                        pixels[i] = brImages.ReadByte();

                    byte lbl = brLabels.ReadByte();

                    MnistImage image = new MnistImage(pixels, numRows, numCols, lbl);
                    images.Add(image);
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (brImages != null)
                {
                    brImages.Close();
                    ifsImages = null;
                }
                if (ifsImages != null)
                {
                    ifsImages.Close();
                }

                if (brLabels != null)
                {
                    brLabels.Close();
                    ifsLabels = null;
                }
                if (ifsLabels != null)
                {
                    ifsLabels.Close();
                }
            }
            return images;
        }
    }
}

