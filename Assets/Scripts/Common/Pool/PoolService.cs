using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Common.Pool
{
    public class PoolService : MonoBehaviour
    {
        [System.Serializable]
        public class PoolEntry
        {
            public Globals.PrefabType type;
            public MonoBehaviour prefab; // cast sang IPoolable bên trong
            public int defaultCapacity = 15;
            public int maxSize = 100;
            public int prewarmCount = 0;
        }

        public List<PoolEntry> prefabs;
        private Dictionary<Globals.PrefabType, object> poolMap = new();
        private static PoolService _instance;
        public static PoolService Instance => _instance;
        private void Awake()
        {
          
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            // tạo sẵn pool từ đầu nếu các game có prefab chung cần thêm vào pool
            foreach (var entry in prefabs)
            {
                var prefab = entry.prefab as IPoolable;
                var type = entry.type;

                var genericType = typeof(PoolManager<>).MakeGenericType(entry.prefab.GetType());
                var manager = System.Activator.CreateInstance(genericType, entry.prefab, transform, entry.defaultCapacity,
                    entry.maxSize);

                poolMap[type] = manager;
                var method = genericType.GetMethod("Prewarm");
                method?.Invoke(manager, new object[] { entry.prewarmCount });
            }
        }
        
        // chỉ tạo pool theo prefab của game khi vào chơi
        public void Register<T>(Globals.PrefabType type, Transform parent , T prefab, int defaultCapacity, int maxSize, int prewarmCount)
            where T : MonoBehaviour, IPoolable
        {
            if (poolMap.ContainsKey(type)) return; // đã có rồi thì bỏ qua

            var manager = new PoolManager<T>(prefab, parent, defaultCapacity, maxSize);
            poolMap[type] = manager;
            manager.Prewarm(prewarmCount);
        }

        public T Get<T>(Globals.PrefabType type) where T : MonoBehaviour, IPoolable
        {
            if (!poolMap.TryGetValue(type, out var managerObj))
            {
                Debug.LogError($"No pool found for type {type}");
                return null;
            }

            return (managerObj as PoolManager<T>)?.Get();
        }

        public void Release<T>(Globals.PrefabType type, T item) where T : MonoBehaviour, IPoolable
        {
            var manager = poolMap[type] as PoolManager<T>;
            manager?.Release(item);
        }
    }
}