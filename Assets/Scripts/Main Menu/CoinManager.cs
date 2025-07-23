using UnityEngine;
using System;

namespace Vampire
{
    public class CoinManager : MonoBehaviour
    {
        public static CoinManager Instance { get; private set; }

        public event Action<int> OnCoinsChanged; // ✅ 이벤트

        private int coins;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            coins = PlayerPrefs.GetInt("Coins", 0);
        }

        public int GetCoins()
        {
            return coins;
        }

        public void SetCoins(int amount)
        {
            coins = amount;
            PlayerPrefs.SetInt("Coins", coins);
            PlayerPrefs.Save();

            OnCoinsChanged?.Invoke(coins); // ✅ 구독자에 알림
        }

        public void AddCoins(int amount)
        {
            float multiplier = BoostManager.Instance != null ? BoostManager.Instance.GetMultiplier(BoostType.Coin) : 1f;
            int finalAmount = Mathf.RoundToInt(amount * multiplier);

            SetCoins(coins + finalAmount);

            if (finalAmount > 0)
                CoinGainTextSpawner.Instance?.ShowGain(finalAmount); // 💰 최종 획득량 표시
        }

        public bool SpendCoins(int amount)
        {
            if (coins >= amount)
            {
                SetCoins(coins - amount);
                return true;
            }
            return false;
        }
    }
}
