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

            nameText.text = blueprint.upgradeName;
            itemImage.sprite = blueprint.upgradeSprite;
            costText.text = $"BUY ${blueprint.cost} (Lv. {blueprint.level}/{blueprint.maxLevel})";

            buyButton.interactable = blueprint.level < blueprint.maxLevel;
            buyButton.onClick.AddListener(BuyUpgrade);
        }

        private void BuyUpgrade()
        {
            Debug.Log("Attempting to buy upgrade...");
            int coins = PlayerPrefs.GetInt("Coins", 0);

            if (coins >= itemBlueprint.cost && itemBlueprint.level < itemBlueprint.maxLevel)
            {
                Debug.Log("Upgrade purchase successful.");
                PlayerPrefs.SetInt("Coins", coins - itemBlueprint.cost);
                itemBlueprint.level++;

                // ✅ 실제 업그레이드 효과 누적 적용
                switch (itemBlueprint.type)
                {
                    case UpgradeType.ProjectileUpgrade:
                        CrossSceneData.BonusProjectile++;
                        break;
                    case UpgradeType.DamageUpgrade:
                        CrossSceneData.BonusDamage++;
                        break;
                    case UpgradeType.HPUpgrade:
                        CrossSceneData.BonusHP++;
                        break;
                    case UpgradeType.SpeedUpgrade:
                        CrossSceneData.BonusSpeed++;
                        break;
                }

                // ✅ UI 업데이트
                costText.text = $"BUY ${itemBlueprint.cost} (Lv. {itemBlueprint.level}/{itemBlueprint.maxLevel})";

                // ✅ 더 이상 못 사면 비활성화
                buyButton.interactable = itemBlueprint.level < itemBlueprint.maxLevel;

                coinDisplay.UpdateDisplay();
            }
            else
            {
                Debug.Log("Not enough coins or already max level.");
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
