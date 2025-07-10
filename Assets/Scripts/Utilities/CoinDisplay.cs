using UnityEngine;
using TMPro;

namespace Vampire
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CoinDisplay : MonoBehaviour
    {
        private TextMeshProUGUI coinText;

        void Start()
        {
            coinText = GetComponent<TextMeshProUGUI>();
            if (coinText == null)
            {
                Debug.LogError("[CoinDisplay] Missing TextMeshProUGUI on this GameObject!");
                return;
            }

            coinText.text = PlayerPrefs.GetInt("Coins", 0).ToString();
        }

        public void UpdateDisplay()
        {
            coinText = GetComponent<TextMeshProUGUI>();
            if (coinText == null)
            {
                Debug.LogError("[CoinDisplay] Missing TextMeshProUGUI. Cannot update display.");
                return;
            }

            coinText.text = PlayerPrefs.GetInt("Coins", 0).ToString();
        }
    }
}
