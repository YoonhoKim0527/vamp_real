using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class ShopItemCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image itemImage;
        [SerializeField] private RectTransform itemImageRect;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Button buyButton;

        private Shop shop;
        private ShopItemBlueprint itemBlueprint;
        private CoinDisplay coinDisplay;

        public void Init(Shop shop, ShopItemBlueprint blueprint, CoinDisplay coinDisplay)
        {
            this.shop = shop;
            this.itemBlueprint = blueprint;

            // ✅ CoinDisplay 연결: 인자로 전달 못 받으면 자동 탐색
            this.coinDisplay = coinDisplay != null ? coinDisplay : FindObjectOfType<CoinDisplay>();
            if (this.coinDisplay == null)
            {
                Debug.LogWarning("[ShopItemCard] CoinDisplay not found! UI may not update coins.");
            }

            Debug.Log($"Init ShopItemCard: {blueprint.itemName}, {blueprint.cost}, {blueprint.itemSprite}");

            Refresh(); // ✅ 상태를 UI에 반영
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyItem);
        }

        private void BuyItem()
        {
            int coins = PlayerPrefs.GetInt("Coins", 0);
            if (coins >= itemBlueprint.cost && !itemBlueprint.owned)
            {
                PlayerPrefs.SetInt("Coins", coins - itemBlueprint.cost);
                itemBlueprint.owned = true;
                Debug.Log($"[ShopItemCard] Purchased {itemBlueprint.itemName}");

                // ✅ 게임 상태 저장
                var gameStateManager = FindObjectOfType<GameStateManager>();
                if (gameStateManager != null)
                {
                    gameStateManager.SaveGame();
                }

                // ✅ Shop 전체 리프레시 (코인 포함)
                if (shop != null)
                {
                    shop.RefreshShopUI();
                }

                // ✅ 메인 UI의 CoinDisplay도 갱신
                var mainCoinDisplay = FindObjectOfType<CoinDisplay>();
                if (mainCoinDisplay != null)
                {
                    mainCoinDisplay.UpdateDisplay();
                }
            }
        }


        public void Refresh()
        {
            // ✅ 구매 여부에 따라 UI 갱신
            nameText.text = itemBlueprint.itemName;
            itemImage.sprite = itemBlueprint.itemSprite;

            if (itemBlueprint.owned)
            {
                costText.text = "OWNED";
                buyButton.interactable = false;
                buttonImage.color = Color.gray; // 비활성화된 버튼 색
            }
            else
            {
                costText.text = "BUY $" + itemBlueprint.cost.ToString();
                buyButton.interactable = true;
                buttonImage.color = Color.white; // 기본 버튼 색
            }
        }

        public void UpdateLayout()
        {
            float yHeight = Mathf.Abs(itemImageRect.sizeDelta.y);
            float xWidth = itemBlueprint.itemSprite.textureRect.width / (float)itemBlueprint.itemSprite.textureRect.height * yHeight;
            if (xWidth > Mathf.Abs(itemImageRect.sizeDelta.x))
            {
                xWidth = Mathf.Abs(itemImageRect.sizeDelta.x);
                yHeight = itemBlueprint.itemSprite.textureRect.height / (float)itemBlueprint.itemSprite.textureRect.width * xWidth;
            }
            ((RectTransform)itemImage.transform).sizeDelta = new Vector2(xWidth, yHeight);
        }
    }
}
