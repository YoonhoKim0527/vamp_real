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
            FireSingleProjectile(
                playerCharacter.CenterTransform.position,
                playerCharacter.LookDirection,
                playerStats.attackPower,
                1f
            );
        }

        private void FireSingleProjectile(Vector3 spawnPos, Vector2 direction, float baseAttackPower, float damageMultiplier, bool isGhost = false, Color? ghostColor = null)
        {
            float totalDamage = baseAttackPower * damage.Value * damageMultiplier;

            bool isCritical = Random.value < playerStats.criticalChance;
            if (isCritical)
            {
                totalDamage *= (1 + playerStats.criticalDamage);
            }

            float effectiveKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

            Projectile projectile = entityManager.SpawnProjectile(
                projectileIndex,
                spawnPos,
                totalDamage,
                effectiveKnockback,
                level >= 1 ? speed.Value * evolvedSpeedMultiplier : speed.Value,
                monsterLayer
            );

            projectile.Launch(direction, isCritical);

            if (isGhost && ghostColor.HasValue)
            {
                var sr = projectile.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = ghostColor.Value;
                }
            }

            if (level >= 1 && projectile is GunProjectile gunProjectile)
            {
                gunProjectile.EnablePiercing(evolvedLifetime, piercingEffectPrefab);
            }

            projectile.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
        }

        public override void MirrorActivate(float damageMultiplier, Vector3 spawnPosition, Color ghostColor)
        {
            StartCoroutine(FireMirrorClip(damageMultiplier, spawnPosition, ghostColor));
        }

        private IEnumerator FireMirrorClip(float damageMultiplier, Vector3 spawnPosition, Color ghostColor)
        {
            int clipSize = projectileCount.Value;

            for (int i = 0; i < clipSize; i++)
            {
                FireSingleProjectile(
                    spawnPosition,
                    playerCharacter.LookDirection,
                    playerStats.attackPower,
                    damageMultiplier,
                    true,         // isGhost
                    ghostColor    // <- 새로운 파라미터
                );

                yield return new WaitForSeconds(1 / firerate.Value);
            }
        }
    }
}
