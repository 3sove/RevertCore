using System;
using System.Runtime.Serialization;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
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
