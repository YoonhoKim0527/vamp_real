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

        protected override void Attack()
        {
            StartCoroutine(FireClip());
        }

        protected override void Use()
        {
            base.Use();

            if (CrossSceneData.ExtraProjectile && projectileCount != null)
            {
                projectileCount.ForceAdd(1);
            }
            if (CrossSceneData.BonusProjectile > 0 && projectileCount != null)
            {
                projectileCount.ForceAdd(CrossSceneData.BonusProjectile);
            }
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
            float totalDamage = playerCharacter.Stats.GetTotalDamage() * damage.Value;

            Projectile projectile = entityManager.SpawnProjectile(
                projectileIndex,
                playerCharacter.CenterTransform.position,
                totalDamage,
                knockback.Value,
                level >= 0 ? speed.Value * evolvedSpeedMultiplier : speed.Value, // ✅ 진화 시 속도 업
                monsterLayer
            );

            // ✅ 레벨 1 이상이면 관통 모드 활성화
            if (level >= 0 && projectile is GunProjectile gunProjectile)
            {
                gunProjectile.EnablePiercing(evolvedLifetime, piercingEffectPrefab);
            }

            projectile.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
            projectile.Launch(playerCharacter.LookDirection);
        }
    }
}
