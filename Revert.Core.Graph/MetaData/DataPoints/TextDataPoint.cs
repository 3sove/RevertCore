using System.Diagnostics;
using System.Runtime.Serialization;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    [DebuggerDisplay("{Key} : {Value}")]
    public class TextDataPoint : DataPoint<string, string>
    {
        public TextDataPoint(string key, string value)
            : base(key, value)
        {
        }

        public TextDataPoint()
        {
        }

        [DataMember]
        public override bool IsSearchable { get; set; } = true;
        
    }
}
