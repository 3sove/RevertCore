using System.Diagnostics;
using System.Runtime.Serialization;
using Revert.Core.Common;
using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    [DebuggerDisplay("{Key} : {Value}")]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    public class TextDataPoint : DataPoint<string, string>
    {
        public TextDataPoint(string key, string value)
            : base(key, value)
        {
        }

        public TextDataPoint()
        {
        }

        [DataMember]
        //[ProtoMember((int)ProtobufIds.MemberDataPointSearchable)]
        public override bool IsSearchable { get; set; } = true;
        
    }
}
