using System;
using System.Runtime.Serialization;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
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
