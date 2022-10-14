using System.Runtime.Serialization;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    public class ContinuousDataPoint : DataPoint<string, double>
    {

        public ContinuousDataPoint() : base()
        {
        }

        public ContinuousDataPoint(string key, double value)
            : base(key, value)
        {
        }
    }
}
