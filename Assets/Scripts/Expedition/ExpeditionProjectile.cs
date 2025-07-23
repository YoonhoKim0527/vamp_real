using UnityEngine;

namespace Vampire
{
    public class ExpeditionProjectile : MonoBehaviour
    {
        [SerializeField] float speed = 10f;
        float damage;
        Vector3 direction;

        public void Launch(Vector3 dir, float dmg)
        {
            direction = dir.normalized;
            damage = dmg;
            Debug.Log($"🎯 Projectile launched! Direction: {direction}, Damage: {damage}");
        }

        void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
            Debug.Log($"🟡 Moving projectile: pos={transform.position}, dir={direction}");
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"🧨 OnTriggerEnter2D with {other.gameObject.name}");
            if (other.TryGetComponent(out ExpeditionBoss boss))
            {
                Debug.Log($"💥 Hit boss: {boss.name}, damage={damage}");
                boss.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
