using System.Runtime.Serialization;
using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
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
