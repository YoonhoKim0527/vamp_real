using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Vampire
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private List<ShopItemBlueprint> allItems;
        [SerializeField] private List<UpgradeItemBlueprint> allUpgrades;

        private SaveManager saveManager;

        void Awake()
        {
            saveManager = GetComponent<SaveManager>();
            if (saveManager == null)
            {
                Debug.LogError("[GameStateManager] SaveManager component not found!");
                return;
            }
        }

        void Start()
        {
            Debug.Log("[GameStateManager] SaveManager found. Loading game data.");
            LoadGame();

            // ✅ Shop Init 호출 (세이브 데이터 로드 후에)
            var shop = FindObjectsOfType<Shop>(true).FirstOrDefault();
            if (shop != null)
            {
                if (!shop.gameObject.activeSelf)
                {
                    shop.gameObject.SetActive(true); // 비활성 상태면 활성화
                }

                if (!shop.IsInitialized)
                {
                    shop.Init();
                }

                shop.RefreshShopUI();
            }
            else
            {
                Debug.LogWarning("[GameStateManager] Shop not found in scene.");
            }
        }

        public void SaveGame()
        {
            if (saveManager == null)
            {
                Debug.LogError("[GameStateManager] SaveManager missing! Cannot save.");
                return;
            }

            if (allItems == null || allItems.Count == 0)
            {
                Debug.LogWarning("[GameStateManager] No ShopItems to save.");
            }

            if (allUpgrades == null || allUpgrades.Count == 0)
            {
                Debug.LogWarning("[GameStateManager] No Upgrades to save.");
            }

            saveManager.SaveGame(allItems, allUpgrades);
        }

        public void LoadGame()
        {
            SaveData data = saveManager.LoadGame();

            if (data.ownedItems != null)
            {
                foreach (var savedItem in data.ownedItems)
                {
                    var item = allItems.Find(i => i.itemName == savedItem.itemName);
                    if (item != null)
                    {
                        item.owned = savedItem.owned;
                        Debug.Log($"[GameStateManager] Restored {savedItem.itemName} = {savedItem.owned}");
                    }
                    else
                    {
                        Debug.LogWarning($"[GameStateManager] Item not found: {savedItem.itemName}");
                    }
                }
            }

            if (data.upgradeLevels != null)
            {
                foreach (var savedUpgrade in data.upgradeLevels)
                {
                    var upgrade = allUpgrades.Find(u => u.upgradeName == savedUpgrade.upgradeName);
                    if (upgrade != null)
                    {
                        upgrade.level = savedUpgrade.level;
                        Debug.Log($"[GameStateManager] Restored {savedUpgrade.upgradeName} = {savedUpgrade.level}");
                    }
                    else
                    {
                        Debug.LogWarning($"[GameStateManager] Upgrade not found: {savedUpgrade.upgradeName}");
                    }
                }
            }

            // ✅ Shop UI 리렌더링
            var shop = FindObjectsOfType<Shop>(true).FirstOrDefault();
            if (shop != null)
            {
                shop.RefreshShopUI();
            }
            else
            {
                Debug.LogWarning("[GameStateManager] Shop not found in scene.");
            }
        }
    }
}
