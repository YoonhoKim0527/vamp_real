using UnityEngine;
using System;

namespace Vampire
{
    public class RubyManager : MonoBehaviour
    {
        public static RubyManager Instance { get; private set; }

        public event Action<int> OnRubiesChanged;

        private int rubies;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            rubies = PlayerPrefs.GetInt("Rubies", 0);
            Debug.Log($"rubies: {rubies}");
        }

        public int GetRubies()
        {
            return rubies;
        }

        public void SetRubies(int amount)
        {
            rubies = amount;
            PlayerPrefs.SetInt("Rubies", rubies);
            PlayerPrefs.Save();

            Debug.Log($"[RubyManager] 루비 갱신: {rubies}");
            OnRubiesChanged?.Invoke(rubies);
        }

        public void AddRubies(int amount)
        {
            SetRubies(rubies + amount);

            if (amount > 0)
                Debug.Log($"[RubyManager] 루비 +{amount} (총 {rubies})");
        }
        public void Add100Rubies()
        {
            AddRubies(100);
        }

        public bool SpendRubies(int amount)
        {
            if (rubies >= amount)
            {
                SetRubies(rubies - amount);
                return true;
            }
            return false;
        }

        public void ShowNotEnoughRubyPopup()
        {
            var popup = FindObjectOfType<NotEnoughRubyPopup>(true);
            if (popup != null)
            {
                popup.Open();
            }
            else
            {
                Debug.LogWarning("[RubyManager] NotEnoughRubyPopup을 찾을 수 없습니다.");
            }
        }
    }
}
