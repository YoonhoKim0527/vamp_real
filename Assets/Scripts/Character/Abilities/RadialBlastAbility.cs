using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class RadialBlastAbility : ProjectileAbility
    {
        [Header("Radial Blast Stats")]
        [SerializeField] private Sprite effectSprite;
        [SerializeField] private UpgradeableFloat cooldown;
        [SerializeField] private UpgradeableProjectileCount projectileCount;

        private float timeSinceLastCast;

        protected override void Use()
        {
            base.Use();
            if (projectileIndex == -1 && projectilePrefab != null)
            {
                projectileIndex = entityManager.AddPoolForProjectile(projectilePrefab);
            }
            timeSinceLastCast = cooldown.Value;
        }

        protected override void Update()
        {
            base.Update();
            timeSinceLastCast += Time.deltaTime;
            if (timeSinceLastCast >= cooldown.Value)
            {
                Attack();
                timeSinceLastCast = 0f;
            }
        }

        protected override void Attack()
        {
            StartCoroutine(CastRadialBlast());
        }

        private IEnumerator CastRadialBlast()
        {
            // 1. 이펙트 이미지 표시
            GameObject effect = new GameObject("RadialBlastEffect");
            SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
            sr.sprite = effectSprite;
            sr.sortingOrder = 1000;
            effect.transform.position = playerCharacter.CenterTransform.position;
            effect.transform.localScale = Vector3.one * 4f;

            yield return new WaitForSeconds(0.5f);
            Destroy(effect);

            // 2. 12방향 투사체 발사
            for (int i = 0; i < projectileCount.Value; i++)
            {
                float angle = 360f / projectileCount.Value * i;
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
                LaunchProjectile(dir);
            }
        }

        // Shuriken처럼 direction 기반으로 launch
        protected void LaunchProjectile(Vector2 direction)
        {
            Projectile projectile = entityManager.SpawnProjectile(
                projectileIndex,
                playerCharacter.CenterTransform.position,
                damage.Value,
                knockback.Value,
                speed.Value,
                monsterLayer
            );

            if (projectile == null)
            {
                Debug.LogError("[RadialBlast] Spawned projectile is NULL!");
                return;
            }

            projectile.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
            projectile.Launch(direction);
        }
    }   
}
