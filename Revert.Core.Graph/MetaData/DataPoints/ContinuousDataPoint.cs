using System.Runtime.Serialization;
using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
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
