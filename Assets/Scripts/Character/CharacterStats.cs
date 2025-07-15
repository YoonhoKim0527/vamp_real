using UnityEngine;

namespace Vampire
{
    public class CharacterStats
    {
        float baseDamage;
        float baseHP;
        float baseSpeed;

        float damageMultiplier = 1f;
        float hpMultiplier = 1f;
        float speedMultiplier = 1f;

        public CharacterStats(CharacterBlueprint blueprint)
        {
            baseDamage = blueprint.baseDamage;
            baseHP = blueprint.hp;
            baseSpeed = blueprint.movespeed;

            // 🔥 savefile.json에서 업그레이드 데이터 로드
            ApplySavedUpgrades();

            Debug.Log($"[CharacterStats] Damage Multiplier: {damageMultiplier}");
            Debug.Log($"[CharacterStats] HP Multiplier: {hpMultiplier}");
            Debug.Log($"[CharacterStats] Speed Multiplier: {speedMultiplier}");
        }

        private void ApplySavedUpgrades()
        {
            var saveManager = Object.FindObjectOfType<SaveManager>();
            if (saveManager == null)
            {
                Debug.LogError("[CharacterStats] SaveManager not found in scene! Cannot load upgrades.");
                return;
            }

            SaveData data = saveManager.LoadGame();
            if (data == null)
            {
                Debug.LogWarning("[CharacterStats] No SaveData found. Using base stats.");
                return;
            }

            foreach (var upgrade in data.upgradeLevels)
            {
                Debug.Log($"[CharacterStats] Applying Upgrade: {upgrade.upgradeName} Lv.{upgrade.level}");

                switch (upgrade.upgradeName)
                {
                    case "strong":
                        damageMultiplier += 0.1f * upgrade.level;
                        break;

                    case "a":
                        hpMultiplier += 0.5f * upgrade.level;
                        break;

                    case "c":
                        speedMultiplier += 0.2f * upgrade.level;
                        break;

                    case "b":
                        CrossSceneData.BonusProjectile = upgrade.level;
                        break;

                    default:
                        Debug.LogWarning($"[CharacterStats] Unknown upgrade: {upgrade.upgradeName}");
                        break;
                }
            }
        }

        // 🔹 데미지 계산
        public float GetTotalDamage()
        {
            return baseDamage * damageMultiplier;
        }

        // 🔹 체력 계산
        public float GetTotalHP()
        {
            return baseHP * hpMultiplier;
        }

        // 🔹 이동속도 계산
        public float GetTotalSpeed()
        {
            return baseSpeed * speedMultiplier;
        }

        public void RecalculateFromSave()
        {
            // 기존 multiplier 초기화
            damageMultiplier = 1f;
            hpMultiplier = 1f;
            speedMultiplier = 1f;
            CrossSceneData.BonusProjectile = 0;

            // 다시 savefile.json 로드 후 재계산
            ApplySavedUpgrades();

            Debug.Log("[CharacterStats] Stats recalculated.");
        }

    }
}
