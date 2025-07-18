using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance;
        public float Coin { get; private set; }

        void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); }
            Coin = PlayerPrefs.GetFloat("COIN", 0);
        }

        public void AddCoins(float amount)
        {
            Coin += amount;
            PlayerPrefs.SetFloat("COIN", Coin);
        }
    }
}
