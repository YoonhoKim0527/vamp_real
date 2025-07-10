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
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button buyButton;

        private Upgrade upgrade;
        private UpgradeItemBlueprint itemBlueprint;
        private CoinDisplay coinDisplay;

        public void Init(Upgrade upgrade, UpgradeItemBlueprint blueprint, CoinDisplay coinDisplay)
        {
            this.upgrade = upgrade;
            this.itemBlueprint = blueprint;

            if (coinDisplay == null)
            {
                this.coinDisplay = FindObjectOfType<CoinDisplay>();
            }
            else
            {
                this.coinDisplay = coinDisplay;
            }

            nameText.text = blueprint.upgradeName;

            // ✅ Sprite가 null일 때만 설정
            if (blueprint.upgradeSprite != null)
            {
                itemImage.sprite = blueprint.upgradeSprite;
            }
            else
            {
                Debug.LogWarning($"[UpgradeItemCard] {blueprint.upgradeName} has no sprite assigned!");
            }

            Refresh();
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyUpgrade);
        }

        private void BuyUpgrade()
        {
            int coins = PlayerPrefs.GetInt("Coins", 0);

            if (coins >= itemBlueprint.cost && itemBlueprint.level < itemBlueprint.maxLevel)
            {
                // ✅ 돈 차감
                PlayerPrefs.SetInt("Coins", coins - itemBlueprint.cost);

                // ✅ 업그레이드 상태 갱신
                itemBlueprint.level++;
                Debug.Log($"[UpgradeItemCard] Purchased upgrade: {itemBlueprint.upgradeName} (Level {itemBlueprint.level})");

                // ✅ 업그레이드 효과 적용
                switch (itemBlueprint.type)
                {
                    case UpgradeType.ProjectileUpgrade: CrossSceneData.BonusProjectile++; break;
                    case UpgradeType.DamageUpgrade: CrossSceneData.BonusDamage++; break;
                    case UpgradeType.HPUpgrade: CrossSceneData.BonusHP++; break;
                    case UpgradeType.SpeedUpgrade: CrossSceneData.BonusSpeed++; break;
                }

                // ✅ SaveGame 호출
                var gameStateManager = FindObjectOfType<GameStateManager>();
                if (gameStateManager != null)
                {
                    gameStateManager.SaveGame();
                    Debug.Log($"[UpgradeItemCard] Saved game after upgrading {itemBlueprint.upgradeName}.");
                }
                else
                {
                    Debug.LogWarning("[UpgradeItemCard] GameStateManager not found! SaveGame skipped.");
                }

                // ✅ Upgrade UI 전체 리프레시
                if (upgrade != null)
                {
                    upgrade.RefreshUpgradeUI();
                    Debug.Log("[UpgradeItemCard] Upgrade UI refreshed.");
                }
                else
                {
                    Debug.LogWarning("[UpgradeItemCard] Upgrade reference missing. Refresh skipped.");
                }

                // ✅ 코인 UI 강제 갱신 (업그레이드 외부 포함)
                var allCoinDisplays = FindObjectsOfType<CoinDisplay>();
                foreach (var display in allCoinDisplays)
                {
                    display.UpdateDisplay();
                }
            }
            else
            {
                Debug.LogWarning("[UpgradeItemCard] Not enough coins or already at max level.");
            }
        }

        public void Refresh()
        {
            costText.text = $"BUY ${itemBlueprint.cost} (Lv. {itemBlueprint.level}/{itemBlueprint.maxLevel})";
            buyButton.interactable = itemBlueprint.level < itemBlueprint.maxLevel;
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
