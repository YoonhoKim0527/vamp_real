using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class BossPullSkill : BossAbility
    {
        [Header("Pull Settings")]
        public float pullDuration = 2f;
        public float pullStrength = 15f;
        public float cooldown = 10f;

        [Header("Sprite")]
        [SerializeField] private Sprite pullSprite;
        private SpriteRenderer spriteRenderer;
        private Sprite originalSprite;
        private Animator animator;

        private float nextAvailableTime = 0f;

        private Vector3 originalSpriteScale;

        public override IEnumerator Activate()
        {
            if (Time.time < nextAvailableTime)
                yield break;

            nextAvailableTime = Time.time + cooldown;
            active = true;

            if (spriteRenderer == null)
                spriteRenderer = monster.GetComponentInChildren<SpriteRenderer>();
            if (animator == null)
                animator = monster.GetComponentInChildren<Animator>();

            if (spriteRenderer != null)
            {
                originalSprite = spriteRenderer.sprite;
                originalSpriteScale = spriteRenderer.transform.localScale;

                // ✅ sprite 교체 + 크기 확대
                spriteRenderer.sprite = pullSprite;
                spriteRenderer.transform.localScale = originalSpriteScale * 2f;
            }

            if (animator != null)
                animator.enabled = false;

            float t = 0f;
            while (t < pullDuration)
            {
                // ✅ sprite 강제 유지
                if (spriteRenderer != null && pullSprite != null)
                    spriteRenderer.sprite = pullSprite;

                // 플레이어 끌어당기기
                Vector2 dir = (monster.transform.position - playerCharacter.transform.position).normalized;
                float delta = pullStrength * Time.deltaTime;
                playerCharacter.transform.position += (Vector3)(dir * delta);

                t += Time.deltaTime;
                yield return null;
            }

            // ✅ sprite 원래대로 복구
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = originalSprite;
                spriteRenderer.transform.localScale = originalSpriteScale;
            }

            if (animator != null)
                animator.enabled = true;

            active = false;
        }


        public override float Score()
        {
            return Time.time >= nextAvailableTime ? 1f : 0f;
        }
    }
}
