using UnityEngine;
using System.Collections.Generic;

namespace Vampire
{
    public class CoinGainTextSpawner : MonoBehaviour
    {
        public static CoinGainTextSpawner Instance;

        [SerializeField] CoinGainText prefab;
        [SerializeField] Transform parent;
        [SerializeField] int poolSize = 10;

        Queue<CoinGainText> pool = new();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            for (int i = 0; i < poolSize; i++)
            {
                CoinGainText item = Instantiate(prefab, parent);
                item.gameObject.SetActive(false);
                pool.Enqueue(item);
            }
        }

        public void ShowGain(int amount)
        {
            CoinGainText text = pool.Dequeue();
            text.gameObject.SetActive(true);
            text.Show(amount);
            pool.Enqueue(text);
        }
    }
}
