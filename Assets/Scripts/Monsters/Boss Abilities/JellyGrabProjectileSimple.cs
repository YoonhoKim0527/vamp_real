using UnityEngine;

namespace Vampire
{
    /// <summary>
    /// Kinematic 직진 + Trigger 충돌로 캡처.
    /// 프리팹에 Rigidbody2D(BodyType=Kinematic), Collider2D(isTrigger=true) 필요.
    /// </summary>
    public class JellyGrabProjectileSimple : MonoBehaviour
    {
        private JellyDevourSkill owner;
        private Vector2 dir;
        private float speed;
        private float lifetime;
        private LayerMask hitLayer;

        private float t0;
        private Rigidbody2D rb;

        public void Init(JellyDevourSkill owner, Vector2 direction, float speed, float lifetime, LayerMask hitLayer)
        {
            this.owner = owner;
            this.dir = direction.normalized;
            this.speed = speed;
            this.lifetime = Mathf.Max(0.05f, lifetime);
            this.hitLayer = hitLayer;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            t0 = Time.time;
        }

        private void FixedUpdate()
        {
            // 이동
            Vector2 step = dir * speed * Time.fixedDeltaTime;
            if (rb != null) rb.MovePosition(rb.position + step);
            else            transform.position += (Vector3)step;

            // 터널링 방지 근접 검사
            float radius = 0.28f;
            var hit = Physics2D.OverlapCircle(transform.position, radius, hitLayer);
            if (hit != null) TryCapture(hit);

            // TTL
            if (Time.time - t0 >= lifetime)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & hitLayer) == 0) return;
            TryCapture(other);
        }

        private void TryCapture(Component hit)
        {
            if (owner == null) { Destroy(gameObject); return; }

            var ch = hit.GetComponentInParent<Character>();
            if (ch != null)
            {
                owner.BeginCapture(ch);
                Destroy(gameObject);
            }
        }
    }
}
