using UnityEngine.Pool;
using UnityEngine;

namespace Vampire
{
    public class PoisonCloudPool : Pool
    {
        protected ObjectPool<PoisonCloud> pool;

        public override void Init(EntityManager entityManager, Character playerCharacter, GameObject prefab, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 1000)
        {
            base.Init(entityManager, playerCharacter, prefab, collectionCheck, defaultCapacity, maxSize);
            pool = new ObjectPool<PoisonCloud>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPooledItem, collectionCheck, defaultCapacity, maxSize);
        }

        public PoisonCloud Get()
        {
            return pool.Get();
        }

        public void Release(PoisonCloud cloud)
        {
            pool.Release(cloud);
        }

        protected PoisonCloud CreatePooledItem()
        {
            PoisonCloud cloud = Instantiate(prefab, transform).GetComponent<PoisonCloud>();
            return cloud;
        }

        protected void OnTakeFromPool(PoisonCloud cloud)
        {
            cloud.gameObject.SetActive(true);
        }

        protected void OnReturnedToPool(PoisonCloud cloud)
        {
            cloud.gameObject.SetActive(false);
        }

        protected void OnDestroyPooledItem(PoisonCloud cloud)
        {
            Destroy(cloud.gameObject);
        }
    }
}
