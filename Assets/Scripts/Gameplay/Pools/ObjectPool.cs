using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
public class ObjectPool : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] int initialSize = 50;

    Queue<GameObject> pool = new();

    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
            AddToPool();
    }

    GameObject AddToPool()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    public GameObject Get()
    {
        if (pool.Count == 0)
            AddToPool();

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
}
