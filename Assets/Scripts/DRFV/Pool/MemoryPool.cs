using System.Collections.Concurrent;

namespace DRFV.Pool
{
    public class MemoryPool<T> where T : class
    {
        private ConcurrentDictionary<string, T> _pool = new();

        public void Push(string id, T value)
        {
            _pool.TryAdd(id, value);
        }
        public T Get(string id)
        {
            return _pool.TryGetValue(id, out T value) ? value : null;
        }

        public void Remove(string id)
        {
            _pool.TryRemove(id, out _);
        }
    
        public void Clear()
        {
            _pool.Clear();
        }
    }
}
