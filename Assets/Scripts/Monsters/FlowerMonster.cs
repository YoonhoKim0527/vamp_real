using UnityEngine;

namespace Vampire
{
    public class FlowerMonster : Monster
    {
        protected override void FixedUpdate()
        {
            // ✅ 움직이지 않음
            rb.velocity = Vector2.zero;
        }

        protected override void Update()
        {
            // ✅ 부모 Update 호출 X (플레이어 추적 제거)
            // 대신 EntityManager 위치 갱신
            entityManager.Grid.UpdateClient(this);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                IDamageable player = other.GetComponentInParent<IDamageable>();
                if (player != null)
                {
                    player.TakeDamage(20, Vector2.zero); // ✅ Knockback 제거
                }
            }
        }
    }
}
