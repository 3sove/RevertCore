using System;

namespace Revert.Core.Common
{
    public interface IKeyGenerator<T>
    {
        T GetCurrent();
        T GetNext();
    }

    public class KeyGenerator<T> : IKeyGenerator<T>
    {
        private readonly Func<T, T> getNext;

        public KeyGenerator(T startingId, Func<T, T> getNext)
        {
            this.current = startingId;
            this.getNext = getNext;
        }

        private T current;
        public T GetCurrent()
        {
            return current;
        }

        public T GetNext()
        {
            current = getNext(current);
            return current;
        }
    }
   

}
