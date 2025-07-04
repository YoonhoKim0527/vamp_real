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
        protected Character playerCharacter;

        public void Init(float damage, float radius, float duration, LayerMask layer, Character owner, Character playerCharacter)
        {
            this.damagePerTick = damage;
            this.radius = radius;
            this.duration = duration;
            this.monsterLayer = layer;
            this.owner = owner;
            this.playerCharacter = playerCharacter;
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
                        float totalDamage = playerCharacter.Stats.GetTotalDamage() * damagePerTick;
                        d.TakeDamage(totalDamage, Vector2.zero);
                        owner.OnDealDamage.Invoke(totalDamage);
                    }
                }

                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;
            }

            Destroy(gameObject); // or gameObject.SetActive(false) if using pooling
        }
    }
}