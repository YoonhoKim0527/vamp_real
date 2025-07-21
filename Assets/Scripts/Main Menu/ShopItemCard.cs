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

        public void Init(Shop shop, ShopItemBlueprint blueprint)
        {
            this.shop = shop;
            this.itemBlueprint = blueprint;

            Debug.Log($"Init ShopItemCard: {blueprint.itemName}, {blueprint.cost}, {blueprint.itemSprite}");

            Refresh(); // ✅ 상태를 UI에 반영
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyItem);
        }

        private void BuyItem()
        {
            int currentCoins = CoinManager.Instance.GetCoins(); // ✅ CoinManager로 코인 조회

            if (currentCoins >= itemBlueprint.cost && !itemBlueprint.owned)
            {
                // ✅ 코인 차감
                CoinManager.Instance.SpendCoins(itemBlueprint.cost);

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
            }
            else if (itemBlueprint.owned)
            {
                Debug.LogWarning($"[ShopItemCard] {itemBlueprint.itemName} already owned.");
            }
            else
            {
                Debug.LogWarning($"[ShopItemCard] Not enough coins! Current: {currentCoins}, Needed: {itemBlueprint.cost}");
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
