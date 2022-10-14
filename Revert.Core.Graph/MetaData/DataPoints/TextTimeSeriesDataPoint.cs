using System;
using System.Runtime.Serialization;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
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
