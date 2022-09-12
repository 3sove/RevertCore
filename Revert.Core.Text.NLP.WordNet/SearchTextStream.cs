using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revert.Core.Text.NLP.WordNet
{

    public abstract class SearchTextStream
    {
        private StreamReader _stream;

        public StreamReader Stream
        {
            get
            {
                return _stream;
            }
        }

        protected SearchTextStream(Stream stream)
        {
            _stream = new StreamReader(stream);
        }

        public string Search(object key)
        {
            if (_stream.BaseStream.Length == 0L)
                return null;
            return Search(key, 0L, _stream.BaseStream.Length - 1L);
        }

        public abstract string Search(object key, long start, long end);

        protected void CheckSearchRange(long start, long end)
        {
            if (start < 0L)
                throw new ArgumentOutOfRangeException(nameof(start), "Start byte position must be non-negative");
            if (end >= Stream.BaseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End byte position must be less than the length of the stream");
            if (start > end)
                throw new ArgumentOutOfRangeException(nameof(start), "Start byte position must be less than or equal to end byte position");
        }

        public virtual void Close()
        {
            if (_stream == null)
                return;
            _stream.Close();
            _stream = null;
        }

        public virtual void ReInitialize(Stream stream)
        {
            Close();
            _stream = new StreamReader(stream);
        }
    }
}
