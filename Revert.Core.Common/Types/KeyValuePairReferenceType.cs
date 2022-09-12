namespace Revert.Core.Common.Types
{
    public class KeyValuePairReferenceType<TKey, TValue>
    {
        public TKey Key = default;
        public TValue Value = default;

        public KeyValuePairReferenceType(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is KeyValuePairReferenceType<TKey, TValue> == false) return false;

            var kvp = (KeyValuePairReferenceType<TKey, TValue>)obj;

            if (Equals(Key, kvp.Key) && Equals(Value, kvp.Value)) return true;
            return false;
        }
    }
}
