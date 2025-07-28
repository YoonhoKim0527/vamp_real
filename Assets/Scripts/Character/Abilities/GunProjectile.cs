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

            if (piercingEnabled)
            {
                col.isTrigger = true; // 충돌 대신 트리거 처리
            }
        }

        public override void Launch(Vector2 direction, bool isCritical = false) // 🟥 critical 플래그 추가
        {
            this.isCritical = isCritical; // 🟥 내부에 저장
            base.Launch(direction);

            if (piercingEnabled)
            {
                StartCoroutine(PiercingTimer());
            }
        }

        protected override void HitDamageable(IDamageable damageable)
        {
            if (!piercingEnabled)
            {
                base.HitDamageable(damageable); // 🟩 일반 모드 처리
            }
            else
            {
                // 🟥 critical 여부 전달
                damageable.TakeDamage(damage, knockback * direction, isCritical);
                OnHitDamageable.Invoke(damage);

                if (piercingEffectPrefab != null)
                {
                    GameObject effect = Instantiate(piercingEffectPrefab, transform.position, Quaternion.identity);
                    Destroy(effect, 0.3f);
                }
            }
        }

        protected override void HitNothing()
        {
            if (!piercingEnabled)
            {
                base.HitNothing();
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
                base.OnTriggerEnter2D(collider);
            }
            else
            {
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
