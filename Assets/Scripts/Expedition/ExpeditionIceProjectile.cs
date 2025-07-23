using UnityEngine;

namespace Vampire
{
    public class ExpeditionIceProjectile : MonoBehaviour
    {
        float speed = 10f;
        float damage;
        float freezeDuration = 3f;
        Transform target;

        public void Init(Vector3 startPosition, Transform boss, float dmg)
        {
            transform.position = startPosition;
            target = boss;
            damage = dmg;
        }

        void Update()
        {
            if (target == null) return;

            Vector3 dir = (target.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out ExpeditionBoss boss))
            {
                boss.Freeze(freezeDuration);
                boss.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
