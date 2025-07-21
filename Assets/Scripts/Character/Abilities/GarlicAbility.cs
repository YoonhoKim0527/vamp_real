using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class GarlicAbility : Ability
    {
        [Header("Garlic Stats")]
        [SerializeField] protected LayerMask monsterLayer;
        [SerializeField] protected UpgradeableDamage damage;
        [SerializeField] protected UpgradeableAOE radius;
        [SerializeField] protected UpgradeableDamageRate damageRate;
        [SerializeField] protected UpgradeableKnockback knockback;

        private float timeSinceLastAttack;
        private CircleCollider2D damageCollider;
        private SpriteRenderer garlicRenderer;

        private float lastRadius; // ✅ 마지막 반영된 반경 기록

        void Awake()
        {
            damageCollider = GetComponent<CircleCollider2D>();
            garlicRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public override void Init(AbilityManager abilityManager, EntityManager entityManager, Character playerCharacter, CharacterStatBlueprint playerStats)
        {
            base.Init(abilityManager, entityManager, playerCharacter, playerStats);
            transform.SetParent(playerCharacter.transform);
            transform.localPosition = Vector3.zero;
        }

        protected override void Use()
        {
            base.Use();
            gameObject.SetActive(true);

            // ✅ 처음 실행 시 반경 적용
            UpdateVisuals();
            lastRadius = radius.Value;
        }

        protected override void Upgrade()
        {
            base.Upgrade();
            // ✅ 업그레이드 시 반경 적용
            UpdateVisuals();
            lastRadius = radius.Value;
        }

        void Update()
        {
            // ✅ 게임 중 AOE 값이 변경되면 스프라이트와 콜라이더 동기화
            if (Mathf.Abs(radius.Value - lastRadius) > 0.01f)
            {
                UpdateVisuals();
                lastRadius = radius.Value;
            }

            // ✅ 데미지 주기 처리
            timeSinceLastAttack += Time.deltaTime;
            if (timeSinceLastAttack >= 1f / damageRate.Value)
            {
                DealDamageInRadius();
                timeSinceLastAttack = 0f;
            }
        }

        private void UpdateVisuals()
        {
            if (damageCollider != null)
                damageCollider.radius = radius.Value;

            if (garlicRenderer != null && garlicRenderer.sprite != null)
            {
                float spriteWidth = garlicRenderer.sprite.bounds.size.x;
                float scale = (radius.Value * 2f) / spriteWidth; // ✅ 반지름 기준으로 스프라이트 스케일 계산
                garlicRenderer.transform.localScale = Vector3.one * scale;
            }
        }

        private void DealDamageInRadius()
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius.Value, monsterLayer);

            foreach (Collider2D collider in hitColliders)
            {
                IDamageable damageable = collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    // ✅ CharacterStatBlueprint 기반 데미지 계산
                    float totalDamage = playerStats.attackPower * damage.Value;

                    // ✅ 치명타 확률 적용
                    if (Random.value < playerStats.criticalChance)
                    {
                        totalDamage *= (1 + playerStats.criticalDamage);
                        Debug.Log("[GarlicAbility] Critical hit!");
                    }

                    Vector2 knockbackDirection = (damageable.transform.position - transform.position).normalized;
                    float effectiveKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

                    damageable.TakeDamage(totalDamage, effectiveKnockback * knockbackDirection);
                    playerCharacter.OnDealDamage.Invoke(totalDamage);
                }
            }
        }
    }
}
