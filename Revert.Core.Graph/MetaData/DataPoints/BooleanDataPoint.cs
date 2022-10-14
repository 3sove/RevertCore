using System.Diagnostics;
using System.Runtime.Serialization;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    [DebuggerDisplay("{Key} : {Value}")]
    public class BooleanDataPoint : DataPoint<string, bool>
    {
        public BooleanDataPoint(string key, bool value)
            : base(key, value)
        {
        }

        public BooleanDataPoint()
        {
        }

        [DataMember]
        public override bool IsSearchable { get; set; } = true;

        [DataMember]
        public override bool IsResolvable { get; set; } = true;
    }
}
