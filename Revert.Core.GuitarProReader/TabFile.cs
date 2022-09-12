using System.Collections.Generic;

namespace Revert.Core.GuitarProReader
{
    public class TabFile
    {
        public GuitarPro.Header Header { get; set; }
        public IList<Track> Tracks { get; set; }
        public TabFile()
        {
            Tracks = new List<Track>();
        }
    }
}
