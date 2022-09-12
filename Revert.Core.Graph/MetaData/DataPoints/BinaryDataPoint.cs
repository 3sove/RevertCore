using System.Diagnostics;
using System.Runtime.Serialization;
using Revert.Core.Common;
using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    [DebuggerDisplay("{Key} : {Value}")]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    public class BinaryDataPoint : DataPoint<string, byte[]>
    {
        public BinaryDataPoint(string key, byte[] value)
            : base(key, value)
        {
        }

        public BinaryDataPoint()
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
