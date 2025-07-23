using UnityEngine;

namespace Vampire
{
    public class TornadoProjectile : MonoBehaviour
    {
        float damage;
        Transform target;
        float speed = 3f;
        float rotateRadius = 1.5f;
        float angle = 0f;
        float angularSpeed = 360f; // degrees per second

        Vector3 centerOffset;

        public void Launch(Transform targetTransform, float dmg)
        {
            target = targetTransform;
            damage = dmg;
            centerOffset = targetTransform.position - transform.position;
        }

        void Update()
        {
            if (target == null) return;

            angle += angularSpeed * Time.deltaTime;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * rotateRadius;
            transform.position = Vector3.MoveTowards(transform.position, target.position + offset, speed * Time.deltaTime);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out ExpeditionBoss boss))
            {
                boss.TakeDamage(damage);
                Debug.Log("🌪️ Tornado hit!");
                Destroy(gameObject);
            }
        }
    }
}
