using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class TailSpike : MonoBehaviour
    {
        [SerializeField] private float delayBeforeHit = 0.5f;
        [SerializeField] private float damageRadius = 1.5f;
        [SerializeField] private float damageAmount = 20f;
        [SerializeField] private LayerMask hitLayer;
        [SerializeField] private GameObject hitEffect; // optional effect prefab

        private bool hasHit = false;

        private void Start()
        {
            StartCoroutine(SpikeRoutine());
        }

        private IEnumerator SpikeRoutine()
        {
            // 경고 연출 시간
            yield return new WaitForSeconds(delayBeforeHit);

            if (!hasHit)
            {
                hasHit = true;
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius, hitLayer);
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent<IDamageable>(out var target))
                        target.TakeDamage(damageAmount, Vector2.zero);
                }

                // 이펙트 생성
                if (hitEffect != null)
                    Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, damageRadius);
        }
    }
}
