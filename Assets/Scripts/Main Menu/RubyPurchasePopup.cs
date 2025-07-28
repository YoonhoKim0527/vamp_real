using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class RubyPurchasePopup : MonoBehaviour
    {
        [SerializeField] private Button buy10Button;
        [SerializeField] private Button buy100Button;
        [SerializeField] private Button buy1000Button;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            buy10Button.onClick.AddListener(() => OnPurchase(10));
            buy100Button.onClick.AddListener(() => OnPurchase(100));
            buy1000Button.onClick.AddListener(() => OnPurchase(1000));
            closeButton.onClick.AddListener(Close);
        }

        private void OnPurchase(int amount)
        {
            Debug.Log($"[RubyPurchasePopup] 유저가 {amount} 루비를 구매하려 함. 실제 구매 로직은 추후 구현");
            // TODO: 실제 결제 로직 붙이기
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
