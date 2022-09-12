using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
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
