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

            if (CrossSceneData.ExtraProjectile && boomerangCount != null)
            {
                boomerangCount.ForceAdd(1);  
            }
            if (CrossSceneData.BonusProjectile > 0 && boomerangCount != null)
            {
                boomerangCount.ForceAdd(CrossSceneData.BonusProjectile);  
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
            Boomerang boomerang = entityManager.SpawnBoomerang(
                boomerangIndex,
                playerCharacter.CenterTransform.position,
                damage.Value,
                knockback.Value,
                throwRadius,
                throwTime,
                monsterLayer
            );

            Vector2 throwPosition;
            // Throw randomly at nearby enemies
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
            Boomerang boomerang = entityManager.SpawnBoomerang(
                boomerangIndex,
                playerCharacter.CenterTransform.position,
                damage.Value,
                knockback.Value,
                throwRadius * awakenedThrowRadiusMultiplier,
                throwTime,
                monsterLayer
            );

            // ✅ 부메랑 크기 초기화 후 4배로 고정
            boomerang.transform.localScale = boomerangPrefab.transform.localScale * awakenedSizeMultiplier;

            // ✅ 무작위 방향
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
