using System.Diagnostics;
using System.Runtime.Serialization;
using Revert.Core.Common;
using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    [DebuggerDisplay("{Key} : {Value}")]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    public class BooleanDataPoint : DataPoint<string, bool>
    {
        public BooleanDataPoint(string key, bool value)
            : base(key, value)
        {
        }

        public BooleanDataPoint()
        {
        }

        [DataMember]
        //[ProtoMember((int)ProtobufIds.MemberDataPointSearchable)]
        public override bool IsSearchable { get; set; } = true;

        [DataMember]
        //[ProtoMember((int)ProtobufIds.MemberDataPointResolvable)]
        public override bool IsResolvable { get; set; } = true;
    }
}
