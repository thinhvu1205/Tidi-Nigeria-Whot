using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Common.Pool
{
    public class PoolManager<T> where T : MonoBehaviour, IPoolable
    {
        private ObjectPool<T> pool;
        private T prefab;

        public PoolManager(T prefab, Transform parent = null, int defaultCapacity = 15, int maxSize = 100)
        {
            this.prefab = prefab;

            pool = new ObjectPool<T>(
                () => Object.Instantiate(prefab, parent),
                actionOnGet: (item) =>
                {
                    item.gameObject.SetActive(true);
                    item.OnGetFromPool();
                },
                actionOnRelease: (item) =>
                {
                    item.OnReturnToPool();
                    item.gameObject.SetActive(false);
                    item.transform.SetParent(parent);
                },
                actionOnDestroy: (item) =>
                {
                    Object.Destroy(item.gameObject);
                },
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        public T Get() => pool.Get();
        
        public void Release(T item) => pool.Release(item);
        
        public void Prewarm(int count)
        {
            List<T> tempList = new();

            for (int i = 0; i < count; i++)
            {
                var obj = pool.Get();
                tempList.Add(obj); 
            }

            foreach (var obj in tempList)
            {
                pool.Release(obj);
            }
        }
        
        /// <summary>
        /// Clear all objects in the pool
        /// </summary>
        public void Clear()
        {
            pool.Clear();
        }
    }
}