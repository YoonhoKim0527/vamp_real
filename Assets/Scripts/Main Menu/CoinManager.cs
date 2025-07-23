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
            SetCoins(coins + amount);
            if (amount > 0)
            CoinGainTextSpawner.Instance?.ShowGain(amount);
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
