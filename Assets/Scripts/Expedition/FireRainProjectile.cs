using UnityEngine;

namespace Vampire
{
    public class FireRainProjectile : MonoBehaviour
    {
        [SerializeField] float speed = 10f;
        float damage;
        Vector3 direction;

        public void Launch(Vector3 dir, float dmg)
        {
            direction = dir.normalized;
            damage = dmg;
            Debug.Log($"🔥 FireRainProjectile launched! Direction: {direction}, Damage: {damage}");
        }

        void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out ExpeditionBoss boss))
            {
                Debug.Log($"💥 Fire hit boss: {boss.name}, damage={damage}");
                boss.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
