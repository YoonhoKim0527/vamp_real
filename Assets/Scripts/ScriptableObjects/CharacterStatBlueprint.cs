using System;
using UnityEngine;
namespace Vampire
{
    [CreateAssetMenu(fileName = "CharacterStat", menuName = "Blueprints/CharacterStat", order = 1)]
    /// <summary>
    /// 캐릭터의 모든 스탯 정보를 담고 있는 Blueprint
    /// JSON 직렬화를 위해 Serializable로 선언
    /// </summary>
    /// 
    [Serializable]
    public class CharacterStatBlueprint
    {
        public float attackPower;          // 공격력
        public float maxHealth;            // 최대 체력
        public float moveSpeed;            // 이동 속도
        public float defense;              // 방어력
        public float healthRegen;          // 체력 회복량
        public float criticalChance;       // 치명타 확률 (0~1, 퍼센트 아님)
        public float criticalDamage;       // 치명타 데미지 배율 (예: 1.5배)
        public int extraProjectiles;       // 추가 발사체 수

        // 기본 생성자 (초기 기본값 지정)
        public CharacterStatBlueprint()
        {
            attackPower = 10f;
            maxHealth = 100f;
            moveSpeed = 5f;
            defense = 5f;
            healthRegen = 1f;
            criticalChance = 0f;      // 0%
            criticalDamage = 0f;      // 1배
            extraProjectiles = 0;
        }

        // 스탯 업그레이드 함수들
        public void UpgradeAttack(float amount)
        {
            attackPower += amount;
        }

        public void UpgradeHealth(float amount)
        {
            maxHealth += amount;
        }

        public void UpgradeMoveSpeed(float amount)
        {
            moveSpeed += amount;
        }

        public void UpgradeDefense(float amount)
        {
            defense += amount;
        }

        public void UpgradeHealthRegen(float amount)
        {
            healthRegen += amount;
        }

        public void UpgradeCriticalChance(float amount)
        {
            criticalChance = Mathf.Clamp01(criticalChance + amount); // 최대 1로 제한
        }

        public void UpgradeCriticalDamage(float amount)
        {
            criticalDamage += amount;
        }

        public void UpgradeExtraProjectiles(int amount)
        {
            extraProjectiles += amount;
        }

        public void CopyFrom(CharacterStatBlueprint other)
        {
            this.attackPower = other.attackPower;
            this.maxHealth = other.maxHealth;
            this.moveSpeed = other.moveSpeed;
            this.defense = other.defense;
            this.healthRegen = other.healthRegen;
            this.criticalChance = other.criticalChance;
            this.criticalDamage = other.criticalDamage;
            this.extraProjectiles = other.extraProjectiles;
        }

    }
}