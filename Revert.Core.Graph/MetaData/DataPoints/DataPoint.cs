using System.Runtime.Serialization;
using Revert.Core.Extensions;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    [DataContract(IsReference = true)]
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

        public string Summary => $"{Key.ToString().Truncate(100, true, true, true)} - {Value.ToString().Truncate(800, true, true, true)}";

        [DataMember]
        public TKey Key { get; set; }

        [DataMember]
        public TValue Value { get; set; }

        [DataMember]
        public virtual bool IsSearchable { get; set; } = true;

        [DataMember]
        public virtual bool IsResolvable { get; set; }

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
