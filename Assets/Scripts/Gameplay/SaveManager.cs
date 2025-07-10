using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Vampire
{
    [System.Serializable]
    public class SaveData
    {
        public List<ItemSaveData> ownedItems;
        public List<UpgradeSaveData> upgradeLevels;
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


    public class SaveManager : MonoBehaviour
    {
        private string savePath;

        void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, "savefile.json");
            Debug.Log($"[SaveManager] Save path: {savePath}");
        }

        public void SaveGame(List<ShopItemBlueprint> items, List<UpgradeItemBlueprint> upgrades)
        {
            SaveData data = new SaveData();

            data.ownedItems = new List<ItemSaveData>();

            Debug.Log($"[SaveGame] Saving {items.Count} items.");
            foreach (var item in items)
            {
                Debug.Log($"[SaveGame] Item in list: {item.itemName} | owned = {item.owned}");
            }

            foreach (var item in items)
            {
                Debug.Log($"[SaveManager] Saving Item: '{item.itemName}' Owned: {item.owned}");
                data.ownedItems.Add(new ItemSaveData
                {
                    itemName = item.itemName,
                    owned = item.owned
                });
            }

            data.upgradeLevels = new List<UpgradeSaveData>();
            foreach (var upgrade in upgrades)
            {
                Debug.Log($"[SaveManager] Saving Upgrade: '{upgrade.upgradeName}' Level: {upgrade.level}");
                data.upgradeLevels.Add(new UpgradeSaveData
                {
                    upgradeName = upgrade.upgradeName,
                    level = upgrade.level
                });
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"[SaveManager] Game saved at: {savePath}");
            Debug.Log($"[SaveManager] JSON Content:\n{json}");
        }

        public SaveData LoadGame()
        {
            Debug.Log($"[SaveManager] Save path at load: {savePath}");
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveManager] Game loaded.");
                return data;
            }
            else
            {
                Debug.LogWarning("[SaveManager] No save file found. Initializing new SaveData.");
                return new SaveData
                {
                    ownedItems = new List<ItemSaveData>(),
                    upgradeLevels = new List<UpgradeSaveData>()
                };
            }
        }
    }
}
