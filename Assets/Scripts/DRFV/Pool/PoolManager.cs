using DRFV.inokana;
using DRFV.Setting;
using UnityEngine;

namespace DRFV.Pool
{
    public class PoolManager : MonoSingleton<PoolManager>
    {
        public bool usePool;
        public MemoryPool<Sprite> spritePool = new();
        public MemoryPool<AudioClip> audioClipPool = new();

        public void Push<T>(MemoryPool<T> pool, string id, T value) where T : class
        {
            if (!usePool) return;
            pool.Push(id, value);
        }

        public T Get<T>(MemoryPool<T> pool, string id) where T : class
        {
            return usePool ? pool.Get(id) : null;
        }
        public void Remove<T>(MemoryPool<T> pool, string id) where T : class
        {
            if (!usePool) return;
            pool.Remove(id);
        }
        
        public void Clear<T>(MemoryPool<T> pool) where T : class
        {
            if (!usePool) return;
            pool.Clear();
        }

        public void RefreshSetting()
        {
            usePool = GlobalSettings.CurrentSettings.UseMemoryCache;
            if (usePool) return;
            spritePool.Clear();
            audioClipPool.Clear();
        }
    }
}
