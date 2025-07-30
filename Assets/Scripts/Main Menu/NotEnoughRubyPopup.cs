using UnityEngine;
using UnityEngine.UI;
using System;

namespace Vampire
{
    public class NotEnoughRubyPopup : MonoBehaviour
    {
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private RubyPurchasePopup purchasePopup; // ✅ 인스펙터에서 연결 필요

        private void Awake()
        {
            yesButton.onClick.AddListener(OnYesClicked);
            noButton.onClick.AddListener(Close);
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        private void OnYesClicked()
        {
            gameObject.SetActive(false);
            Invoke(nameof(ShowPurchasePopup), 0.01f); // ✅ 다음 프레임에 안전하게 실행
        }

        private void ShowPurchasePopup()
        {
            if (purchasePopup != null)
            {
                purchasePopup.Open();
            }
            else
            {
                Debug.LogWarning("[NotEnoughRubyPopup] RubyPurchasePopup이 연결되어 있지 않음");
            }
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
