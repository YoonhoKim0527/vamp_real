using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Vampire
{
    public class GameStateManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private UpgradeManager upgradeManager;

        [Header("Data")]
        [SerializeField] private List<ShopItemBlueprint> allItems;
        [SerializeField] private List<UpgradeItemBlueprint> allUpgrades;
        [SerializeField] private CharacterStatBlueprint playerStats;

        private CharacterBlueprint selectedCharacter; // ✅ 씬 저장 방지 위해 SerializeField 제거
        private CharacterStatBlueprint originalStats;

        [SerializeField] private EquipmentManager equipmentManager;

        public bool IsInitialized { get; private set; } = false;

        public float TotalDamage => playerStats.attackPower;

        void Awake()
        {
            // ✅ 싱글턴 처리
            if (FindObjectsOfType<GameStateManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(this);

            if (saveManager == null)
                saveManager = GetComponent<SaveManager>();

            if (saveManager == null)
            {
                Debug.LogError("[GameStateManager] SaveManager component not found!");
                return;
            }

            if (upgradeManager == null)
            {
                upgradeManager = FindObjectOfType<UpgradeManager>();
                if (upgradeManager == null)
                    Debug.LogError("[GameStateManager] UpgradeManager not found in scene!");
                else
                    Debug.Log("[GameStateManager] Found UpgradeManager in scene.");
            }
        }

        void Start()
        {
            Debug.Log("[GameStateManager] Loading game data...");
            LoadGame();
            IsInitialized = true;

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"[GameStateManager] Current Scene: {currentScene}");

            // ❌ selectedCharacter 설정 제거
            // ❌ ApplyCharacterMultipliers() 호출 제거

            if (currentScene == "Main Menu")
            {
                InitShopAndUpgradeUI();
            }
        }


        public void SaveGame()
        {
            if (saveManager == null)
            {
                Debug.LogError("[GameStateManager] SaveManager missing! Cannot save.");
                return;
            }

            if (allItems == null) allItems = new List<ShopItemBlueprint>();
            if (allUpgrades == null) allUpgrades = new List<UpgradeItemBlueprint>();

            List<UpgradeStateSaveData> upgradeStates = upgradeManager?.GetUpgradeStates() ?? new List<UpgradeStateSaveData>();
            var equippedItems = equipmentManager?.GetEquippedItemsForSave() ?? new List<EquippedItemSaveData>();


            saveManager.SaveGame(allItems, allUpgrades, playerStats, upgradeStates, equippedItems);
        }

        public void LoadGame()
        {
            SaveData data = saveManager.LoadGame();

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"[GSM] current: {currentScene}");
            if (currentScene == "Main Menu")
            {
                Debug.Log("[GameStateManager] 122");
                RestoreItems(data);
                RestoreUpgrades(data);
                upgradeManager?.RefreshAllUI();

                Debug.Log("[GameStateManager] 1");

                // ✅ 여기 추가
                if (equipmentManager == null)
                {
                    equipmentManager = FindObjectOfType<EquipmentManager>();
                    Debug.LogWarning("[GameStateManager] equipmentManager is null, using FindObjectOfType fallback.");
                }
                Debug.Log("[GameStateManager] 2");

                if (equipmentManager != null && data.equippedItems != null)
                {
                    equipmentManager.LoadEquippedItems(data.equippedItems);
                    Debug.Log("[GameStateManager] Loaded equipped items into EquipmentManager.");
                }
                else
                {
                    Debug.LogWarning("[GameStateManager] EquipmentManager or equippedItems missing.");
                }
            }

            playerStats = data.playerStats ?? new CharacterStatBlueprint();
            Debug.Log("[GameStateManager] Player stats restored.");
        }

        private void RestoreItems(SaveData data)
        {
            if (data.ownedItems != null)
            {
                foreach (var savedItem in data.ownedItems)
                {
                    var item = allItems.Find(i => i.itemName == savedItem.itemName);
                    if (item != null)
                        item.owned = savedItem.owned;
                }
            }
        }

        private void RestoreUpgrades(SaveData data)
        {
            if (data.upgradeLevels != null)
            {
                foreach (var savedUpgrade in data.upgradeLevels)
                {
                    var upgrade = allUpgrades.Find(u => u.upgradeName == savedUpgrade.upgradeName);
                    if (upgrade != null)
                        upgrade.level = savedUpgrade.level;
                }
            }

            if (data.upgradeStates != null && data.upgradeStates.Count == 8)
                upgradeManager?.SetUpgradeStates(data.upgradeStates);
            else
                upgradeManager?.InitializeDefaultUpgradeStates();
        }

        private void InitShopAndUpgradeUI()
        {
            var shop = FindObjectsOfType<Shop>(true).FirstOrDefault();
            if (shop != null)
            {
                if (!shop.gameObject.activeSelf)
                    shop.gameObject.SetActive(true);

                if (!shop.IsInitialized)
                    shop.Init();

                shop.RefreshShopUI();
            }

            var upgrade = FindObjectsOfType<Upgrade>(true).FirstOrDefault();
            if (upgrade != null)
            {
                if (!upgrade.gameObject.activeSelf)
                    upgrade.gameObject.SetActive(true);

                if (!upgrade.IsInitialized)
                    upgrade.RefreshUpgradeUI();
            }
        }

        public void ApplyCharacterMultipliers()
        {
            if (selectedCharacter == null)
            {
                Debug.LogWarning("[GameStateManager] selectedCharacter is NULL. 곱연산 스킵됨.");
                return;
            }

            Debug.Log($"[GameStateManager] ApplyCharacterMultipliers() - selectedCharacter: {selectedCharacter.name}");

            originalStats = new CharacterStatBlueprint();
            originalStats.CopyFrom(playerStats);

            playerStats.attackPower *= selectedCharacter.baseDamage;
            playerStats.maxHealth *= selectedCharacter.hp;
            playerStats.moveSpeed *= selectedCharacter.movespeed;
            playerStats.defense *= selectedCharacter.armor;
            playerStats.healthRegen *= selectedCharacter.recovery;
            playerStats.criticalChance *= selectedCharacter.luck;

            Debug.Log("[GameStateManager] Character stat multipliers applied.");
        }

        public void ResetCharacterStats()
        {
            if (originalStats != null)
            {
                playerStats.CopyFrom(originalStats);
                Debug.Log("[GameStateManager] Character stats reset to original values.");
            }
            else
            {
                Debug.LogWarning("[GameStateManager] No backup stats found.");
            }
        }

        public CharacterStatBlueprint PlayerStats => playerStats;
        public UpgradeManager UpgradeManager => upgradeManager;

        public List<UpgradeStateSaveData> GetUpgradeStatesSafe()
        {
            return upgradeManager != null ? upgradeManager.GetUpgradeStates() : new List<UpgradeStateSaveData>();
        }

        void OnApplicationQuit()
        {
            selectedCharacter = null;
            Debug.Log("[GameStateManager] OnApplicationQuit() - selectedCharacter cleared.");
        }

        // GameStateManager.cs
        public void SetSelectedCharacter(CharacterBlueprint blueprint)
        {
            selectedCharacter = blueprint;
            Debug.Log($"[GameStateManager] SetSelectedCharacter() 호출됨: {selectedCharacter.name}");
        }
    }
}
