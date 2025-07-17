using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 50;

        private Queue<GameObject> pool = new();

        void Awake()
        {
            for (int i = 0; i < initialSize; i++)
                AddToPool();
        }

        private GameObject AddToPool()
        {
            GameObject obj;

            // ✅ 부모(transform)가 씬에 유효한지 확인
            if (transform != null && transform.gameObject.scene.IsValid())
            {
                obj = Instantiate(prefab, transform); // 부모가 유효하면 부모 지정
            }
            else
            {
                Debug.LogWarning("[ObjectPool] 부모 transform이 유효하지 않아 부모 없이 생성");
                obj = Instantiate(prefab); // 부모가 없으면 그냥 생성
            }

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
