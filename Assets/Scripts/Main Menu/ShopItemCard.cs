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
            this.coinDisplay = coinDisplay;
            
            Debug.Log($"Init ShopItemCard: {blueprint.itemName}, {blueprint.cost}, {blueprint.itemSprite}");

            nameText.text = blueprint.itemName.ToString();
            itemImage.sprite = blueprint.itemSprite;
            costText.text = "BUY $" + blueprint.cost.ToString();
            buyButton.interactable = !blueprint.owned;

            buyButton.onClick.AddListener(BuyItem);

        }

        private void BuyItem()
        {
            Debug.Log("buy1");
            int coins = PlayerPrefs.GetInt("Coins", 0);
            if (coins >= itemBlueprint.cost && !itemBlueprint.owned)
            {
                Debug.Log("buy2");
                PlayerPrefs.SetInt("Coins", coins - itemBlueprint.cost);
                itemBlueprint.owned = true;
                buyButton.interactable = false;
                Debug.Log(itemBlueprint.type);
                // 🔥 아이템 타입에 따른 효과 처리
                if (itemBlueprint.type == ShopItemType.ProjectileUpgrade)
                {
                    Debug.Log("bu3");
                    CrossSceneData.ExtraProjectile = true;
                }
                coinDisplay.UpdateDisplay();
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