using UnityEngine;

namespace Vampire
{
    public class FrogThrowable : Throwable
    {
        Transform playerTransform;

        void Start()
        {
            playerTransform = playerCharacter?.transform;
        }

        void Update()
        {
            if (playerTransform == null) return;

            float yDistance = playerTransform.position.y - transform.position.y;
            if (yDistance >= 10f)
            {
                DestroyThrowable(); // 풀에서 제거
            }
        }

        protected override void Explode()
        {
            Debug.Log("🐸 Frog exploded!");
            DestroyThrowable();
        }

        protected void OnTriggerEnter2D(Collider2D other)
        {
            if ((targetLayer & (1 << other.gameObject.layer)) != 0)
            {
                if (other.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage, knockback * Vector2.down);
                    OnHitDamageable.Invoke(damage);
                }

                Explode();
            }
            else if (other.gameObject.CompareTag("Ground"))
            {
                Explode();
            }
        }
    }
}
