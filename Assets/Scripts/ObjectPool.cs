using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private T prefab;
    private List<T> pool;

    public ObjectPool(T prefab, int initialSize = 10, Transform parent = null)
    {
        this.prefab = prefab;
        pool = new List<T>();

        // Khởi tạo sẵn một số object
        for (int i = 0; i < initialSize; i++)
        {
            T obj = GameObject.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Add(obj);
        }
    }

    public T Get(Transform parent = null, Vector3? position = null, Quaternion? rotation = null)
    {
        T obj = pool.Find(o => !o.gameObject.activeInHierarchy);

        if (obj == null)
        {
            // Nếu không còn object rảnh, tạo mới
            obj = GameObject.Instantiate(prefab);
            pool.Add(obj);
        }

        obj.gameObject.SetActive(true);

        if (parent != null)
        {
            obj.transform.SetParent(parent, worldPositionStays: false);
        }

        if (position != null)
        {
            obj.transform.localPosition = position.Value;
        }

        if (rotation != null)
        {
            obj.transform.localRotation = rotation.Value;
        }

        obj.transform.localScale = Vector3.one;
        return obj;
    }

    public void Release(T obj)
    {
        obj.gameObject.SetActive(false);
    }
}
