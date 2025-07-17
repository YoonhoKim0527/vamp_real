using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class GunProjectile : Projectile
    {
        private bool piercingEnabled = false;
        private float piercingLifetime;
        private GameObject piercingEffectPrefab;
        private GameObject activePiercingEffect;

        public void EnablePiercing(float lifetime, GameObject effectPrefab)
        {
            piercingEnabled = true;
            piercingLifetime = lifetime;
            piercingEffectPrefab = effectPrefab;

            // ✅ Collider를 관통 모드로 전환
            if (piercingEnabled)
            {
                col.isTrigger = true; // 충돌 대신 트리거 처리
            }
        }

        protected override void HitDamageable(IDamageable damageable)
        {
            if (!piercingEnabled)
            {
                // ✅ 일반 모드: 적을 맞추면 파괴
                base.HitDamageable(damageable);
            }
            else
            {
                // ✅ 관통 모드: 적을 맞춰도 파괴하지 않고 데미지만 적용
                damageable.TakeDamage(damage, knockback * direction);
                OnHitDamageable.Invoke(damage);

                // ✅ 관통 이펙트 생성
                if (piercingEffectPrefab != null)
                {
                    GameObject effect = Instantiate(piercingEffectPrefab, transform.position, Quaternion.identity);
                    Destroy(effect, 0.3f); // 이펙트 짧게 유지
                }
            }
        }

        protected override void HitNothing()
        {
            if (!piercingEnabled)
            {
                base.HitNothing();
            }
            // ✅ 관통 모드에서는 HitNothing도 무시하고 이동 유지
        }

        public override void Launch(Vector2 direction)
        {
            base.Launch(direction);

            if (piercingEnabled)
            {
                // ✅ 일정 시간 후 파괴
                StartCoroutine(PiercingTimer());
            }
        }

        private IEnumerator PiercingTimer()
        {
            yield return new WaitForSeconds(piercingLifetime);
            DestroyProjectile();
        }

        protected override void OnTriggerEnter2D(Collider2D collider)
        {
            if (!piercingEnabled)
            {
                base.OnTriggerEnter2D(collider); // 일반 모드 충돌 처리
            }
            else
            {
                // ✅ 관통 모드에서는 충돌 시에도 멈추지 않음
                if ((targetLayer & (1 << collider.gameObject.layer)) != 0)
                {
                    if (collider.transform.parent.TryGetComponent<IDamageable>(out IDamageable damageable))
                    {
                        HitDamageable(damageable);
                    }
                }
            }
        }
    }
}
