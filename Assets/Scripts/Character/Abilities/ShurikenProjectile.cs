using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class ShurikenProjectile : Projectile
    {
        private Character player;
        private CharacterStatBlueprint playerStats; // ✅ 플레이어 스탯
        private float throwRadius;
        private float throwTime;
        private float chainRange;
        private System.Action onReturnCallback; // ✅ 귀환 콜백

        private bool isReturning = false;
        private Monster currentTarget;
        private bool isCritical; // ✅ 치명타 여부 저장

        public void Init(
            Character playerCharacter,
            CharacterStatBlueprint stats,
            float throwRadius,
            float throwTime,
            float chainRange,
            System.Action onReturnCallback,
            bool isCritical) // ✅ 치명타 여부 추가
        {
            this.player = playerCharacter;
            this.playerStats = stats;
            this.throwRadius = throwRadius;
            this.throwTime = throwTime;
            this.chainRange = chainRange;
            this.onReturnCallback = onReturnCallback;
            this.isCritical = isCritical; // ✅ 저장
            isReturning = false;
            currentTarget = null;
        }

        public void StartAttackSequence()
        {
            StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            Debug.Log("[ShurikenProjectile] 공격 루프 시작");

            // 🟥 첫 타겟 탐색
            currentTarget = FindClosestMonster(player.CenterTransform.position, throwRadius);

            if (currentTarget == null)
            {
                Debug.Log("[ShurikenProjectile] 첫 타겟 없음 → 플레이어 귀환");
                yield return SafeMoveToPlayer();
                ReturnToPlayer();
                yield break;
            }

            while (!isReturning)
            {
                // ✅ 타겟으로 이동
                yield return SafeMoveToTarget(currentTarget);

                // ✅ 도착 후 데미지 처리
                if (currentTarget != null && !isReturning)
                {
                    float totalDamage = damage; // ✅ 이미 Ability 쪽에서 계산된 값 사용

                    if (isCritical)
                    {
                        Debug.Log("💥 [ShurikenProjectile] Critical hit (from Ability)!");
                    }

                    Vector2 knockbackDir = (currentTarget.transform.position - player.CenterTransform.position).normalized;
                    float effectiveKnockback = knockback * (1 + playerStats.defense * 0.1f);

                    currentTarget.TakeDamage(totalDamage, knockbackDir * effectiveKnockback, isCritical); // ✅ 치명타 여부 전달
                    Debug.Log($"[ShurikenProjectile] {currentTarget.name} 타격 {totalDamage:F1} damage");

                    // DamageText 색상 적용
                    if (isCritical)
                    {
                        entityManager.SpawnDamageText(currentTarget.CenterTransform.position, totalDamage, true);
                    }
                    else
                    {
                        entityManager.SpawnDamageText(currentTarget.CenterTransform.position, totalDamage, false);
                    }

                    OnHitDamageable.Invoke(totalDamage);
                }

                // ✅ 다음 타겟 탐색
                Monster nextTarget = FindClosestMonster(currentTarget.transform.position, chainRange, currentTarget);

                if (nextTarget == null)
                {
                    Debug.Log("[ShurikenProjectile] 연쇄 종료 → 플레이어 귀환");
                    break;
                }

                currentTarget = nextTarget;
            }

            // ✅ 플레이어로 귀환
            yield return SafeMoveToPlayer();
            ReturnToPlayer();
        }

        private IEnumerator SafeMoveToTarget(Monster target)
        {
            if (target == null) yield break;

            Vector2 targetPos = target.transform.position;

            // ✅ 플레이어로부터 최대 거리 제한
            if (Vector2.Distance(player.CenterTransform.position, targetPos) > throwRadius * 1.5f)
            {
                Debug.LogWarning("[ShurikenProjectile] 타겟이 너무 멀음 → 플레이어로 귀환");
                yield return SafeMoveToPlayer();
                ReturnToPlayer();
                yield break;
            }

            yield return MoveToPosition(targetPos);
        }

        private IEnumerator SafeMoveToPlayer()
        {
            yield return MoveToPosition(player.CenterTransform.position);
        }

        private IEnumerator MoveToPosition(Vector2 targetPos)
        {
            Vector2 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < throwTime)
            {
                transform.position = Vector2.Lerp(startPos, targetPos, elapsed / throwTime);

                // ✅ 플레이어 반경 초과시 강제 귀환
                if (Vector2.Distance(player.CenterTransform.position, transform.position) > throwRadius * 2f)
                {
                    Debug.LogWarning("[ShurikenProjectile] 너무 멀어 → 강제 귀환");
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
        }

        private void ReturnToPlayer()
        {
            if (!isReturning)
            {
                isReturning = true;
                onReturnCallback?.Invoke();
                entityManager.DespawnProjectile(projectileIndex, this);
            }
        }

        protected override void DestroyProjectile()
        {
            Debug.LogWarning("[ShurikenProjectile] DestroyProjectile 차단됨");
        }

        protected override void HitNothing()
        {
            Debug.LogWarning("[ShurikenProjectile] HitNothing 호출 차단");
        }

        private Monster FindClosestMonster(Vector2 origin, float range, Monster exclude = null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range, targetLayer);
            Monster closest = null;
            float minDist = float.MaxValue;

            foreach (Collider2D hit in hits)
            {
                Monster m = hit.GetComponent<Monster>();
                if (m != null && m != exclude)
                {
                    float dist = (m.transform.position - (Vector3)origin).sqrMagnitude;
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = m;
                    }
                }
            }
            return closest;
        }
    }
}
