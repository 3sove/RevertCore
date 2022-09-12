using System.Runtime.Serialization;
using Revert.Core.Common;
using Revert.Core.Extensions;
using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    [ProtoInclude((int)ProtobufIds.IncludeContinuousDataPoint, typeof(ContinuousDataPoint))]
    [ProtoInclude((int)ProtobufIds.IncludeDiscreteDataPoint, typeof(DiscreteDataPoint))]
    [ProtoInclude((int)ProtobufIds.IncludeTextDataPoint, typeof(TextDataPoint))]
    [ProtoInclude((int)ProtobufIds.IncludeBooleanDataPoint, typeof(BooleanDataPoint))]
    [ProtoInclude((int)ProtobufIds.IncludeDateDataPoint, typeof(DateDataPoint))]
    [ProtoInclude((int)ProtobufIds.IncludeContinuousTimeSeriesDataPoint, typeof(ContinuousTimeSeriesDataPoint))]
    [ProtoInclude((int)ProtobufIds.IncludeDiscreteTimeSeriesDataPoint, typeof(DiscreteTimeSeriesDataPoint))]
    public abstract class DataPoint<TKey, TValue> : IDataPoint<TKey, TValue>
    {
        protected DataPoint()
        {
        }

        protected DataPoint(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        [ProtoIgnore]
        public string Summary => $"{Key.ToString().Truncate(100, true, true, true)} - {Value.ToString().Truncate(800, true, true, true)}";

        [DataMember]
        //[ProtoMember((int)ProtobufIds.MemberDataPointKey)]
        public TKey Key { get; set; }

        [DataMember]
        //[ProtoMember((int)ProtobufIds.MemberDataPointValue)]
        public TValue Value { get; set; }

        [DataMember]
        //[ProtoMember((int)ProtobufIds.MemberDataPointSearchable)]
        public virtual bool IsSearchable { get; set; } = true;

        [DataMember]
        //[ProtoMember((int)ProtobufIds.MemberDataPointResolvable)]
        public virtual bool IsResolvable { get; set; }

        [ProtoIgnore]
        public object key
        {
            get
            {
                return Key;
            }
            set
            {
                Key = (TKey)value;
            }
        }

        [ProtoIgnore]
        public object value
        {
            get
            {
                return Value;
            }
            set
            {
                Value = (TValue)value;
            }
        }

        public override int GetHashCode()
        {
            return Key?.GetHashCode() ?? 0 ^ Value?.GetHashCode() ?? 0;
        }

        public override bool Equals(object obj)
        {
            var objDataPoint = obj as DataPoint<TKey, TValue>;
            if (objDataPoint == null) return false;
            return Equals(Key, objDataPoint.Key) && Equals(Value, objDataPoint.Value);
        }
    }
}
