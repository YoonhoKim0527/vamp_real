using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class ShurikenAbility : Ability
    {
        [Header("Shuriken Stats")]
        [SerializeField] private GameObject shurikenPrefab;
        [SerializeField] private LayerMask monsterLayer;
        [SerializeField] private float throwRadius = 5f;
        [SerializeField] private float throwTime = 0.15f;
        [SerializeField] private UpgradeableDamage damage;
        [SerializeField] private UpgradeableKnockback knockback;
        [SerializeField] private UpgradeableWeaponCooldown cooldown;
        [SerializeField] private float chainRange = 2f; // 연쇄 범위

        private int shurikenIndex;
        private float timeSinceLastAttack;
        private bool isShurikenActive = false; // ✅ 현재 슈리켄 운용 여부

        protected override void Use()
        {
            base.Use();
            gameObject.SetActive(true);
            timeSinceLastAttack = cooldown.Value;

            shurikenIndex = entityManager.AddPoolForProjectile(shurikenPrefab);
        }

        void Update()
        {
            timeSinceLastAttack += Time.deltaTime;

            if (!isShurikenActive && timeSinceLastAttack >= cooldown.Value)
            {
                timeSinceLastAttack = 0f;
                LaunchShuriken();
            }
        }

        private void LaunchShuriken()
        {
            Debug.Log("[ShurikenAbility] 슈리켄 발사");

            // ✅ CharacterStatBlueprint 기반 데미지/넉백 계산
            float totalDamage = playerStats.attackPower * damage.Value;
            Debug.Log($"total: {totalDamage}, attackpower: {playerStats.attackPower}, damge: {damage.Value}");

            // ✅ 치명타 판정
            bool isCritical = false;
            if (Random.value < playerStats.criticalChance)
            {
                totalDamage *= (1 + playerStats.criticalDamage);
                isCritical = true;
            }

            float effectiveKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

            ShurikenProjectile shuriken = entityManager.SpawnProjectile(
                shurikenIndex,
                playerCharacter.CenterTransform.position,
                totalDamage,
                effectiveKnockback,
                throwRadius / throwTime,
                monsterLayer
            ).GetComponent<ShurikenProjectile>();

            if (shuriken != null)
            {
                isShurikenActive = true; // ✅ 운용 중 표시
                shuriken.Init(playerCharacter, playerStats, throwRadius, throwTime, chainRange, OnShurikenReturn, isCritical);
                shuriken.StartAttackSequence();
            }
            else
            {
                Debug.LogError("[ShurikenAbility] SpawnProjectile 실패");
            }
        }

        // ✅ 슈리켄이 완전히 돌아왔을 때 호출
        private void OnShurikenReturn()
        {
            Debug.Log("[ShurikenAbility] 슈리켄 귀환 완료");
            isShurikenActive = false;
        }
    }
}
