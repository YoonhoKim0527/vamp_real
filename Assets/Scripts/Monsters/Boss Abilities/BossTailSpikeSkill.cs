using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class BossTailSpikeSkill : BossAbility
    {
        [SerializeField] private GameObject tailSpikePrefab;
        [SerializeField] private float range = 6f;
        [SerializeField] private int spikeCount = 10;
        [SerializeField] private float spikeDelay = 0.4f;
        [SerializeField] private float cooldown = 8f;
        [SerializeField] private float spikeLifetime = 2f;
        [SerializeField] private Sprite attackSprite;

        private float nextAvailableTime = 0f;
        private Sprite originalSprite;
        private Vector3 originalScale;

        private SpriteRenderer spriteRenderer;
        private List<GameObject> spawnedSpikes = new();

        public override IEnumerator Activate()
        {
            if (Time.time < nextAvailableTime)
                yield break;

            nextAvailableTime = Time.time + cooldown;
            active = true;

            // ✅ 애니메이션 & 무적 시작
            if (spriteRenderer == null)
                spriteRenderer = monster.SpriteRenderer;

            if (spriteRenderer != null)
            {
                originalSprite = spriteRenderer.sprite;
                originalScale = spriteRenderer.transform.localScale;

                monster.Animator.StopAnimating(); // 애니메이션 중단
                spriteRenderer.sprite = attackSprite;
                spriteRenderer.transform.localScale = originalScale;
            }

            monster.SetInvincible(true); // ✅ 무적 적용
            monster.Stop();

            // ✅ 꼬리 생성
            for (int i = 0; i < spikeCount; i++)
            {
                if (spriteRenderer != null && attackSprite != null)
                    spriteRenderer.sprite = attackSprite;

                Vector2 randPos = monster.transform.position + (Vector3)Random.insideUnitCircle * range;
                GameObject spike = Instantiate(tailSpikePrefab, randPos, Quaternion.identity);
                spawnedSpikes.Add(spike);

                yield return new WaitForSeconds(spikeDelay);
            }

            // ✅ 꼬리 유지 시간 대기
            yield return new WaitForSeconds(spikeLifetime);

            // ✅ 꼬리 제거
            foreach (var spike in spawnedSpikes)
            {
                if (spike != null) Destroy(spike);
            }
            spawnedSpikes.Clear();

            // ✅ 애니메이션 복구 & 무적 해제
            if (spriteRenderer != null && originalSprite != null)
            {
                spriteRenderer.sprite = originalSprite;
                spriteRenderer.transform.localScale = originalScale;
            }

            monster.Animator.StartAnimating(true);
            monster.SetInvincible(false); // ✅ 무적 해제

            active = false;
        }

        public override float Score()
        {
            return Time.time >= nextAvailableTime ? 1f : 0f;
        }
    }
}
