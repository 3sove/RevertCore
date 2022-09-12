namespace Revert.Core.MachineLearning.Data
{
    public class MnistImage
    {
        public byte[] pixels;
        public byte label;
        public int rows;
        public int columns;

        public MnistImage(byte[] pixels, int rows, int columns, byte label)
        {
            this.pixels = pixels;
            this.label = label;
            this.rows = rows;
            this.columns = columns;
        }

        public override string ToString()
        {
            string output = string.Empty;
            for (int i = 0; i < 28; ++i)
            {
                for (int j = 0; j < 28; ++j)
                {
                    var pixel = pixels[i * 28 + j];
                    if (pixel == 0)
                        output += " "; //white
                    else if (pixel <= 85)
                        output += "."; //light grey
                    else if (pixel <= 170)
                        output += ":"; //dark grey
                    else
                        output += "#"; //black
                }
                output += System.Environment.NewLine;
            }
            return output + label.ToString();
        }
    }
}

