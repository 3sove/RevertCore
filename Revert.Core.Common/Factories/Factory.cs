using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Common.Factories
{
    public abstract class Factory<T, TSelf> where TSelf : Factory<T, TSelf>, new()
    {
        private static TSelf instance = default(TSelf);
        public static TSelf Instance
        {
            get
            {
                if (Object.Equals(instance, default(T)))
                {
                    instance = new TSelf();
                }
                return instance;
            }
        }


        private Queue<T> stack = new Queue<T>();
        protected virtual int initialCapacity { get; } = 5000;

        public Factory()
        {
            for (int i = 0; i < initialCapacity; i++)
            {
                var newItem = getNewItem();
                stack.Enqueue(newItem);
            }
        }

        protected abstract T getNewItem();

        public T get()
        {
            if (stack.Count == 0)
            {
                return getNewItem();
            }
            return stack.Dequeue();
        }

        public T peek()
        {
            if (stack.Count == 0)
            {
                stack.Enqueue(getNewItem());
            }
            return stack.Peek();
        }

        public virtual void dispose(T item)
        {
            stack.Enqueue(item);
        }

        public virtual void dispose(ICollection<T> items)
        {
            foreach (var item in items) stack.Enqueue(item);
        }
    }
}
