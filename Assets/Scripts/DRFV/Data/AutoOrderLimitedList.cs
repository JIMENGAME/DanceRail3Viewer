using System;
using System.Collections.Generic;

namespace DRFV.Data
{
    public class AutoOrderLimitedList<T> : List<T> where T : IComparable
    {
        public new void Add(T item)
        {
            base.Add(item);
            Sort((a ,b) => a.CompareTo(b));
        }

        public AutoOrderLimitedList(int capacity) : base(capacity)
        {
        }
    }
}