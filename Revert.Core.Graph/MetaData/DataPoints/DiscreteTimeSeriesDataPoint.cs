using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    public class DiscreteTimeSeriesDataPoint : TimeSeriesDataPoint<long>
    {
        public DiscreteTimeSeriesDataPoint()
        {
        }

        public DiscreteTimeSeriesDataPoint(string key, Tuple<DateTime, long> value)
            : base(key, value)
        {
        }
    }
}
