using UnityEngine;

namespace Vampire
{
    public class GloveProjectile : MonoBehaviour
    {
        Vector3 startPos;
        Vector3 targetPos;
        float speed;
        float damage;
        bool returning = false;

        public void Launch(Vector3 from, Vector3 to, float spd, float dmg)
        {
            startPos = from;
            targetPos = to;
            speed = spd;
            damage = dmg;

            transform.position = from;

            // ✅ 방향 반전 처리 (SpriteRenderer 방식)
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.flipX = targetPos.x < from.x;

            // ✅ 또는 localScale.x 방식
            /*
            Vector3 scale = transform.localScale;
            scale.x = (targetPos.x < from.x) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
            */
        }

        void Update()
        {
            if (!returning)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPos) < 0.1f)
                    returning = true;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, startPos, speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, startPos) < 0.1f)
                    Destroy(gameObject);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!returning && other.TryGetComponent(out ExpeditionBoss boss))
            {
                boss.TakeDamage(damage);
                returning = true;
            }
        }
    }
}
