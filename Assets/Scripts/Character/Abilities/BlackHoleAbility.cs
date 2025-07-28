using UnityEngine;

namespace Vampire
{
    public class BlackHoleAbility : ProjectileAbility
    {
        [SerializeField] protected float explosionDelay = 2f;
        [SerializeField] protected float pullForce = 5f;
        [SerializeField] protected float blackHoleRadius = 3f;
        [SerializeField] protected float travelDistance = 3f;
        [SerializeField] protected ParticleSystem blackHoleEffect;
        protected override void Attack()
        {
            float totalDamage = playerStats.attackPower * damage.Value;
            bool isCritical = Random.value < playerStats.criticalChance;
            if (isCritical) totalDamage *= (1 + playerStats.criticalDamage);

            float effectiveKnockback = 0f;

            var obj = entityManager.SpawnProjectile(
                projectileIndex,
                playerCharacter.CenterTransform.position,
                totalDamage,
                effectiveKnockback,
                speed.Value,
                monsterLayer
            );

            var proj = obj.GetComponent<BlackHoleProjectile>();
            if (proj != null)
            {
                proj.Init(
                    playerCharacter.LookDirection,
                    playerCharacter,
                    monsterLayer,
                    isCritical,
                    explosionDelay,
                    pullForce,
                    blackHoleRadius,
                    effectiveKnockback,
                    blackHoleEffect,
                    totalDamage,
                    travelDistance // ✅ 추가
                );
            }
        }

    }
}
