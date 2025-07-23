using UnityEngine;

namespace Vampire
{
    public class ExpeditionBananaProjectile : MonoBehaviour
    {
        [SerializeField] float speed = 6f;
        [SerializeField] float flightTime = 1.2f; // 왕복 한쪽 시간
        [SerializeField] float tickInterval = 0.3f;
        [SerializeField] float totalLifetime = 2.5f;
        [SerializeField] float rotationSpeed = 720f; // 도/초

        float elapsedTime;
        float tickTimer;
        bool returning = false;

        Transform targetBoss;
        Transform caster;
        float damage;

        public void Init(Transform boss, Transform casterTransform, float baseDamage)
        {
            targetBoss = boss;
            caster = casterTransform;
            damage = baseDamage;
        }

        void Update()
        {
            elapsedTime += Time.deltaTime;
            tickTimer += Time.deltaTime;

            // 회전
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            // 방향 계산
            Vector3 targetPos = returning ? caster.position : targetBoss.position;
            Vector3 dir = (targetPos - transform.position).normalized;

            transform.position += dir * speed * Time.deltaTime;

            // 틱 데미지
            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                TryDealDamage();
            }

            // 되돌아오기 시작
            if (!returning && elapsedTime >= flightTime)
                returning = true;

            if (elapsedTime >= totalLifetime)
                Destroy(gameObject);
        }

        void TryDealDamage()
        {
            if (targetBoss == null) return;

            float dist = Vector3.Distance(transform.position, targetBoss.position);
            if (dist < 1.5f && !returning)
            {
                targetBoss.GetComponent<ExpeditionBoss>()?.TakeDamage(damage * tickInterval);
            }
        }
    }
}
