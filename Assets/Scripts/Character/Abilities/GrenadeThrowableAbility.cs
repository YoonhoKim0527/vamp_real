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
                Debug.Log("hi2");
                // ✅ 진화 모드: 부모 Attack 무시하고 custom 패턴 사용
                yield return FireEvolvedGrenadesRepeatedly();
            }
            else
            {
                Debug.Log("hi");
                // ✅ 기존 방식
                yield return base.Attack();
            }
        }

        private IEnumerator FireEvolvedGrenadesRepeatedly()
        {
            float totalDamage = playerCharacter.Stats.GetTotalDamage() * damage.Value;

            for (int repeat = 0; repeat < evolvedRepeatCount; repeat++)
            {
                Vector2 origin = playerCharacter.CenterTransform.position;

                Debug.Log($"[Grenade] Evolved 발사 {repeat + 1}/{evolvedRepeatCount}");

                for (int i = 0; i < evolvedProjectileCount; i++)
                {
                    float angle = i * evolvedAngleStep;
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;

                    GrenadeThrowable throwable = (GrenadeThrowable)entityManager.SpawnThrowable(
                        throwableIndex,
                        origin,
                        totalDamage,
                        knockback.Value,
                        0,
                        monsterLayer
                    );

                    throwable.SetupGrenade(fragmentCount.Value);
                    throwable.Throw(origin + direction * throwRadius); // ✅ 각도별 위치로 던짐
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
