using UnityEngine;
using System.Collections;

namespace Vampire
{
    public class SwarmMonster : Monster
    {
        private Vector2 moveDirection;
        private float moveSpeed;

        public void InitSwarm(Vector2 spawnPosition, Vector2 targetDirection, float speed)
        {
            transform.position = spawnPosition;
            moveDirection = targetDirection.normalized;
            moveSpeed = speed;
            rb.velocity = moveDirection * moveSpeed;
        }

        protected override void FixedUpdate()
        {
            rb.velocity = moveDirection * moveSpeed;
        }

        void Update()
        {
            // 부모 Update 호출하지 않음: 플레이어 추적 제거
            transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);

            // 화면 밖이면 제거
            if (!IsVisibleToCamera())
            {
                // 🦇 SwarmMonster는 일반 풀 X → Swarm 전용 풀로 반환
                entityManager.DespawnSwarmMonster(this, false);
            }
        }

        private bool IsVisibleToCamera()
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
            return viewportPos.x >= -0.1f && viewportPos.x <= 1.1f &&
                   viewportPos.y >= -0.1f && viewportPos.y <= 1.1f;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                IDamageable player = other.GetComponentInParent<IDamageable>();
                if (player != null)
                {
                    Vector2 knockbackDir = (player.transform.position - transform.position).normalized;
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

            // 🦇 Swarm 전용 풀로 반환
            entityManager.DespawnSwarmMonster(this, killedByPlayer);
        }
    }
}
