using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Revert.Core.Common;
using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    [DebuggerDisplay("{Key} : {Value}")]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    public class DateDataPoint : DataPoint<string, DateTime>
    {
        public DateDataPoint(string key, DateTime value)
            : base(key, value)
        {
        }

        public DateDataPoint()
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
