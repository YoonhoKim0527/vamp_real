using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class UpgradeItemCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image itemImage;
        [SerializeField] private RectTransform itemImageRect;
        // [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Button buyButton;

        private Upgrade upgrade;
        private UpgradeItemBlueprint itemBlueprint;
        private CoinDisplay coinDisplay;

        public void Init(Upgrade upgrade, UpgradeItemBlueprint blueprint, CoinDisplay coinDisplay)
        {
            this.upgrade = upgrade;
            this.itemBlueprint = blueprint;
            this.coinDisplay = coinDisplay;

            Debug.Log($"Init UpgradeItemCard: {blueprint.upgradeName}, {blueprint.cost}, {blueprint.upgradeSprite}");

            nameText.text = blueprint.upgradeName;
            // descriptionText.text = blueprint.description;
            itemImage.sprite = blueprint.upgradeSprite;
            costText.text = "BUY $" + blueprint.cost.ToString();
            buyButton.interactable = !blueprint.purchased;

            buyButton.onClick.AddListener(BuyUpgrade);
        }

        private void BuyUpgrade()
        {
            Debug.Log("Attempting to buy upgrade...");
            int coins = PlayerPrefs.GetInt("Coins", 0);
            if (coins >= itemBlueprint.cost && !itemBlueprint.purchased)
            {
                Debug.Log("Upgrade purchase successful.");
                PlayerPrefs.SetInt("Coins", coins - itemBlueprint.cost);
                itemBlueprint.purchased = true;
                buyButton.interactable = false;

                // 🛠️ 실제 업그레이드 효과 적용
                switch (itemBlueprint.type)
                {
                    case UpgradeType.ProjectilePlus:
                        CrossSceneData.ExtraProjectile = true;
                        break;
                    case UpgradeType.DamageUp:
                        CrossSceneData.ExtraDamage = true;
                        break;
                    case UpgradeType.MaxHPUp:
                        CrossSceneData.ExtraHP = true;
                        break;
                    case UpgradeType.MoveSpeedUp:
                        CrossSceneData.ExtraSpeed = true;
                        break;
                }

                coinDisplay.UpdateDisplay();
            }
        }

        public void UpdateLayout()
        {
            float yHeight = Mathf.Abs(itemImageRect.sizeDelta.y);
            float xWidth = itemBlueprint.upgradeSprite.textureRect.width / (float)itemBlueprint.upgradeSprite.textureRect.height * yHeight;
            if (xWidth > Mathf.Abs(itemImageRect.sizeDelta.x))
            {
                xWidth = Mathf.Abs(itemImageRect.sizeDelta.x);
                yHeight = itemBlueprint.upgradeSprite.textureRect.height / (float)itemBlueprint.upgradeSprite.textureRect.width * xWidth;
            }
            ((RectTransform)itemImage.transform).sizeDelta = new Vector2(xWidth, yHeight);
        }
    }
}
