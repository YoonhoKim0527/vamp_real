using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class ReverseMirageAbility : Ability
    {
        [Header("Reverse Mirage Settings")]
        [SerializeField] float invincibleDuration = 2f;
        [SerializeField] float cooldown = 4f;
        [SerializeField] float explosionDelay = 2f;
        [SerializeField] float explosionRadius = 2f;
        [SerializeField] UpgradeableDamage counterDamage;
        [SerializeField] LayerMask monsterLayer;

        [Header("Visual Effects")]
        [SerializeField] GameObject cooldownRingPrefab;       // 1. 쿨타임 링
        [SerializeField] Color invincibleColor = Color.yellow; // 2. 무적 시 황금색
        [SerializeField] GameObject explosionRangePrefab;     // 3. 폭발 범위 표시용 프리팹

        bool onCooldown = false;
        SpriteRenderer playerRenderer;
        Color originalColor;
        GameObject cooldownRingInstance;

        public override void Select()
        {
            base.Select();
            gameObject.SetActive(true);
            playerCharacter.OnDamaged += OnPlayerHit;

            playerRenderer = playerCharacter.GetComponentInChildren<SpriteRenderer>();
            if (playerRenderer != null)
                originalColor = playerRenderer.color;

            ShowCooldownRing(); // ✅ 시작 시 링 표시
        }

        void OnPlayerHit()
        {
            if (onCooldown) return;
            onCooldown = true;

            HideCooldownRing(); // ✅ 링 제거
            CreateFakeCloneAndExplode();
            StartCoroutine(TemporaryInvincibility());
            StartCoroutine(ResetCooldown());
        }

        void ShowCooldownRing()
        {
            if (cooldownRingPrefab == null || cooldownRingInstance != null) return;

            cooldownRingInstance = Instantiate(cooldownRingPrefab, playerCharacter.transform);
        }

        void HideCooldownRing()
        {
            if (cooldownRingInstance != null)
            {
                Destroy(cooldownRingInstance);
                cooldownRingInstance = null;
            }
        }

        void CreateFakeCloneAndExplode()
        {
            var originalRenderer = playerCharacter.GetComponentInChildren<SpriteRenderer>();
            if (originalRenderer == null) return;

            GameObject clone = new GameObject("FakeClone");
            clone.transform.position = playerCharacter.transform.position;
            clone.transform.rotation = playerCharacter.transform.rotation;
            clone.transform.localScale = playerCharacter.transform.localScale;

            var sr = clone.AddComponent<SpriteRenderer>();
            sr.sprite = originalRenderer.sprite;
            sr.flipX = originalRenderer.flipX;
            sr.sortingLayerID = originalRenderer.sortingLayerID;
            sr.sortingOrder = originalRenderer.sortingOrder;
            sr.material = new Material(originalRenderer.sharedMaterial);
            sr.color = new Color(0, 0, 0, 1f); // 검은 잔상

            if (explosionRangePrefab != null)
            {
                GameObject rangeIndicator = Instantiate(explosionRangePrefab, clone.transform.position, Quaternion.identity);
                rangeIndicator.transform.localScale = Vector3.one * explosionRadius * 2f; // 반지름 -> 지름
                Destroy(rangeIndicator, explosionDelay);
            }

            StartCoroutine(DelayedExplosion(clone.transform.position, clone));
        }

        IEnumerator DelayedExplosion(Vector2 position, GameObject clone)
        {
            yield return new WaitForSeconds(explosionDelay);

            float totalDamage = counterDamage.Value * playerStats.attackPower;
            Collider2D[] hit = Physics2D.OverlapCircleAll(position, explosionRadius, monsterLayer);
            foreach (var col in hit)
            {
                if (col.TryGetComponent(out IDamageable dmg))
                    dmg.TakeDamage(totalDamage);
            }

            Debug.Log("💥 ReverseMirage Clone Exploded!");
            Destroy(clone);
        }

        IEnumerator TemporaryInvincibility()
        {
            if (playerRenderer != null)
                playerRenderer.color = invincibleColor;

            playerCharacter.SetInvincible(true);
            yield return new WaitForSeconds(invincibleDuration);
            playerCharacter.SetInvincible(false);

            if (playerRenderer != null)
                playerRenderer.color = originalColor;
        }

        IEnumerator ResetCooldown()
        {
            yield return new WaitForSeconds(cooldown);
            onCooldown = false;

            ShowCooldownRing(); // ✅ 쿨타임 끝나면 다시 링 생성
        }

        void OnDestroy()
        {
            playerCharacter.OnDamaged -= OnPlayerHit;
        }
    }
}
