using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    public class TextTimeSeriesDataPoint : TimeSeriesDataPoint<string>
    {
        public TextTimeSeriesDataPoint()
        {
        }

        public TextTimeSeriesDataPoint(string key, Tuple<DateTime, string> value)
            : base(key, value)
        {
        }
    }
}
