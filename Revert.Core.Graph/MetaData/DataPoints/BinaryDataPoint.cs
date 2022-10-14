using System.Diagnostics;
using System.Runtime.Serialization;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    [DebuggerDisplay("{Key} : {Value}")]
    public class BinaryDataPoint : DataPoint<string, byte[]>
    {
        public BinaryDataPoint(string key, byte[] value)
            : base(key, value)
        {
        }

        public BinaryDataPoint()
        {
        }

        [DataMember]
        public override bool IsSearchable { get; set; } = true;

        [DataMember]
        public override bool IsResolvable { get; set; } = true;
    }
}
