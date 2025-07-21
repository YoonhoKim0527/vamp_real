using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class MachineGunAbility : GunAbility
    {
        [Header("Machine Gun Stats")]
        [SerializeField] protected GameObject machineGun;
        [SerializeField] protected Transform launchTransform;
        [SerializeField] protected UpgradeableRotationSpeed rotationSpeed;
        [SerializeField] protected float gunRadius;
        protected Vector3 gunDirection = Vector2.right;

        protected override void Update()
        {
            base.Update();

            // ✅ 재장전 중일 때 총 회전
            float reloadRotation = 0;
            float t = timeSinceLastAttack / cooldown.Value;
            if (t > 0 && t < 1)
            {
                reloadRotation = t * 360;
            }

            // ✅ 총기 회전 처리
            float theta = Time.time * rotationSpeed.Value;
            gunDirection = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0);
            machineGun.transform.position = playerCharacter.CenterTransform.position + gunDirection * gunRadius;
            machineGun.transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * theta - reloadRotation);
        }

        protected override void LaunchProjectile()
        {
            // ✅ CharacterStatBlueprint 기반 데미지/넉백 계산
            float totalDamage = playerStats.attackPower * damage.Value;

            // ✅ 치명타 확률 적용
            if (Random.value < playerStats.criticalChance)
            {
                totalDamage *= (1 + playerStats.criticalDamage);
                Debug.Log("💥 [MachineGunAbility] Critical hit!");
            }

            float effectiveKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

            // ✅ 발사체 생성
            Projectile projectile = entityManager.SpawnProjectile(
                projectileIndex,
                launchTransform.position,
                totalDamage,
                effectiveKnockback,
                speed.Value,
                monsterLayer
            );
            projectile.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
            projectile.Launch(gunDirection);
        }
    }
}
