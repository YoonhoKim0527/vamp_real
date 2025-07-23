using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class GrenadeThrowableAbility : ThrowableAbility
    {
        [Header("Grenade Stats")]
        [SerializeField] protected UpgradeableProjectileCount fragmentCount;
        [SerializeField] private int evolvedProjectileCount = 24;  // ✅ 진화 시 발사 개수
        [SerializeField] private float evolvedAngleStep = 15f;     // ✅ 각도 간격 (360/24)
        [SerializeField] private int evolvedRepeatCount = 2;       // ✅ 진화 시 반복 횟수
        [SerializeField] private float evolvedRepeatDelay = 0.6f;  // ✅ 발사 간격 (초)

        protected override IEnumerator Attack()
        {
            if (level >= 2)
            {
                Debug.Log("💣 [Grenade] Evolved mode attack triggered");
                // ✅ 진화 모드: 부모 Attack 무시하고 custom 패턴 사용
                yield return FireEvolvedGrenadesRepeatedly();
            }
            else
            {
                Debug.Log("💣 [Grenade] Normal attack triggered");
                // ✅ 기존 방식
                yield return base.Attack();
            }
        }

        private IEnumerator FireEvolvedGrenadesRepeatedly()
        {
            for (int repeat = 0; repeat < evolvedRepeatCount; repeat++)
            {
                Vector2 origin = playerCharacter.CenterTransform.position;

                Debug.Log($"💣 [Grenade] Evolved burst {repeat + 1}/{evolvedRepeatCount}");

                for (int i = 0; i < evolvedProjectileCount; i++)
                {
                    float angle = i * evolvedAngleStep;
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;

                    // ✅ CharacterStatBlueprint 기반 데미지 계산
                    float totalDamage = playerStats.attackPower * damage.Value;

                    // ✅ 치명타 확률 적용
                    bool isCritical = Random.value < playerStats.criticalChance;
                    if (isCritical)
                    {
                        totalDamage *= (1 + playerStats.criticalDamage);
                    }

                    float effectiveKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

                    GrenadeThrowable throwable = (GrenadeThrowable)entityManager.SpawnThrowable(
                        throwableIndex,
                        origin,
                        totalDamage,
                        effectiveKnockback,
                        0,
                        monsterLayer
                    );

                    throwable.SetupGrenade(fragmentCount.Value);

                    // 🟥 critical 정보도 넘기기
                    throwable.Throw(origin + direction * throwRadius, isCritical);
                    throwable.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
                }

                // ✅ 다음 발사까지 잠시 대기
                if (repeat < evolvedRepeatCount - 1)
                {
                    yield return new WaitForSeconds(evolvedRepeatDelay);
                }
            }
        }
    }
}
