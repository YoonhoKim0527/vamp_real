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

            // 🔹 상점 아이템 효과 반영
            if (CrossSceneData.ExtraDamage)
                damageMultiplier += 0.2f;
            if (CrossSceneData.ExtraHP)
                hpMultiplier += 0.2f;  // 예: 체력 30% 증가
            if (CrossSceneData.ExtraSpeed)
                speedMultiplier += 0.5f;

            // 🔹 업그레이드 누적 적용
            ApplyUpgradeMultiplier();
        }

        public void ApplyUpgradeMultiplier()
        {
            damageMultiplier += 0.1f * CrossSceneData.BonusDamage;
            hpMultiplier += 0.5f * CrossSceneData.BonusHP;
            speedMultiplier += 0.5f * CrossSceneData.BonusSpeed;
            Debug.Log($"[Upgrade] Damage x{damageMultiplier}, HP x{hpMultiplier}, Speed x{speedMultiplier}");
        }

        // 🔹 데미지 계산
        public float GetTotalDamage()
        {
            return baseDamage * (damageMultiplier);
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
    }
}