using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class GunAbility : ProjectileAbility
    {
        [Header("Gun Stats")]
        [SerializeField] protected UpgradeableProjectileCount projectileCount;
        [SerializeField] protected UpgradeableDamageRate firerate;

        [Header("Evolved Settings")]
        [SerializeField] private float evolvedSpeedMultiplier = 2f; // ✅ 진화 시 속도 배수
        [SerializeField] private float evolvedLifetime = 3f;        // ✅ 관통 지속 시간
        [SerializeField] private GameObject piercingEffectPrefab;   // ✅ 관통 시 이펙트

        protected override void Use()
        {
            base.Use();

            // ✅ CrossSceneData 대신 CharacterStatBlueprint를 사용
            if (playerStats.extraProjectiles > 0 && projectileCount != null)
            {
                projectileCount.ForceAdd(playerStats.extraProjectiles);
                Debug.Log($"[GunAbility] Added {playerStats.extraProjectiles} extra projectiles from PlayerStats.");
            }
        }

        protected override void Attack()
        {
            StartCoroutine(FireClip());
        }

        private IEnumerator FireClip()
        {
            int clipSize = projectileCount.Value;
            timeSinceLastAttack -= clipSize / firerate.Value;

            for (int i = 0; i < clipSize; i++)
            {
                LaunchProjectile();
                yield return new WaitForSeconds(1 / firerate.Value);
            }
        }

        protected override void LaunchProjectile()
        {
            float totalDamage = playerStats.attackPower * damage.Value;

            // ✅ 치명타 확률 적용
            bool isCritical = Random.value < playerStats.criticalChance;
            if (isCritical)
            {
                totalDamage *= (1 + playerStats.criticalDamage);
            }

            float effectiveKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

            Projectile projectile = entityManager.SpawnProjectile(
                projectileIndex,
                playerCharacter.CenterTransform.position,
                totalDamage,
                effectiveKnockback,
                level >= 1 ? speed.Value * evolvedSpeedMultiplier : speed.Value,
                monsterLayer
            );

            // 🟥 critical 정보 넘기기
            projectile.Launch(playerCharacter.LookDirection, isCritical);

            if (level >= 1 && projectile is GunProjectile gunProjectile)
            {
                gunProjectile.EnablePiercing(evolvedLifetime, piercingEffectPrefab);
                Debug.Log("🔫 [GunAbility] Piercing mode enabled.");
            }

            projectile.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
        }
    }
}
