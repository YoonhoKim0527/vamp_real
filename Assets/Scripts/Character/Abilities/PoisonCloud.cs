using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class PoisonCloud : MonoBehaviour
    {
        float damagePerTick;
        float radius;
        float duration;
        float tickInterval = 0.5f;

        LayerMask monsterLayer;
        Character owner;

        public void Init(float damage, float radius, float duration, LayerMask layer, Character owner)
        {
            this.damagePerTick = damage;
            this.radius = radius;
            this.duration = duration;
            this.monsterLayer = layer;
            this.owner = owner;

            transform.localScale = Vector3.one * radius * 0.2f;
            StartCoroutine(ApplyEffect());
        }

        IEnumerator ApplyEffect()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, monsterLayer);
                foreach (var hit in hits)
                {
                    IDamageable d = hit.GetComponentInParent<IDamageable>();
                    if (d != null)
                    {
                        d.TakeDamage(damagePerTick, Vector2.zero);
                        owner.OnDealDamage.Invoke(damagePerTick);
                    }
                }

                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;
            }

            Destroy(gameObject); // or gameObject.SetActive(false) if using pooling
        }
    }
}