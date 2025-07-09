using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class ShurikenAbility : ProjectileAbility
    {
        [Header("Shuriken Stats")]
        [SerializeField] protected UpgradeableProjectileCount projectileCount;
        [SerializeField] protected float shurikenDelay;

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
        protected override void Attack()
        {
            StartCoroutine(LuanchShurikens());
        }

        protected IEnumerator LuanchShurikens()
        {
            timeSinceLastAttack -= projectileCount.Value * shurikenDelay;
            for (int i = 0; i < projectileCount.Value; i++)
            {
                LaunchProjectile(playerCharacter.LookDirection);
                yield return new WaitForSeconds(shurikenDelay);
            }
        }

        protected void LaunchProjectile(Vector2 direction)
        {
            Debug.Log("shuriken");
            float totalDamage = playerCharacter.Stats.GetTotalDamage() * damage.Value;
            Projectile projectile = entityManager.SpawnProjectile(projectileIndex, playerCharacter.CenterTransform.position, totalDamage, knockback.Value, speed.Value, monsterLayer);
            projectile.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
            projectile.Launch(direction);
        }
    }
}

