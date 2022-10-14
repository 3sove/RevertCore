using System;
using System.Runtime.Serialization;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract]
    public abstract class TimeSeriesDataPoint<T> : DataPoint<string, Tuple<DateTime, T>>
    {
        protected TimeSeriesDataPoint()
        {
        }

        protected TimeSeriesDataPoint(string key, Tuple<DateTime, T> value) : base(key, value)
        {
        }
    }
}
