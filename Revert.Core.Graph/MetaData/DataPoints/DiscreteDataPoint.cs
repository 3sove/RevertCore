using System.Runtime.Serialization;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    public class DiscreteDataPoint : DataPoint<string, long>
    {
        public DiscreteDataPoint()
        {
            
        }

        public DiscreteDataPoint(string key, long value)
            : base(key, value)
        {
        }
    }
}
