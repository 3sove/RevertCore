using Revert.Core.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Revert.Core.Common.Types.Tries
{
    [DebuggerDisplay("{DebugString}")]
    public class KeyAggregate<TKey> : List<System.Collections.Generic.KeyValuePair<TKey, int>>
    {
        public KeyAggregate() : base()
        {
        }

        public KeyAggregate(IEnumerable<System.Collections.Generic.KeyValuePair<TKey, int>> items) : base(items)
        {
        }
     
        public string DebugString
        {
            get { return this.Select(item => $"{item.Key}|{item.Value}").Combine(", "); }
        }
    }

    [DebuggerDisplay("{DebugString}")]
    public class KeyAggregate<TKey, TValue> : List<KeyTripplet<TKey, List<TValue>, int>>
    {
        public KeyAggregate() : base()
        {
        }

        public KeyAggregate(IEnumerable<KeyTripplet<TKey, List<TValue>, int>> items) : base(items)
        {
        }

        public string DebugString
        {
            //{item.KeyTwo.DistinctWithCount().Select(value => $"{value.KeyOne} ({value.KeyTwo})").Combine("-")}
            get { return this.Select(item => $"{item.KeyOne}|{item.KeyThree}").Combine(", "); }
        }
    }
}
