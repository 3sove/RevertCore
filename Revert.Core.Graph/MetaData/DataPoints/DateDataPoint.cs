using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    [DebuggerDisplay("{Key} : {Value}")]
    public class DateDataPoint : DataPoint<string, DateTime>
    {
        public DateDataPoint(string key, DateTime value)
            : base(key, value)
        {
        }

        public DateDataPoint()
        {
        }

        [DataMember]
        public override bool IsSearchable { get; set; } = true;

        [DataMember]
        public override bool IsResolvable { get; set; } = true;
    }
}
