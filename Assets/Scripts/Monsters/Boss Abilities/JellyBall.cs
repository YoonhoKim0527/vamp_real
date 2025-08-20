using UnityEngine;

namespace Vampire
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class JellyBall : MonoBehaviour
    {
        // 이동
        private Vector2 moveDir;
        private float moveSpeed;
        private float life;

        // 히트
        private float damage;
        [SerializeField] private float stunSeconds = 3f;
        [SerializeField] private Color hitTint = Color.green;

        private bool initialized;

        /// <summary>
        /// Trigger 전용. 플레이어에 Rigidbody2D가 있으므로 JellyBall에는 RB 불필요.
        /// </summary>
        public void Init(Vector2 dir, float speed, float lifetime, float damage)
        {
            moveDir = dir.normalized;
            moveSpeed = speed;
            life = lifetime;
            this.damage = damage;

            initialized = true;
            Destroy(gameObject, life);
        }

        private void Awake()
        {
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
        }

        private void Update()
        {
            if (!initialized) return;
            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 플레이어만 처리 (레이어 마스크 불일치로 인한 미적용 방지)
            if (!other.CompareTag("Player")) return;

            // 1) 데미지: Rocket과 동일한 경로(IDamageable)
            var dmg = other.GetComponentInParent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(damage, Vector2.zero);

            // 2) 상태이상/색상
            var ch = other.GetComponentInParent<Character>();
            if (ch != null)
            {
                ch.Stun(stunSeconds);
                ch.SetTemporaryColor(hitTint, stunSeconds);
            }

            Destroy(gameObject);
        }
    }
}
