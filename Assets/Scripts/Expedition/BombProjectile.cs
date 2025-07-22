using UnityEngine;
using System.Collections;

namespace Vampire
{
    public class BombProjectile : MonoBehaviour
    {
        [SerializeField] float flightTime = 1f;
        [SerializeField] float explosionDelay = 2f;
        [SerializeField] float explosionRadius = 1.5f;

        float damage;
        Vector3 startPos;
        Vector3 targetPos;

        public void Launch(Vector3 target, float dmg)
        {
            damage = dmg;
            startPos = transform.position;
            targetPos = target;

            StartCoroutine(ParabolicMove());
        }

        IEnumerator ParabolicMove()
        {
            float t = 0;
            while (t < flightTime)
            {
                t += Time.deltaTime;
                float normalizedTime = t / flightTime;

                Vector3 linearPos = Vector3.Lerp(startPos, targetPos, normalizedTime);
                float height = 2f * Mathf.Sin(Mathf.PI * normalizedTime); // 포물선
                transform.position = linearPos + Vector3.up * height;

                yield return null;
            }

            // 착지 후 지연 폭발
            StartCoroutine(ExplodeAfterDelay());
        }

        IEnumerator ExplodeAfterDelay()
        {
            Debug.Log("💣 Bomb landed. Waiting to explode...");
            yield return new WaitForSeconds(explosionDelay);

            Debug.Log("💥 Bomb exploded!");
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out ExpeditionBoss boss))
                {
                    boss.TakeDamage(damage);
                    Debug.Log($"🔥 Boss took {damage} damage from bomb explosion");
                }
            }

            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
