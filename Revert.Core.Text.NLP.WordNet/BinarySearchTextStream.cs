using System;
using System.IO;

namespace Revert.Core.Text.NLP.WordNet
{
    public class BinarySearchTextStream : SearchTextStream
    {
        private SearchComparisonDelegate _searchComparison;

        public BinarySearchTextStream(Stream stream, SearchComparisonDelegate searchComparison)
          : base(stream)
        {
            _searchComparison = searchComparison;
        }

        public BinarySearchTextStream(string path, SearchComparisonDelegate searchComparison)
          : this(new FileStream(path, FileMode.Open, FileAccess.Read), searchComparison)
        {
        }

        public override string Search(object key, long start, long end)
        {
            CheckSearchRange(start, end);
            while (start <= end)
            {
                Stream.BaseStream.Position = (long)((start + end) / 2.0);
                int num1 = 0;
                for (; Stream.BaseStream.Position > 0L; Stream.BaseStream.Position -= 2L)
                {
                    int num2;
                    if ((num2 = Stream.BaseStream.ReadByte()) == -1)
                        throw new Exception("Failed to read byte");
                    char ch = (char)num2;
                    if (++num1 > 1 && ch == '\n')
                        break;
                }
                long position1 = Stream.BaseStream.Position;
                uint position2 = (uint)position1;
                if (position2 != position1)
                    throw new Exception("uint overflow");
                Stream.DiscardBufferedData();
                string currentLine = ReadLine(Stream, ref position2);
                --position2;
                int num3 = _searchComparison(key, currentLine);
                if (num3 == 0)
                    return currentLine;
                if (num3 < 0)
                    end = position1 - 1L;
                else if (num3 > 0)
                    start = position2 + 1U;
            }
            return null;
        }

        public delegate int SearchComparisonDelegate(object key, string currentLine);

        public static string ReadLine(StreamReader reader, ref uint position)
        {
            string str = reader.ReadLine();
            if (str == null)
                return str;
            uint num = position;
            position += (uint)reader.CurrentEncoding.GetByteCount(str + Environment.NewLine);
            if (position < num)
                throw new Exception("Reader position wrapped around");
            return str;
        }
    }
}
