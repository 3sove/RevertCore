using System;
using System.Collections.Generic;

namespace Revert.Core.GuitarProReader
{
    public class Measure
    {
        //TODO: Metadata of measure

        //public int StringsNumber { get; set; } //TODO: ambiguous usage

        public Byte NumeratorSignature { get; set; }
        public Byte DenominatorSignature { get; set; }

        private List<Beat> beats;
        public List<Beat> Beats
        {
            get { return beats ?? (beats = new List<Beat>()); }
            set { beats = value; }
        }
    }
}
