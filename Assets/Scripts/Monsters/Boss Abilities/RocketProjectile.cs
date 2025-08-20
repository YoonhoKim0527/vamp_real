using UnityEngine;

namespace Vampire
{
    public class RocketProjectile : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField] private float arriveThreshold = 0.05f;
        [SerializeField] private float rotateTowardsVelocity = 1080;
        [Tooltip("로켓 스프라이트의 앞방향 기준 보정 각도 (0 = 오른쪽이 앞)")]
        [SerializeField] private float spriteForwardOffset = 0f; // 예: 위쪽이 앞이면 +90

        private Vector3 targetPos;
        private float speed;
        private float damage;
        private float explosionRadius;
        private LayerMask damageLayer;
        private GameObject explosionPrefab;

        public bool HasExploded { get; private set; }

        public void Init(
            Vector3 startPos,
            Vector3 targetPos,
            float speed,
            float damage,
            float explosionRadius,
            LayerMask damageLayer,
            GameObject explosionPrefab)
        {
            transform.position = startPos;
            this.targetPos = targetPos;
            this.speed = Mathf.Max(0.01f, speed);
            this.damage = damage;
            this.explosionRadius = Mathf.Max(0f, explosionRadius);
            this.damageLayer = damageLayer;
            this.explosionPrefab = explosionPrefab;

            // 초기 회전
            Vector2 dir = ((Vector2)(this.targetPos - transform.position)).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + spriteForwardOffset;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void Update()
        {
            if (HasExploded) return;

            Vector3 pos = transform.position;
            Vector3 toTarget = targetPos - pos;
            float dist = toTarget.magnitude;

            if (dist <= arriveThreshold)
            {
                transform.position = targetPos;
                Explode();
                return;
            }

            Vector3 dir = toTarget / dist;
            float step = speed * Time.deltaTime;

            if (step >= dist)
            {
                transform.position = targetPos;
                Explode();
                return;
            }
            else
            {
                transform.position = pos + dir * step;
            }

            // 진행 방향 회전
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + spriteForwardOffset;
            float newAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotateTowardsVelocity * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }

        private void Explode()
        {
            if (HasExploded) return;
            HasExploded = true;

            if (explosionPrefab != null)
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            if (explosionRadius > 0f)
            {
                var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageLayer);
                foreach (var h in hits)
                {
                    var dmg = h.GetComponentInParent<IDamageable>();
                    if (dmg != null)
                        dmg.TakeDamage(damage, Vector2.zero);
                }
            }

            Destroy(gameObject);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (HasExploded) return;
            Gizmos.color = new Color(1f, 0.25f, 0.1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
#endif
    }
}
