using UnityEngine;

namespace Vampire
{
    [System.Serializable]
    public class LootGameObject
    {
        public GameObject item;
        [Range(0f, 1f)] public float dropChance;

        // ✅ 추가 (Coin 사용 시 필요)
        public CoinType coinType;
    }
}