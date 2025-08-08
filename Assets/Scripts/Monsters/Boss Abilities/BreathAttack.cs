using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class BreathAttack : MonoBehaviour
    {
        [Header("Damage Settings")]
        public float damage = 20f;
        public float tickRate = 0.5f;
        public Vector2 size = new Vector2(8f, 2f);
        public LayerMask hitLayer;

        private float duration;
        private Vector2 direction;

        [Header("Visual Effect")]
        [SerializeField] private GameObject breathEffectPrefab;
        private GameObject spawnedEffect;

        [Header("Boss Sprite Control")]
        private SpriteRenderer bossRenderer;
        private Sprite breathSprite;
        private Sprite originalSprite;

        public void Init(Vector2 direction, float duration, SpriteRenderer bossRenderer, Sprite breathSprite)
        {
            this.direction = direction.normalized;
            this.duration = duration;
            this.bossRenderer = bossRenderer;
            this.breathSprite = breathSprite;

            StartCoroutine(BreathRoutine());
        }

        private IEnumerator BreathRoutine()
        {
            float elapsed = 0f;

            // 1. 보스 스프라이트 변경
            if (bossRenderer != null && breathSprite != null)
            {
                originalSprite = bossRenderer.sprite;
                bossRenderer.sprite = breathSprite;
            }

            // 2. 브레스 이펙트 생성 (입 위치에서)
            if (breathEffectPrefab != null)
            {
                Vector3 offset = Vector3.zero;

                if (direction == Vector2.right)
                    offset = new Vector3(7f, 0.4f, 0f);  // 오른쪽 입 위치 오프셋
                else if (direction == Vector2.left)
                    offset = new Vector3(-7f, 0.4f, 0f); // 왼쪽 입 위치 오프셋

                Vector3 spawnPos = transform.position + offset;

                spawnedEffect = Instantiate(breathEffectPrefab, spawnPos, Quaternion.identity, transform);

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                spawnedEffect.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // 브레스 방향 회전
            }

            // 3. 데미지 반복 적용
            while (elapsed < duration)
            {
                DamageInArea();
                yield return new WaitForSeconds(tickRate);
                elapsed += tickRate;
            }

            // 4. 종료: sprite 복구, 이펙트 제거
            if (spawnedEffect != null)
                Destroy(spawnedEffect);

            if (bossRenderer != null && originalSprite != null)
                bossRenderer.sprite = originalSprite;

            Destroy(gameObject);
        }

        private void DamageInArea()
        {
            Vector2 center = (Vector2)transform.position + direction * (size.x / 2f);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, hitLayer);

            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent<IDamageable>(out var target))
                {
                    target.TakeDamage(damage, Vector2.zero);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector2 center = (Vector2)transform.position + direction * (size.x / 2f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
