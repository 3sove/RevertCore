using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.Common.Types.Collections
{
    public abstract class PagedArray
    {
        #region Declarations & Constants
        public const int MinimumBits = 6; // smallest reasonable size 3 by 3
        public const int MaximumBits = 48; // largest reasonable size 24 by 24 or 2 ^ 48, or 256 trillion

        protected const int DefaultEstimatedPercentage = 20; // default percentage to use when making pagedarrays for merging 2 sets

        // To adjust sizes, just modify totalCount.  totalCount must be EVEN!
        protected byte BitCount; // itemCount of the number of bits to use in the index
        protected ulong InnerMask;
        protected ulong OuterMask;
        protected ulong LastPage;
        protected ulong ItemCount;

        public ulong NaturalLimit { get; protected set; }

        public ulong PageSize { get; protected set; }

        public long HighestSet { get; protected set; } = -1;

        public ulong InitialSize { get; protected set; }
        #endregion

        #region Base Static Methods
        public static int EstimateBits(long count, int percent = 10, int minimum = MinimumBits)
        {
            if (count < 1) return MinimumBits;

            if (count >= ((long)1 << MaximumBits))
                throw new Exception("The total number of items are attempting to allocate exceeds " + (Math.Pow(2, MaximumBits) - 1).ToString("#,#") + " - this value is not supported by the PagedArray.");

            count *= 100;
            count /= percent;
            return BitsNeeded(count);
        }

        public static int BitsNeeded(long count)
        {
            // itemCount is how big, at the least, to make the table
            // now figure the number of bits.  Bits must be even, so round up
            var res = 0;
            while (count != 0)
            {
                count >>= 1;
                res++;
            }

            if ((res & 0x01) == 0x01) // is odd
                res++;

            if (res < MinimumBits)
                return MinimumBits;

            if (res > MaximumBits)
                return MaximumBits;

            return res;
        }
        #endregion
    }

    public class PagedArray<T> : PagedArray, IEnumerable<T>
    {
        #region Declarations

        public T[][] PagedArrayMatrix { get; protected set; }

        protected Dictionary<ulong, T> Overflow; // Overflow incase the paged array gets full.
        #endregion

        #region Constructor & Initialization

        public PagedArray(int totalBits = 32)
        {
            if ((totalBits > MaximumBits) || (totalBits < MinimumBits) || ((totalBits & 0x01) != 0))
                throw new Exception($"Bad PagedArray size. Total bits specified were {totalBits}.  Current Minimum is {MinimumBits}, and current Maximum is {MaximumBits}.");

            var totalCount = (byte)totalBits;
            BitCount = (byte)(totalCount / 2);

            PageSize = ((ulong)0x1 << BitCount); //0x100000;
            InnerMask = (PageSize - 1); // 0x00f...f
            OuterMask = ~InnerMask;  // 0xff0...0
            NaturalLimit = ((ulong)0x1 << totalCount); // mask is also the largest index supported
            PagedArrayMatrix = new T[PageSize][];
        }

        public void FillIndex()
        {
            for (ulong i = 0; i < PageSize; i++)
            {
                PagedArrayMatrix[i] = new T[PageSize];
            }
            LastPage = PageSize - 1;
        }

        public PagedArray(ulong initialSize, int totalBits)
            : this(totalBits)
        {
            InitialSize = initialSize;

            var size = (initialSize & OuterMask) >> BitCount;
            if ((initialSize & InnerMask) != 0)
            {
                size++;
            }

            for (ulong i = 0; i <= size; i++)
            {
                if (PagedArrayMatrix[i] == null)
                {
                    PagedArrayMatrix[i] = new T[PageSize];
                }
            }

            if (initialSize >= NaturalLimit)
            {
                var extra = (initialSize - NaturalLimit) + 1;

                if (extra > (int.MaxValue / 2))
                {
                    throw new Exception("PagedArray called with an InitialSize much larger than TotalBits allows.  Consider decreasing the initial size or increasing totalBits.");
                }

                Overflow = new Dictionary<ulong, T>((int)extra);
            }
            LastPage = size;
        }

        public PagedArray(PagedArray<T> source, bool ensureNoOverFlow = false, bool locationImportant = false)
        {
            if (ensureNoOverFlow && (source.Overflow != null) && (source.Overflow.Count != 0))
            {
                PagedArray<T> replacementSource;
                if (locationImportant)
                {
                    ulong max = 0;
                    foreach (var item in source.Overflow)
                    {
                        if (item.Key > max)
                        {
                            max = item.Key;
                        }
                    }
                    replacementSource = new PagedArray<T>(BitsNeeded((long)max));
                    foreach (var item in source.GetIndexedNotNull())
                    {
                        replacementSource.TrySetValue(item.Key, item.Value);
                    }
                }
                else
                {
                    replacementSource = new PagedArray<T>(BitsNeeded((long)(source.NaturalLimit + (ulong)source.Overflow.Count)));
                    foreach (var item in source)
                    {
                        replacementSource.Add(item);
                    }
                }
                source = replacementSource;
            }
            PagedArrayMatrix = source.PagedArrayMatrix;
            BitCount = source.BitCount;
            PageSize = source.PageSize;
            InnerMask = source.InnerMask;
            OuterMask = source.OuterMask;
            HighestSet = source.HighestSet;
            LastPage = source.LastPage;
            NaturalLimit = source.NaturalLimit;
            ItemCount = source.ItemCount;
            Overflow = source.Overflow;
        }

        #endregion

        private void OverflowSet(ulong index, T value)
        {
            if (Overflow == null)
                Overflow = new Dictionary<ulong, T>();
            if ((long)index > HighestSet)
                HighestSet = (long)index;

            Overflow[index] = value;
        }

        private bool OverflowTryGet(ulong index, out T result)
        {
            if (Overflow != null)
                return Overflow.TryGetValue(index, out result);
            result = default;
            return false;
        }

        public ulong Count()
        {
            return ItemCount;
        }

        /// <summary>
        /// This itemCount can be more accurate than the standard Count() method, but it is much slower.
        /// </summary>
        public ulong Count_Accurate()
        {
            ulong count = 0;
            foreach (var bucket in PagedArrayMatrix.Where(item => item != null))
                count += (ulong)bucket.Count(item => item != null);

            if (Overflow != null)
                count += (ulong)Overflow.Count;

            return count;
        }

        public bool IsEmpty()
        {
            if (PagedArrayMatrix.Any(item => item != null)) return false;
            return Overflow == null;
        }

        public void Clear()
        {
            Overflow?.Clear();

            for (uint i = 0; i < LastPage; i++)
                PagedArrayMatrix[i] = null;

            HighestSet = -1;
        }

        public void Add(T value)
        {
            TrySetValue((ulong)HighestSet + 1, value);
        }

        public bool TrySetValue(ulong index, T value)
        {
            SetValue(index, value);
            return true;
        }

        public bool TryGetValue(ulong index, out T result)
        {
            if (index > (ulong)HighestSet)
            {
                result = default;
                return false;
            }
            if (index < NaturalLimit)
            {
                var outerIndex = (index & OuterMask) >> BitCount;

                if (PagedArrayMatrix?[outerIndex] == null)
                {
                    result = default;
                    return false;
                }

                result = PagedArrayMatrix
                    [outerIndex]
                    [(index & InnerMask)];
                return true;
            }
            return OverflowTryGet(index, out result);
        }

        public T TryReturnValue(ulong index)
        {
            if (index < NaturalLimit)
            {
                if (PagedArrayMatrix?[(index & OuterMask) >> BitCount] == null)
                {
                    return default;
                }

                return PagedArrayMatrix
                    [(index & OuterMask) >> BitCount]
                    [(index & InnerMask)];
            }
            T result;
            OverflowTryGet(index, out result);
            return result;
        }

        public bool TryCompareValue(ulong index, T itemToCompare)
        {
            if (index < NaturalLimit)
            {
                if (PagedArrayMatrix?[(index & OuterMask) >> BitCount] == null)
                {
                    return Equals(itemToCompare, default(T));
                }

                return Equals(PagedArrayMatrix
                    [(index & OuterMask) >> BitCount]
                    [(index & InnerMask)], itemToCompare);
            }
            T result;
            OverflowTryGet(index, out result);
            return result.Equals(itemToCompare);
        }

        public bool IsNullOrDefault(ulong index)
        {
            if (index < NaturalLimit)
            {
                if (PagedArrayMatrix?[(index & OuterMask) >> BitCount] == null)
                {
                    return true;
                }
                return Equals(PagedArrayMatrix[(index & OuterMask) >> BitCount][(index & InnerMask)], default(T));
            }
            T result;
            OverflowTryGet(index, out result);
            return Equals(result, default(T));
        }

        public T this[ulong index]
        {
            get
            {
                if (index < NaturalLimit)
                {
                    if (PagedArrayMatrix?[(index & OuterMask) >> BitCount] == null)
                    {
                        return default;
                    }

                    return PagedArrayMatrix
                        [(index & OuterMask) >> BitCount]
                        [(index & InnerMask)];
                }
                T result;
                OverflowTryGet(index, out result);
                return result;
            }
            set
            {
                SetValue(index, value);
            }
        }

        private void SetValue(ulong index, T value)
        {
            if (index < NaturalLimit)
            {
                var page = (index & OuterMask) >> BitCount;
                if ((long)index > HighestSet)
                {
                    HighestSet = (long)index;
                    LastPage = page;
                }

                if (PagedArrayMatrix[page] == null)
                {
                    PagedArrayMatrix[page] = new T[PageSize];
                }

                PagedArrayMatrix[page][(index & InnerMask)] = value;
            }
            else
            {
                OverflowSet(index, value);
            }
            ItemCount++;
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            T[] bucket;
            for (uint i = 0; i < LastPage; i++)
            {
                bucket = PagedArrayMatrix[i];
                if (bucket != null)
                {
                    for (var j = 0; j < (int)PageSize; j++)
                    {
                        var item = bucket[j];
                        if (!Equals(item, default(T)))
                        {
                            yield return item;
                        }
                    }
                }
            }

            bucket = PagedArrayMatrix[LastPage];
            if (bucket != null)
            {
                int lastItem;
                if (HighestSet < (long)NaturalLimit)
                {
                    lastItem = (int)(HighestSet & (long)InnerMask);
                }
                else
                {
                    lastItem = (int)PageSize - 1;
                }
                for (var j = 0; j <= lastItem; j++)
                {
                    var item = bucket[j];
                    if (!Equals(item, default(T)))
                    {
                        yield return item;
                    }
                }
            }

            if (Overflow != null)
            {
                var list = Overflow.OrderBy(n => n.Key);
                foreach (var item in list)
                {
                    yield return item.Value;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            T[] bucket;
            for (uint i = 0; i < LastPage; i++)
            {
                bucket = PagedArrayMatrix[i];
                if (bucket != null)
                {
                    for (var j = 0; j < (int)PageSize; j++)
                    {
                        var item = bucket[j];
                        if (!Equals(item, default(T)))
                        {
                            yield return item;
                        }
                    }
                }
            }

            bucket = PagedArrayMatrix[LastPage];
            if (bucket != null)
            {
                int lastItem;
                if (HighestSet < (long)NaturalLimit)
                {
                    lastItem = (int)(HighestSet & (long)InnerMask);
                }
                else
                {
                    lastItem = (int)PageSize - 1;
                }

                for (var j = 0; j <= lastItem; j++)
                {
                    var item = bucket[j];
                    if (!Equals(item, default(T)))
                    {
                        yield return item;
                    }
                }
            }
            if (Overflow != null)
            {
                var list = Overflow.OrderBy(n => n.Key);
                foreach (var item in list)
                {
                    yield return item.Value;
                }
            }
        }

        public IEnumerator<T> GetEnumeratorWithDefaults()
        {
            T[] bucket;
            for (uint i = 0; i < LastPage; i++)
            {
                bucket = PagedArrayMatrix[i];
                if (bucket != null)
                {
                    for (var j = 0; j <= (int)PageSize; j++)
                    {
                        yield return bucket[j];
                    }
                }
            }

            bucket = PagedArrayMatrix[LastPage];
            if (bucket != null)
            {
                int lastItem;
                if (HighestSet < (long)NaturalLimit)
                {
                    lastItem = (int)(HighestSet & (long)InnerMask);
                }
                else
                {
                    lastItem = (int)PageSize - 1;
                }
                for (var j = 0; j <= lastItem; j++)
                {
                    yield return bucket[j];
                }
            }

            var next = NaturalLimit;
            if (Overflow != null)
            {
                var list = Overflow.OrderBy(n => n.Key);
                foreach (var item in list)
                {
                    for (; next < item.Key; next++)
                    {
                        yield return default;
                    }
                    yield return item.Value;
                    next = item.Key;
                }
            }
        }

        public IEnumerable<System.Collections.Generic.KeyValuePair<ulong, T>> GetIndexedNotNull()
        {
            ulong index = 0;
            T[] bucket;
            for (uint i = 0; i < LastPage; i++)
            {
                bucket = PagedArrayMatrix[i];
                if (bucket != null)
                {
                    for (var j = 0; j <= (int)PageSize; j++)
                    {
                        if (!Equals(bucket[j], default(T)))
                        {
                            yield return new System.Collections.Generic.KeyValuePair<ulong, T>(index, bucket[j]);
                        }
                        index++;
                    }
                }
                else
                {
                    index += PageSize;
                }
            }

            bucket = PagedArrayMatrix[LastPage];
            if (bucket != null)
            {
                var lastItem = HighestSet < (long)NaturalLimit ? (int)HighestSet & (long)InnerMask
                                                                : (int)PageSize - 1;

                for (var j = 0; j <= lastItem; j++)
                {
                    if (!Equals(bucket[j], default(T))) yield return new System.Collections.Generic.KeyValuePair<ulong, T>(index, bucket[j]);
                    index++;
                }
            }

            if (Overflow != null)
            {
                var list = Overflow.OrderBy(n => n.Key);
                foreach (var item in list)
                {
                    yield return item;
                }
            }
        }

        public IEnumerator<T[]> GetBuckets()
        {
            for (uint i = 0; i <= LastPage; i++)
            {
                var bucket = PagedArrayMatrix[i];
                if (bucket != null) yield return bucket;
            }
        }
        #endregion

        #region SetRoutines
        public PagedArray<T> Intersect(PagedArray<T> other)
        {
            var estimatedSize = ItemCount;
            if (ItemCount > other.ItemCount) estimatedSize = other.ItemCount;

            var result = new PagedArray<T>(BitsNeeded((long)estimatedSize));
            result.Intersect(this, other);
            return result;
        }

        private void Intersect(PagedArray<T> left, PagedArray<T> right)
        {
            var keyedArrayList = CreateDistinct(left);

            var rightBuckets = right.GetBuckets();
            while (rightBuckets.MoveNext())
            {
                var rightPage = rightBuckets.Current;
                for (var i = 0; i < (int)right.PageSize; i++)
                {
                    var item = rightPage[i];
                    if (!Equals(item, default(T)))
                    {
                        var index = (((ulong)item.GetHashCode()) % keyedArrayList.NaturalLimit);
                        var atLocal = keyedArrayList.TryReturnValue(index);
                        var found = atLocal?.Find(item);
                        if (found != null)
                        {
                            Add(item);
                            atLocal.Remove(found);
                        }
                    }
                }
            }
        }

        private static PagedArray<LinkedList<T>> CreateDistinct(PagedArray<T> source, ulong sizeHint = 0)
        {
            if (sizeHint == 0) sizeHint = source.ItemCount;

            var destination = new PagedArray<LinkedList<T>>(EstimateBits((long)sizeHint, DefaultEstimatedPercentage));
            destination.FillIndex();
            PopulateDistinctArray(destination, source);
            return destination;
        }

        private static void PopulateDistinctArray(PagedArray<LinkedList<T>> destination, PagedArray<T> source)
        {
            var sourceBuckets = source.GetBuckets();    //source.GetBuckets().GetEnumerator();
            while (sourceBuckets.MoveNext())
            {
                var sourcePage = sourceBuckets.Current;
                for (var i = 0; i < (int)source.PageSize; i++)
                {
                    var item = sourcePage[i];

                    if (!Equals(item, default(T)))
                    {
                        var index = (((ulong)item.GetHashCode()) % destination.NaturalLimit);
                        var atLocal = destination[index];
                        if (atLocal == null)
                        {
                            atLocal = new LinkedList<T>();
                            atLocal.AddFirst(item);
                            destination[index] = atLocal;
                        }
                        else
                        {
                            if (!atLocal.Contains(item))
                            {
                                atLocal.AddFirst(item);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// this is implemented based upon walking the destination location (i, j) and extracting the data from the possible lists at each location of source
        /// </summary>
        private void LoadFromDistinct(PagedArray<LinkedList<T>> source)
        {
            HighestSet = 0;

            var walk = source.GetEnumerator();
            if (walk.MoveNext() == false)
            {
                ItemCount = (ulong)HighestSet;
                return;
            }

            var node = walk.Current.First;
            for (ulong i = 0; i < PageSize; i++)
            {
                var currentPage = new T[PageSize];
                PagedArrayMatrix[i] = currentPage;
                LastPage = i;
                for (ulong j = 0; j < PageSize; j++)
                {
                    if (node == null)
                    {
                        if (walk.MoveNext() == false) // end of list
                        {
                            ItemCount = (ulong)HighestSet;
                            return;
                        }
                        node = walk.Current.First;
                    }
                    HighestSet++;

                    currentPage[j] = node.Value;
                    node = node.Next;
                }
            }

            // exact fit? rare but possible
            if ((node == null) && (walk.MoveNext() == false))
            {
                ItemCount = (ulong)HighestSet;
                return;
            }
            throw new Exception("'loadThis' in LogicalSetPagedArray had a size mismatch");
        }

        public PagedArray<T> Distinct()
        {
            var returnPagedArray = new PagedArray<T>(EstimateBits((long)ItemCount));
            var distinctPagedArray = CreateDistinct(this, Count());
            returnPagedArray.LoadFromDistinct(distinctPagedArray);
            return returnPagedArray;
        }

        public PagedArray<T> Union(PagedArray<T> other, bool unique = false)
        {
            var estimatedSize = ItemCount + other.ItemCount;
            var result = new PagedArray<T>(BitsNeeded((long)estimatedSize));
            if (unique)
            {
                var destination = CreateDistinct(this, Count() + other.Count());
                PopulateDistinctArray(destination, other);
                result.LoadFromDistinct(destination);
            }
            else
            {
                result.Add(this);
                result.Add(other);
            }
            return result;
        }

        private void Add(PagedArray<T> source)
        {
            var sourceBuckets = source.GetBuckets();
            while (sourceBuckets.MoveNext())
            {
                var sourcePage = sourceBuckets.Current;
                for (var i = 0; i < (int)source.PageSize; i++)
                {
                    var item = sourcePage[i];
                    if (!Equals(item, default(T)))
                    {
                        Add(item);
                    }
                }
            }
        }

        public bool Remove(T target)
        {
            var buckets = GetBuckets();
            while (buckets.MoveNext())
            {
                var page = buckets.Current;
                for (var i = 0; i < (int)PageSize; i++)
                {
                    if (Equals(page[i], target))
                    {
                        page[i] = default;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool RemoveAt(ulong position)
        {
            if (position > (ulong)HighestSet) return false;
            this[position] = default;
            ItemCount--;
            return true;
        }

        public PagedArray<T> Exclude(PagedArray<T> other)
        {
            var result = new PagedArray<T>(BitsNeeded((long)ItemCount));
            result.Exclude(this, other);
            return result;
        }

        private void Exclude(PagedArray<T> left, PagedArray<T> right)
        {
            var keyedArrayList = CreateDistinct(left);

            var rightBuckets = right.GetBuckets();
            while (rightBuckets.MoveNext())
            {
                var rightPage = rightBuckets.Current;
                for (var i = 0; i < (int)right.PageSize; i++)
                {
                    var item = rightPage[i];
                    if (!Equals(item, default(T)))
                    {
                        var index = (((ulong)item.GetHashCode()) % keyedArrayList.NaturalLimit);
                        var atLocal = keyedArrayList[index];
                        var where = atLocal?.Find(item);
                        if (@where != null)
                        {
                            atLocal.Remove(@where);
                        }
                    }
                }
            }

            var keyedBuckets = keyedArrayList.GetBuckets();
            while (keyedBuckets.MoveNext())
            {
                var keyPage = keyedBuckets.Current;

                for (var i = 0; i < (int)keyedArrayList.PageSize; i++)
                {
                    var list = keyPage[i];
                    if (list != null) foreach (var item in list) Add(item);
                }
            }
        }
        #endregion
    }
}