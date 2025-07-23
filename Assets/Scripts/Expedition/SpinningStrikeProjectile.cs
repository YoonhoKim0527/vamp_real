using UnityEngine;

namespace Vampire
{
    public class SpinningStrikeProjectile : MonoBehaviour
    {
        Transform boss;
        float radius;
        float duration;
        float damage;

        float angle = 0f;
        float spinTimer = 0f;
        bool chasing = false;
        Vector3 center;
        float speed = 1f;

        public void Launch(Vector3 origin, Transform bossTarget, float spinRadius, float spinDuration, float dmg)
        {
            center = origin;
            boss = bossTarget;
            radius = spinRadius;
            duration = spinDuration;
            damage = dmg;
        }

        void Update()
        {
            if (!chasing)
            {
                spinTimer += Time.deltaTime;
                angle -= 360f * Time.deltaTime;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
                transform.position = center + offset;

                if (spinTimer >= duration)
                {
                    chasing = true;
                }
            }
            else
            {
                if (boss == null) return;

                Vector3 dir = (boss.position - transform.position).normalized;
                transform.position = Vector3.Lerp(transform.position, boss.position, Time.deltaTime * speed);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out ExpeditionBoss bossHit))
            {
                bossHit.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
