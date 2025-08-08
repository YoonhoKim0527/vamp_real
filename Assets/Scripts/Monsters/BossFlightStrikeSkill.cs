using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class BossFlightStrikeSkill : BossAbility
    {
        [Header("Flight Settings")]
        [SerializeField] private float flightDuration = 3f;
        [SerializeField] private float flightSpeed = 10f;
        [SerializeField] private float damageAmount = 25f;
        [SerializeField] private float hitRadius = 1.5f;
        [SerializeField] private LayerMask hitLayer;
        [SerializeField] private Sprite flightSprite;

        private float nextAvailableTime = 0f;
        [SerializeField] private float cooldown = 10f;

        private SpriteRenderer spriteRenderer;
        private Sprite originalSprite;
        private Vector3 originalScale;

        public override IEnumerator Activate()
        {
            if (Time.time < nextAvailableTime)
                yield break;

            nextAvailableTime = Time.time + cooldown;
            active = true;

            if (spriteRenderer == null)
                spriteRenderer = monster.SpriteRenderer;

            originalSprite = spriteRenderer.sprite;
            originalScale = spriteRenderer.transform.localScale;

            // 애니메이션 중단 + 스프라이트 변경 + 무적
            monster.Animator.StopAnimating();
            spriteRenderer.sprite = flightSprite;
            spriteRenderer.transform.localScale = originalScale;
            monster.SetInvincible(true);

            float elapsed = 0f;
            Vector2 direction = monster.LookDirection.normalized;

            while (elapsed < flightDuration)
            {
                // 이동
                monster.transform.position += (Vector3)(direction * flightSpeed * Time.deltaTime);

                // 충돌 판정
                Collider2D[] hits = Physics2D.OverlapCircleAll(monster.transform.position, hitRadius, hitLayer);
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent<IDamageable>(out var target))
                    {
                        target.TakeDamage(damageAmount, Vector2.zero);
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 복구
            spriteRenderer.sprite = originalSprite;
            spriteRenderer.transform.localScale = originalScale;
            monster.Animator.StartAnimating(true);
            monster.SetInvincible(false);

            active = false;
        }

        public override float Score()
        {
            return Time.time >= nextAvailableTime ? 1f : 0f;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, hitRadius);
        }
    }
}
