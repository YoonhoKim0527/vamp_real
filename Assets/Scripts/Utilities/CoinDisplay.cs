using UnityEngine;
using TMPro;

namespace Vampire
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CoinDisplay : MonoBehaviour
    {
        private TextMeshProUGUI coinText;

        private void Awake()
        {
            coinText = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            // ✅ 코인 값이 변경될 때마다 UpdateDisplay 실행
            CoinManager.Instance.OnCoinsChanged += UpdateDisplay;
            UpdateDisplay(CoinManager.Instance.GetCoins()); // 초기화
        }

        private void OnDisable()
        {
            if (CoinManager.Instance != null)
                CoinManager.Instance.OnCoinsChanged -= UpdateDisplay;
        }

        private void UpdateDisplay(int coins)
        {
            coinText.text = coins.ToString();
        }
    }
}
