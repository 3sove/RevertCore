using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    public class ContinuousTimeSeriesDataPoint : TimeSeriesDataPoint<double>
    {
        public ContinuousTimeSeriesDataPoint()
        {
        }

        public ContinuousTimeSeriesDataPoint(string key, Tuple<DateTime, double> value) : base(key, value)
        {
        }
    }
}
