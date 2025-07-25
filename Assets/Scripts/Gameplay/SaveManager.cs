using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Vampire
{
    [System.Serializable]
    public class UpgradeStateSaveData
    {
        public string upgradeName;         // 업그레이드 이름
        public int level;                  // 현재 레벨
        public int nextCost;               // 다음 업그레이드 비용
        public float upgradeIncrement;     // 업그레이드 시 증가량
    }

    [System.Serializable]
    public class EquippedItemSaveData
    {
        public EquipmentType type;
        public string equipmentName;
        public int tier; // ✅ 고유 식별을 위한 추가
    }

    [System.Serializable]
    public class SaveData
    {
        public List<ItemSaveData> ownedItems;
        public List<UpgradeSaveData> upgradeLevels;
        public List<UpgradeStateSaveData> upgradeStates; // ✅ 새 필드
        public CharacterStatBlueprint playerStats; // ✅ 추가: 캐릭터 스탯 데이터
        public ExpeditionSaveData expeditionData; 
        public List<EquippedItemSaveData> equippedItems;
    }

    [System.Serializable]
    public class ItemSaveData
    {
        public string itemName;
        public bool owned;
    }

    [System.Serializable]
    public class UpgradeSaveData
    {
        public string upgradeName;
        public int level;
    }
    [System.Serializable]
    public class ExpeditionSaveData
    {
        public int bossIndex;
        public float bossCurrentHP;
        public string bossBlueprintName;

        [System.Serializable]
        public class CharacterData
        {
            public string characterName;
            public Vector2 position;
            public bool facingLeft;
        }

        public List<CharacterData> characters = new();
        public List<BoostSaveData> activeBoosts = new();
        public List<string> selectedCharacterNames = new();
    }

    [System.Serializable]
    public class BoostSaveData
    {
        public BoostType type;
        public float remainingTime;
    }


    public class SaveManager : MonoBehaviour
    {
        private string savePath;

        void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, "savefile.json");
            Debug.Log($"[SaveManager] Save path: {savePath}");
        }

        /// <summary>
        /// 게임 데이터 저장 (아이템, 업그레이드, 캐릭터 스탯 포함)
        /// </summary>
        /// overloading 
        public void SaveGame(
            List<ShopItemBlueprint> items,
            List<UpgradeItemBlueprint> upgrades,
            CharacterStatBlueprint playerStats,
            List<UpgradeStateSaveData> upgradeStates
        )
        {
            var oldData = LoadGame();
            SaveGame(items, upgrades, playerStats, upgradeStates, oldData.equippedItems);
        }
        
        public void SaveGame(
            List<ShopItemBlueprint> items,
            List<UpgradeItemBlueprint> upgrades,
            CharacterStatBlueprint playerStats,
            List<UpgradeStateSaveData> upgradeStates,
            List<EquippedItemSaveData> equippedItems
        )
        {
            SaveData data = new SaveData();

            // ✅ Shop Items 저장
            data.ownedItems = new List<ItemSaveData>();
            if (items != null) // null 방지
            {
                foreach (var item in items)
                {
                    data.ownedItems.Add(new ItemSaveData
                    {
                        itemName = item.itemName,
                        owned = item.owned
                    });
                }
            }

            // equiped items 저장
            data.equippedItems = equippedItems ?? new List<EquippedItemSaveData>();

            // ✅ Upgrade Levels 저장
            data.upgradeLevels = new List<UpgradeSaveData>();
            if (upgrades != null) // null 방지
            {
                foreach (var upgrade in upgrades)
                {
                    data.upgradeLevels.Add(new UpgradeSaveData
                    {
                        upgradeName = upgrade.upgradeName,
                        level = upgrade.level
                    });
                }
            }

            // ✅ Upgrade States 저장
            data.upgradeStates = upgradeStates ?? new List<UpgradeStateSaveData>();

            // ✅ Player Stats 저장
            data.playerStats = playerStats ?? new CharacterStatBlueprint();

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"[SaveManager] Game saved at: {savePath}");
        }
        /// <summary>
        /// 게임 데이터 로드
        /// </summary>
        public SaveData LoadGame()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveManager] Game loaded from {savePath}");
                return data;
            }
            else
            {
                Debug.LogWarning("[SaveManager] No save file found. Initializing new SaveData.");
                return new SaveData
                {
                    ownedItems = new List<ItemSaveData>(),
                    upgradeLevels = new List<UpgradeSaveData>(),
                    upgradeStates = new List<UpgradeStateSaveData>(), // ✅ 기본 초기화
                    playerStats = new CharacterStatBlueprint()
                };
            }
        }

        public void SaveStats(CharacterStatBlueprint stats)
        {
            SaveData data = LoadGame(); // 기존 데이터 불러오기
            data.playerStats = stats;

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log("[SaveManager] Player stats saved.");
        }

        public CharacterStatBlueprint LoadStats()
        {
            SaveData data = LoadGame();
            if (data.playerStats != null)
            {
                Debug.Log("[SaveManager] Player stats loaded.");
                return data.playerStats;
            }
            else
            {
                Debug.LogWarning("[SaveManager] No player stats found in save. Initializing defaults.");
                return new CharacterStatBlueprint(); // 기본값 반환
            }
        }
        public void SaveExpeditionData
        (
            int bossIndex,
            float bossHP,
            string bossName,
            List<GameObject> characterObjects,
            List<CharacterBlueprint> blueprints
        )
        {
            ExpeditionSaveData data = new();
            data.bossIndex = bossIndex;
            data.bossCurrentHP = bossHP;
            data.bossBlueprintName = bossName;

            // 캐릭터 저장
            foreach (var obj in characterObjects)
            {
                var character = obj.GetComponent<ExpeditionCharacter>();
                if (character == null) continue;

                string blueprintName = character.GetBlueprintName();
                bool facingLeft = obj.transform.localScale.x < 0;

                data.characters.Add(new ExpeditionSaveData.CharacterData
                {
                    characterName = blueprintName,
                    position = obj.transform.position,
                    facingLeft = facingLeft
                });
            }
            // ✅ selectedCharacters 순서대로 이름 저장
            foreach (var bp in blueprints)
            {
                if (bp != null)
                    data.selectedCharacterNames.Add(bp.name);
                else
                    data.selectedCharacterNames.Add(""); // 빈 슬롯
            }

            // Boost 저장
            foreach (var kv in BoostManager.Instance.RemainingTimes)
            {
                data.activeBoosts.Add(new BoostSaveData
                {
                    type = kv.Key,
                    remainingTime = kv.Value
                });
            }

            // 저장
            SaveData fullData = LoadGame();
            fullData.expeditionData = data;
            string json = JsonUtility.ToJson(fullData, true);
            File.WriteAllText(savePath, json);
            Debug.Log("[SaveManager] Expedition data saved.");
        }

        public ExpeditionSaveData LoadExpeditionData()
        {
            SaveData data = LoadGame();
            return data.expeditionData ?? new ExpeditionSaveData();
        }
    }
}
