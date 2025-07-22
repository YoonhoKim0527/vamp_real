using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class BoomerangAbility : Ability
    {
        [Header("Boomerang Stats")]
        [SerializeField] protected GameObject boomerangPrefab;
        [SerializeField] protected LayerMask monsterLayer;
        [SerializeField] protected float throwRadius;
        [SerializeField] protected float throwTime = 1;
        [SerializeField] protected UpgradeableDamageRate throwRate;
        [SerializeField] protected UpgradeableDamage damage;
        [SerializeField] protected UpgradeableKnockback knockback;
        [SerializeField] protected UpgradeableWeaponCooldown cooldown;
        [SerializeField] protected UpgradeableProjectileCount boomerangCount;

        [Header("Awakening Settings")]
        [SerializeField] private bool isAwakened = false;
        [SerializeField] private int awakeningLevel = 1;           // ✅ 각성 조건 레벨
        [SerializeField] private float awakenedSizeMultiplier = 4f;
        [SerializeField] private float awakenedThrowRadiusMultiplier = 3f;
        [SerializeField] private int awakenedBoomerangCount = 8;

        protected float timeSinceLastAttack;
        protected int boomerangIndex;

        protected override void Use()
        {
            base.Use();

            // ✅ CharacterStatBlueprint의 extraProjectiles 반영
            if (playerStats.extraProjectiles > 0 && boomerangCount != null)
            {
                boomerangCount.ForceAdd(playerStats.extraProjectiles);
            }

            gameObject.SetActive(true);
            timeSinceLastAttack = cooldown.Value;

            boomerangIndex = entityManager.AddPoolForBoomerang(boomerangPrefab);

            // ✅ 각성 체크
            CheckAwakening();
        }

        void Update()
        {
            CheckAwakening();
            timeSinceLastAttack += Time.deltaTime;
            if (timeSinceLastAttack >= cooldown.Value)
            {
                timeSinceLastAttack = Mathf.Repeat(timeSinceLastAttack, cooldown.Value);
                StartCoroutine(Attack());
            }
        }

        protected virtual IEnumerator Attack()
        {
            if (isAwakened)
            {
                // ✅ 각성 시 한 번에 8개 부메랑 발사
                for (int i = 0; i < awakenedBoomerangCount; i++)
                {
                    ThrowAwakenedBoomerang();
                }
                yield break; // ✅ 한 번에 다 던졌으니 끝
            }

            // 기존 공격
            timeSinceLastAttack -= boomerangCount.Value / throwRate.Value;
            for (int i = 0; i < boomerangCount.Value; i++)
            {
                ThrowBoomerang();
                yield return new WaitForSeconds(1 / throwRate.Value);
            }
        }

        protected virtual void ThrowBoomerang()
        {
            float totalDamage = playerStats.attackPower * damage.Value;
            float totalKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

            bool isCritical = false;

            // ✅ 치명타 확률 적용
            if (Random.value < playerStats.criticalChance)
            {
                totalDamage *= (1 + playerStats.criticalDamage);
                isCritical = true;
                Debug.Log("🪃 BoomerangAbility: Critical Hit!");
            }

            Boomerang boomerang = entityManager.SpawnBoomerang(
                boomerangIndex,
                playerCharacter.CenterTransform.position,
                totalDamage,
                totalKnockback,
                throwRadius,
                throwTime,
                monsterLayer
            );

            // ✅ Boomerang에 크리티컬 여부 전달
            boomerang.InitCritical(isCritical);

            Vector2 throwPosition;
            List<ISpatialHashGridClient> nearbyEnemies = entityManager.Grid.FindNearbyInRadius(playerCharacter.transform.position, throwRadius);
            if (nearbyEnemies.Count > 0)
                throwPosition = nearbyEnemies[Random.Range(0, nearbyEnemies.Count)].Position;
            else
                throwPosition = (Vector2)playerCharacter.transform.position + Random.insideUnitCircle.normalized * throwRadius;

            boomerang.Throw(playerCharacter.transform, throwPosition);
            boomerang.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
        }

        private void ThrowAwakenedBoomerang()
        {
            float totalDamage = playerStats.attackPower * damage.Value;
            float totalKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

            bool isCritical = false;

            // ✅ 치명타 확률 적용
            if (Random.value < playerStats.criticalChance)
            {
                totalDamage *= (1 + playerStats.criticalDamage);
                isCritical = true;
                Debug.Log("🪃 BoomerangAbility: Critical Hit (Awakened)!");
            }

            Boomerang boomerang = entityManager.SpawnBoomerang(
                boomerangIndex,
                playerCharacter.CenterTransform.position,
                totalDamage,
                totalKnockback,
                throwRadius * awakenedThrowRadiusMultiplier,
                throwTime,
                monsterLayer
            );

            boomerang.transform.localScale = boomerangPrefab.transform.localScale * awakenedSizeMultiplier;

            // ✅ Boomerang에 크리티컬 여부 전달
            boomerang.InitCritical(isCritical);

            Vector2 throwDirection = Random.insideUnitCircle.normalized;
            Vector2 throwPosition = (Vector2)playerCharacter.transform.position + throwDirection * (throwRadius * awakenedThrowRadiusMultiplier);

            boomerang.Throw(playerCharacter.transform, throwPosition);
            boomerang.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
        }

        private void CheckAwakening()
        {
            if (!isAwakened && level > awakeningLevel)
            {
                isAwakened = true;
                Debug.Log("🪃 BoomerangAbility: Awakened skill activated!");
            }
        }
    }
}
