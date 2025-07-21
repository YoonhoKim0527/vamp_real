using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class ExpeditionBoss : IDamageable
    {
        [SerializeField] SpriteRenderer bossSpriteRenderer;
        [SerializeField] SpriteAnimator bossAnimator;
        [SerializeField] BoxCollider2D bossCollider;
        [SerializeField] ParticleSystem deathParticles;

        float currentHp;
        float maxHp;
        ExpeditionManager expeditionManager;
        bool alive = true;

        public float HP => currentHp;
        public float MaxHP => maxHp;

        public void InitBoss(ExpeditionManager manager, ExpeditionBossBlueprint blueprint)
        {
            expeditionManager = manager;

            // HP 설정
            maxHp = blueprint.hp;
            currentHp = maxHp;

            // 숨쉬는 애니메이션 적용
            bossAnimator.Init(blueprint.breatheAnimation, blueprint.frameTime, true);
            bossAnimator.StartAnimating(true);

            // 위치 초기화
            transform.position = manager.BossSpawnPoint.position;

            // 콜라이더 크기 맞춤 (필요시)
            bossCollider.size = bossSpriteRenderer.bounds.size;

            alive = true;
        }

        public override void TakeDamage(float damage, Vector2 knockback, bool isCritical = false)
        {
            if (!alive) return;

            currentHp -= damage;
            expeditionManager.UpdateBossHP(currentHp, maxHp);

            if (currentHp <= 0)
                StartCoroutine(Killed());
        }

        public override void Knockback(Vector2 knockback)
        {
            // ExpeditionBoss는 넉백 없음 → 무시
        }

        IEnumerator Killed()
        {
            alive = false;

            if (deathParticles != null)
                deathParticles.Play();

            // 애니메이션 정지
            bossAnimator.StopAnimating();
            bossSpriteRenderer.enabled = false;

            yield return new WaitForSeconds(1f); // 죽는 연출 시간

            expeditionManager.OnBossDefeated();
            Destroy(gameObject);
        }
    }
}
