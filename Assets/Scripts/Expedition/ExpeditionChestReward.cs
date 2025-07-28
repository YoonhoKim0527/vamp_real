using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class ExpeditionChestReward : MonoBehaviour
    {
        int goldReward;
        int emeraldReward;
        GameObject openEffectPrefab;
        bool opened = false;

        public void Init(int gold, int emerald, GameObject effect = null)
        {
            goldReward = gold;
            emeraldReward = emerald;
            openEffectPrefab = effect;
        }

        void OnMouseDown()
        {
            if (opened) return;
            opened = true;

            CoinManager.Instance?.AddCoins(goldReward);
            // CurrencyManager.Instance?.AddEmerald(emeraldReward);

            if (openEffectPrefab != null)
                Instantiate(openEffectPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
