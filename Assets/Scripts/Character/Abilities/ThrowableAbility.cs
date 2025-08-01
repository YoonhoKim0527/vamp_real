using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class ThrowableAbility : Ability
    {
        [Header("Throwable Stats")]
        [SerializeField] protected GameObject throwablePrefab;
        [SerializeField] protected LayerMask monsterLayer;
        [SerializeField] protected float throwRadius;
        [SerializeField] protected UpgradeableDamageRate throwRate;
        [SerializeField] protected UpgradeableDamage damage;
        [SerializeField] protected UpgradeableKnockback knockback;
        [SerializeField] protected UpgradeableWeaponCooldown cooldown;
        [SerializeField] protected UpgradeableProjectileCount throwableCount;

        protected float timeSinceLastAttack;
        protected int throwableIndex;

        protected override void Use()
        {
            base.Use();
            gameObject.SetActive(true);
            timeSinceLastAttack = cooldown.Value;
            throwableIndex = entityManager.AddPoolForThrowable(throwablePrefab);
        }

        protected virtual void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            if (timeSinceLastAttack >= cooldown.Value)
            {
                timeSinceLastAttack = Mathf.Repeat(timeSinceLastAttack, cooldown.Value);
                StartCoroutine(Attack());
            }
        }

        protected virtual IEnumerator Attack()
        {
            timeSinceLastAttack -= throwableCount.Value / throwRate.Value;
            for (int i = 0; i < throwableCount.Value; i++)
            {
                LaunchThrowable();
                yield return new WaitForSeconds(1 / throwRate.Value);
            }
        }

        protected virtual void LaunchThrowable()
        {
            // ✅ CharacterStatBlueprint 기반 데미지 계산
            float totalDamage = playerStats.attackPower * damage.Value;

            // ✅ 치명타 여부 계산
            bool isCritical = false;
            if (Random.value < playerStats.criticalChance)
            {
                totalDamage *= (1 + playerStats.criticalDamage);
                isCritical = true;
            }

            // ✅ 방어력 기반 넉백 강화
            float effectiveKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

            // ✅ 투척체 생성 및 발사
            Throwable throwable = entityManager.SpawnThrowable(
                throwableIndex,
                playerCharacter.CenterTransform.position,
                totalDamage,
                effectiveKnockback,
                0,
                monsterLayer
            );

            // ✅ isCritical 전달
            throwable.SetCritical(isCritical);

            throwable.Throw((Vector2)playerCharacter.transform.position + Random.insideUnitCircle * throwRadius);
            throwable.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
        }

        public override void MirrorActivate(float damageMultiplier, Vector3 spawnPos, Color ghostColor)
        {
            StartCoroutine(MirrorThrowRoutine(damageMultiplier, spawnPos, ghostColor));
        }

        private IEnumerator MirrorThrowRoutine(float damageMultiplier, Vector3 spawnPos, Color ghostColor)
        {
            int count = throwableCount.Value;

            for (int i = 0; i < count; i++)
            {
                // ✅ 데미지 계산
                float totalDamage = playerStats.attackPower * damage.Value * damageMultiplier;

                bool isCritical = Random.value < playerStats.criticalChance;
                if (isCritical)
                {
                    totalDamage *= (1 + playerStats.criticalDamage);
                }

                float effectiveKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

                Throwable throwable = entityManager.SpawnThrowable(
                    throwableIndex,
                    spawnPos,
                    totalDamage,
                    effectiveKnockback,
                    0,
                    monsterLayer
                );

                // ✅ critical 정보 및 시각 효과 설정
                throwable.SetCritical(isCritical);
                throwable.SetColor(ghostColor); // 🎨 고스트 전용 색상 추가 (추가 구현 필요)

                // ✅ 랜덤 방향으로 던짐
                throwable.Throw(spawnPos + (Vector3)(Random.insideUnitCircle * throwRadius));
                throwable.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);

                yield return new WaitForSeconds(1 / throwRate.Value);
            }
        }

    }
}
