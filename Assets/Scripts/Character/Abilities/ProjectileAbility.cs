using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class ProjectileAbility : Ability
    {
        [Header("Projectile Stats")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected LayerMask monsterLayer;
        [SerializeField] protected UpgradeableDamage damage;
        [SerializeField] protected UpgradeableProjectileSpeed speed;
        [SerializeField] protected UpgradeableKnockback knockback;
        [SerializeField] protected UpgradeableWeaponCooldown cooldown;

        protected float timeSinceLastAttack;
        protected int projectileIndex;

        protected override void Use()
        {
            base.Use();
            gameObject.SetActive(true);
            timeSinceLastAttack = cooldown.Value;

            // ✅ Projectile 풀링 등록
            projectileIndex = entityManager.AddPoolForProjectile(projectilePrefab);
        }

        protected virtual void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            if (timeSinceLastAttack >= cooldown.Value)
            {
                timeSinceLastAttack = Mathf.Repeat(timeSinceLastAttack, cooldown.Value);
                Attack();
            }
        }

        protected virtual void Attack()
        {
            LaunchProjectile();
        }

        protected virtual void LaunchProjectile()
        {
            // ✅ CharacterStatBlueprint 기반 데미지 계산
            float totalDamage = playerStats.attackPower * damage.Value;

            // ✅ 치명타 확률 판정
            bool isCritical = false;
            if (Random.value < playerStats.criticalChance)
            {
                totalDamage *= (1 + playerStats.criticalDamage);
                isCritical = true;
                Debug.Log("🎯 [ProjectileAbility] Critical hit!");
            }

            // ✅ 넉백에 defense 보정
            float effectiveKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

            // ✅ 발사체 생성 및 발사
            Projectile projectile = entityManager.SpawnProjectile(
                projectileIndex,
                playerCharacter.CenterTransform.position,
                totalDamage,
                effectiveKnockback,
                speed.Value,
                monsterLayer
            );

            projectile.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);

            // 🟥 isCritical 정보 전달
            projectile.Launch(playerCharacter.LookDirection, isCritical);
        }
    }
}
