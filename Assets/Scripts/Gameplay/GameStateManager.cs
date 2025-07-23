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

        [SerializeField] private CharacterBlueprint selectedCharacter; // ✅ 선택된 캐릭터

        private CharacterStatBlueprint originalStats; // ✅ 원본 스탯 백업

        public bool IsInitialized { get; private set; } = false; // ✅ 초기화 완료 여부

        void Awake()
        {
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
                {
                    Debug.LogError("[GameStateManager] UpgradeManager not found in scene! Cannot load player stats.");
                }
                else
                {
                    Debug.Log("[GameStateManager] Found UpgradeManager in scene.");
                }
            }
        }

        void Start()
        {
            Debug.Log("[GameStateManager] Loading game data...");
            LoadGame();
            IsInitialized = true; // ✅ 초기화 완료 플래그 설정

            // ✅ 선택한 캐릭터를 CrossSceneData에서 받아오기
            if (CrossSceneData.CharacterBlueprint != null)
            {
                selectedCharacter = CrossSceneData.CharacterBlueprint;
                Debug.Log($"[GameStateManager] Selected Character: {selectedCharacter.name}");
            }
            else
            {
                Debug.LogWarning("[GameStateManager] No CharacterBlueprint found in CrossSceneData.");
            }

            ApplyCharacterMultipliers(); // ✅ 게임 진입 시 곱연산

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene == "MainMenu") // ✅ MainMenu Scene일 때만
            {
                // ✅ Shop Init
                var shop = FindObjectsOfType<Shop>(true).FirstOrDefault();
                if (shop != null)
                {
                    if (!shop.gameObject.activeSelf)
                        shop.gameObject.SetActive(true);

                    if (!shop.IsInitialized)
                        shop.Init();

                    shop.RefreshShopUI();
                }
                else
                {
                    Debug.LogWarning("[GameStateManager] Shop not found in scene.");
                }

                // ✅ Upgrade Init
                var upgrade = FindObjectsOfType<Upgrade>(true).FirstOrDefault();
                if (upgrade != null)
                {
                    if (!upgrade.gameObject.activeSelf)
                        upgrade.gameObject.SetActive(true);

                    if (!upgrade.IsInitialized)
                        upgrade.RefreshUpgradeUI();
                }
                else
                {
                    Debug.LogWarning("[GameStateManager] Upgrade not found in scene.");
                }
            }
            else
            {
                Debug.Log($"[GameStateManager] Skipping Shop/Upgrade UI because current scene is {currentScene}");
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

            List<UpgradeStateSaveData> upgradeStates = new List<UpgradeStateSaveData>();
            if (upgradeManager != null)
            {
                upgradeStates = upgradeManager.GetUpgradeStates();
            }
            else
            {
                Debug.LogWarning("[GameStateManager] UpgradeManager not assigned. Saving empty upgradeStates.");
            }

            saveManager.SaveGame(allItems, allUpgrades, playerStats, upgradeStates);
        }

        public void LoadGame()
        {   
            SaveData data = saveManager.LoadGame();

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene == "MainMenu") // ✅ MainMenu Scene일 때만
            {
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

                if (upgradeManager != null)
                {
                    if (data.upgradeStates != null && data.upgradeStates.Count == 8)
                    {
                        upgradeManager.SetUpgradeStates(data.upgradeStates);
                        Debug.Log("[GameStateManager] Upgrade states restored.");
                    }
                    else
                    {
                        Debug.LogWarning("[GameStateManager] No saved upgradeStates found. Initializing defaults.");
                        upgradeManager.InitializeDefaultUpgradeStates();
                        SaveGame();
                    }

                    upgradeManager.RefreshAllUI();
                }
            }

            playerStats = data.playerStats ?? new CharacterStatBlueprint();
            Debug.Log("[GameStateManager] Player stats restored.");
        }

        public CharacterStatBlueprint PlayerStats => playerStats;

        public UpgradeManager UpgradeManager => upgradeManager;

        public List<UpgradeStateSaveData> GetUpgradeStatesSafe()
        {
            if (upgradeManager != null)
                return upgradeManager.GetUpgradeStates();

            Debug.LogWarning("[GameStateManager] UpgradeManager is null. Returning empty list.");
            return new List<UpgradeStateSaveData>();
        }

        public void ApplyCharacterMultipliers()
        {
            if (selectedCharacter == null)
            {
                Debug.LogWarning("[GameStateManager] No character selected. Skipping stat multipliers.");
                return;
            }

            // ✅ 원본 스탯 백업
            originalStats = new CharacterStatBlueprint();
            originalStats.CopyFrom(playerStats);

            // ✅ 곱연산 적용
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
                Debug.LogWarning("[GameStateManager] Original stats backup not found. Cannot reset.");
            }
        }
    }
}
