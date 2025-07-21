using UnityEngine;
using System.Collections;

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

        /// ✅ 플레이어 공격으로 죽었을 때 호출
        public override IEnumerator Killed(bool killedByPlayer = true)
        {
            alive = false;
            monsterHitbox.enabled = false;
            entityManager.LivingMonsters.Remove(this);

            if (deathParticles != null)
            {
                deathParticles.Play();
            }

            yield return HitAnimation();

            if (deathParticles != null)
            {
                monsterSpriteRenderer.enabled = false;
                shadow.SetActive(false);
                yield return new WaitForSeconds(deathParticles.main.duration - 0.15f);
                monsterSpriteRenderer.enabled = true;
                shadow.SetActive(true);
            }

            OnKilled.Invoke(this);
            OnKilled.RemoveAllListeners();

            // 🌸 Flower 전용 풀로 반환
            entityManager.DespawnFlowerMonster(this, killedByPlayer);
        }
    }
}
