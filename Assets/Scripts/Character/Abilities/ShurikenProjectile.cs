using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class ShurikenProjectile : Projectile
    {
        private Character player;
        private float throwRadius;
        private float throwTime;
        private float chainRange;
        private System.Action onReturnCallback; // ✅ 귀환 콜백

        private bool isReturning = false;
        private Monster currentTarget;

        public void Init(Character playerCharacter, float throwRadius, float throwTime, float chainRange, System.Action onReturnCallback)
        {
            this.player = playerCharacter;
            this.throwRadius = throwRadius;
            this.throwTime = throwTime;
            this.chainRange = chainRange;
            this.onReturnCallback = onReturnCallback;
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
                    currentTarget.TakeDamage(damage, Vector2.zero);
                    Debug.Log($"[ShurikenProjectile] {currentTarget.name} 타격");
                    OnHitDamageable.Invoke(damage);
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
            // ✅ 슈리켄은 절대 스스로 파괴 금지
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
