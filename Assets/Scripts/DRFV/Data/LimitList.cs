using System.Collections.Generic;

namespace DRFV.Data
{
    public class LimitList<T> : List<T>
    {
        private readonly int _capacity;
        public LimitList(int capacity)
        {
            _capacity = capacity;
        }
        public new void Add(T item)
        {
            base.Add(item);
            if (Count > _capacity) RemoveAt(0);
        }
    }
}