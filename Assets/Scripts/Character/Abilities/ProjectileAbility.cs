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
            projectileIndex = entityManager.AddPoolForProjectile(projectilePrefab);
        }

        protected virtual void Update()
        {
            if (target != null)
            {
                Vector3 dir = (target.position - transform.position).normalized;
                transform.position += dir * speed.Value * Time.deltaTime;
            }
            
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
            float totalDamage = playerCharacter.Stats.GetTotalDamage() * damage.Value;
            Projectile projectile = entityManager.SpawnProjectile(projectileIndex, playerCharacter.CenterTransform.position, totalDamage, knockback.Value, speed.Value, monsterLayer);
            projectile.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
            projectile.Launch(playerCharacter.LookDirection);
        }
    }
}
