using UnityEngine;

namespace Vampire
{
    public class ExpeditionPoisonZone : MonoBehaviour
    {
        [SerializeField] float duration = 4f;
        [SerializeField] float tickInterval = 0.5f;
        [SerializeField] float radius = 1.5f;

        float baseDamage;
        float timer;
        float tickTimer;
        Transform boss;

        public void Init(Transform bossTarget, float damage)
        {
            boss = bossTarget;
            baseDamage = damage;
        }

        void Update()
        {
            timer += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (timer >= duration)
            {
                Destroy(gameObject);
                return;
            }

            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                TryDealDamage();
            }
        }

        void TryDealDamage()
        {
            if (boss == null) return;

            float dist = Vector3.Distance(transform.position, boss.position);
            if (dist <= radius)
            {
                float damagePerTick = baseDamage * 0.1f; // 💥 틱 데미지 = 베이스 데미지 × 계수
                boss.GetComponent<ExpeditionBoss>()?.TakeDamage(damagePerTick);
            }
        }
    }
}
