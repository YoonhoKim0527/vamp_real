using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class JellyBossSkill : BossAbility
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject jellyBallPrefab;
        [SerializeField] private float projectileSpeed = 6f;
        [SerializeField] private float projectileLifetime = 5f;
        [SerializeField] private float projectileDamage = 10f;

        [Header("Target/Spawn")]
        [SerializeField] private LayerMask damageLayer;  // (참고용) 데미지 대상 레이어
        [SerializeField] private Vector2 fallbackMuzzleLocalOffset = new Vector2(0.6f, 0f);
        private Transform muzzle;

        [Header("Pattern")]
        [SerializeField] private int[] ringCounts = new int[] { 3, 4, 6, 10 };
        [SerializeField] private float ringInterval = 1f;           // 정확히 1초 간격
        [SerializeField] private float cooldownAfterSequence = 2f;  // 시퀀스 후 재발사 쿨다운

        [Header("Scoring (probability weight)")]
        [SerializeField] private float baseWeight = 1f;

        private Coroutine sequenceRoutine;
        private float lastSequenceEndTime = -999f;

        public override void Init(BossMonster monster, EntityManager entityManager, Character playerCharacter)
        {
            base.Init(monster, entityManager, playerCharacter);
            muzzle = monster.transform.Find("muzzle");
        }

        public override IEnumerator Activate()
        {
            // 이미 실행 중이면 중복 실행 금지
            if (sequenceRoutine != null) yield break;

            // 이동 막지 않도록 백그라운드로 돌리고 즉시 반환
            sequenceRoutine = StartCoroutine(RunSequence());
            yield break;
        }

        private IEnumerator RunSequence()
        {
            active = true;

            for (int i = 0; i < ringCounts.Length; i++)
            {
                // 예외 발생해도 다음 링으로 이어지도록 방어
                try
                {
                    FireRing(ringCounts[i]);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[JellyBossSkill] FireRing({ringCounts[i]}) error: {e}");
                }

                if (i < ringCounts.Length - 1)
                    yield return new WaitForSeconds(ringInterval);
            }

            lastSequenceEndTime = Time.time;
            active = false;
            sequenceRoutine = null;
        }

        private void FireRing(int count)
        {
            if (jellyBallPrefab == null || count <= 0) return;

            Vector3 origin = GetMuzzleWorldPosition();
            float step = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * step;
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                SpawnProjectile(origin, dir);
            }
        }

        private void SpawnProjectile(Vector3 origin, Vector2 dir)
        {
            var proj = Instantiate(jellyBallPrefab, origin, Quaternion.identity);
            proj.transform.SetParent(null); // 부모 스케일/회전 영향 제거

            // 컴포넌트 확보(없으면 자동 추가)
            if (!proj.TryGetComponent<CircleCollider2D>(out var col))
                col = proj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            // 이동/히트 스크립트 확보
            if (!proj.TryGetComponent<JellyBall>(out var ball))
                ball = proj.AddComponent<JellyBall>();

            // 초기화: 속도/수명/데미지/색상/기절
            ball.Init(dir, projectileSpeed, projectileLifetime, projectileDamage);
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

        // 여러 스킬 중 확률 가중치
        public override float Score()
        {
            if (sequenceRoutine != null) return 0f; // 시퀀스 중에는 비활성
            return (Time.time - lastSequenceEndTime) >= 5f ? baseWeight : 0f;
        }
    }
}
