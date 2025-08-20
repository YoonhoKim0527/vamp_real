using System.Collections;
using UnityEngine;

namespace Vampire
{
    /// <summary>
    /// 5초마다 확정 발동:
    /// - 전방으로 젤리 혀(프로젝타일) 발사 → 적중 시 플레이어를 복부로 끌어당김
    /// - holdDuration만큼 "삼킴" 상태 유지 후 토해내며 데미지와 넉백
    /// - Activate()는 즉시 반환 → 보스 이동/다른 로직 방해 안 함
    /// </summary>
    public class JellyDevourSkill : BossAbility
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject grabProjectilePrefab;   // JellyGrabProjectileSimple 포함
        [SerializeField] private LayerMask hitLayer;                // 보통 Player

        [Header("Timing")]
        [SerializeField] private float cooldown = 5f;               // ★ 5초 확정
        [SerializeField] private float pullDuration = 0.35f;        // 끌어당기는 시간(Fixed)
        [SerializeField] private float holdDuration = 0.75f;        // 삼킨 뒤 토하기 전까지 대기

        [Header("Anchors & Motion")]
        [SerializeField] private Vector2 mouthLocalOffset = new Vector2(0.7f, 0.2f);
        [SerializeField] private Vector2 bellyLocalOffset = new Vector2(0.0f, -0.2f);
        [SerializeField] private float projectileSpeed = 18f;
        [SerializeField] private float projectileLifetime = 2.2f;
        [SerializeField] private float spitOffset = 1.2f;
        [SerializeField] private float spitImpulse = 12f;

        [Header("Damage (다른 스킬 스타일)")]
        [Tooltip("토할 때 현재 체력의 몇 %를 데미지로 줄지")]
        [SerializeField, Range(0f, 1f)] private float damagePercentOfCurrentHP = 0.5f; // 50%
        [SerializeField] private float minDamage = 20f;                                 // 하한
        [SerializeField] private float knockbackForce = 6f;

        [Header("Spawn")]
        [SerializeField] private Vector2 fallbackMuzzleLocalOffset = new Vector2(0.9f, -0.15f);
        [SerializeField] private float forwardSpawnOffset = 0.4f;

        // 내부 상태
        private Transform muzzle;             // monster.transform.Find("muzzle") 자동 탐색
        private Transform mouthAnchor;
        private Transform bellyAnchor;

        private float nextAvailableTime = 0f;
        private bool  isRunning = false;

        // 캡처 상태
        private bool captured = false;
        private Character capturedTarget = null;

        public override void Init(BossMonster monster, EntityManager entityManager, Character playerCharacter)
        {
            base.Init(monster, entityManager, playerCharacter);

            // 발사구 자동 탐색(없으면 fallback 오프셋 사용)
            muzzle = monster.transform.Find("muzzle");

            mouthAnchor = new GameObject("MouthAnchor").transform;
            mouthAnchor.SetParent(monster.transform, false);
            mouthAnchor.localPosition = mouthLocalOffset;

            bellyAnchor = new GameObject("BellyAnchor").transform;
            bellyAnchor.SetParent(monster.transform, false);
            bellyAnchor.localPosition = bellyLocalOffset;
        }

        // ★ 선택되면 즉시 트리거하고 바로 반환(보스는 멈추지 않음)
        public override IEnumerator Activate()
        {
            if (Time.time < nextAvailableTime || isRunning || grabProjectilePrefab == null)
                yield break;

            nextAvailableTime = Time.time + cooldown;
            isRunning = true;

            // 한 사이클 비동기로 처리
            StartCoroutine(DevourCycle());

            // 즉시 반환 → 이동/다른 스킬과 병행 가능
            yield break;
        }

        // ★ 쿨다운 기반 점수(간단): 준비되면 1, 아니면 0
        public override float Score() => (Time.time >= nextAvailableTime && !isRunning && !captured) ? 1f : 0f;

        private IEnumerator DevourCycle()
        {
            // 1) 발사 시작점/방향 스냅샷
            Vector3 startPos = GetMuzzleWorldPosition();
            Vector3 targetPos = playerCharacter.transform.position;
            Vector2 dir = (targetPos - startPos).sqrMagnitude < 0.0001f
                ? Vector2.right
                : ((Vector2)(targetPos - startPos)).normalized;

            startPos += (Vector3)(dir * forwardSpawnOffset);

            // 2) 프로젝타일 생성
            GameObject go = Instantiate(grabProjectilePrefab, startPos, Quaternion.identity);
            var proj = go.GetComponent<JellyGrabProjectileSimple>();
            if (proj == null) proj = go.AddComponent<JellyGrabProjectileSimple>();

            proj.Init(
                owner: this,
                direction: dir,
                speed: projectileSpeed,
                lifetime: projectileLifetime,
                hitLayer: hitLayer
            );

            // 3) 캡처되거나 프로젝타일 수명 종료까지 대기
            float t0 = Time.time;
            while (!captured && (Time.time - t0) < projectileLifetime)
                yield return null;

            // 4) 캡처 안 되면 종료
            if (!captured)
            {
                isRunning = false;
                yield break;
            }

            // 5) 캡처되었으면 내부 시퀀스(끌어당김 → 홀드 → 토하기) 진행 완료까지 대기
            //     BeginCapture() 내부에서 실제 시퀀스를 시작하므로, captured=false 될 때까지 폴링
            while (captured)
                yield return null;

            isRunning = false;
        }

        // === 외부(프로젝타일)에서 호출 ===
        public void BeginCapture(Character target)
        {
            if (captured || target == null) return;
            captured = true;
            capturedTarget = target;
            StartCoroutine(SwallowSequence(target));
        }

        // 끌어당김 → 홀드 → 토하기
        private IEnumerator SwallowSequence(Character target)
        {
            // 끌어당김 (Fixed)
            yield return PullIntoBelly_Fixed(target);

            // 홀드
            yield return new WaitForSeconds(holdDuration);

            // 토하기 + 데미지
            SpitOutAndDamage(target);

            // 정리
            UnlockCharacter(target);
            capturedTarget = null;
            captured = false;
        }

        // FixedUpdate 타이밍에서 부드럽게 복부로 이동
        private IEnumerator PullIntoBelly_Fixed(Character target)
        {
            LockCharacter(target);

            Vector3 startPos = target.transform.position;
            Vector3 endPos = bellyAnchor != null ? bellyAnchor.position : GetLocalOffset(bellyLocalOffset);

            var rb = target.GetComponent<Rigidbody2D>();
            float elapsed = 0f;
            var wait = new WaitForFixedUpdate();

            while (elapsed < pullDuration)
            {
                elapsed += Time.fixedDeltaTime;
                float u = Mathf.SmoothStep(0f, 1f, elapsed / pullDuration);
                Vector3 p = Vector3.Lerp(startPos, endPos, u);

                if (rb != null) rb.MovePosition(p);
                else            target.transform.position = p;

                yield return wait;
            }

            if (rb != null) rb.MovePosition(endPos);
            else            target.transform.position = endPos;
        }

        private void SpitOutAndDamage(Character target)
        {
            if (target == null) return;

            // 토해낼 위치/방향
            Vector3 belly = bellyAnchor != null ? bellyAnchor.position : GetLocalOffset(bellyLocalOffset);
            Vector2 outDir = ((Vector2)(target.transform.position - monster.transform.position)).normalized;
            if (outDir.sqrMagnitude < 0.0001f) outDir = Vector2.right;

            Vector3 outPos = belly + (Vector3)(outDir * spitOffset);

            var rb = target.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = outPos;
                rb.velocity = Vector2.zero;
                rb.AddForce(outDir * spitImpulse, ForceMode2D.Impulse);
            }
            else
            {
                target.transform.position = outPos;
            }

            // 데미지(다른 스킬 스타일: IDamageable 사용)
            float dmg = ComputePercentDamage(target, damagePercentOfCurrentHP, minDamage);
            var dmgIf = target.GetComponentInParent<IDamageable>();
            if (dmgIf != null)
                dmgIf.TakeDamage(dmg, outDir * knockbackForce);
        }

        private static float ComputePercentDamage(Character ch, float percentOfCurrent, float minDamage)
        {
            // Character에 현재체력 필드/프로퍼티가 어떻게 되어 있든,
            // IDamageable만 믿고 "최소 데미지" 보장 + 퍼센트 체력 가정.
            float current = 0f;

            // 가능한 범용 접근(있으면 사용)
            var t = ch.GetType();
            var f = t.GetField("currentHealth", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(float)) current = (float)f.GetValue(ch);
            else
            {
                var p = t.GetProperty("CurrentHealth", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (p != null && p.PropertyType == typeof(float))
                    current = (float)(p.GetValue(ch, null) ?? 0f);
            }

            float byPercent = Mathf.Max(minDamage, current * Mathf.Clamp01(percentOfCurrent));
            return Mathf.Max(1f, byPercent);
        }

        private void LockCharacter(Character ch)
        {
            // 이동/충돌 제한(너희 게임 구조에 맞춤)
            foreach (var col in ch.GetComponentsInChildren<Collider2D>())
                col.enabled = false;

            var rb = ch.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;

            // 선택: Stun(float) 지원 시 무한 스턴
            var stun = ch.GetType().GetMethod("Stun", new[] { typeof(float) });
            if (stun != null) stun.Invoke(ch, new object[] { Mathf.Infinity });
        }

        private void UnlockCharacter(Character ch)
        {
            foreach (var col in ch.GetComponentsInChildren<Collider2D>())
                col.enabled = true;

            var stun = ch.GetType().GetMethod("Stun", new[] { typeof(float) });
            if (stun != null) stun.Invoke(ch, new object[] { 0.01f });
        }

        private Vector3 GetMuzzleWorldPosition()
        {
            if (muzzle != null) return muzzle.position;

            var sr = monster.SpriteRenderer;
            Vector3 pivot = (sr != null) ? sr.bounds.center : monster.transform.position;

            bool facingLeft = sr != null && sr.flipX;
            Vector2 local = fallbackMuzzleLocalOffset;
            if (facingLeft) local.x = -local.x;

            return pivot + (Vector3)local;
        }

        private Vector3 GetLocalOffset(Vector2 local)
        {
            var sr = monster.SpriteRenderer;
            bool left = (sr != null && sr.flipX);
            Vector2 off = local;
            if (left) off.x = -off.x;
            return monster.transform.position + (Vector3)off;
        }
    }
}
