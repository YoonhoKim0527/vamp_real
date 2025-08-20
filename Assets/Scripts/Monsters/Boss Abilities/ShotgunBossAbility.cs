using System.Collections;
using UnityEngine;
using System.Reflection; // ★ 리플렉션로 TTL/사거리/오프스크린 소거 제어

namespace Vampire
{
    /// <summary>
    /// 플래시게임식 '학습 가능한' 탄막 패턴 스케줄러.
    /// - Muzzle(자식 "Muzzle")에서만 발사
    /// - 무작위 없음: 모든 각/속도/타이밍은 결정적
    /// - 4개 패턴을 순차 재생 (원하면 순서/시간 조정)
    /// </summary>
    public class ShotgunBossAbility : BossAbility
    {
        [Header("Bullet / Pooling")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private LayerMask targetLayer;
        private int projectileIndex = -1;

        [Header("Damage / Kinematics")]
        [SerializeField] private float damage = 6f;
        [SerializeField] private float knockback = 2f;
        [SerializeField] private float bulletSpeedMin = 12f;   // 패턴별 기본 속도
        [SerializeField] private float bulletSpeedMax = 16f;

        [Header("Skill Timing")]
        [SerializeField] private float cooldown = 8f;
        [SerializeField] private float skillDurationTotal = 10f; // 전체 패턴 러닝 시간(초)
        [SerializeField] private bool anchorDuringSkill = true;  // 스킬 동안 정지할지 여부
        private float nextAvailableTime = 0f;

        [Header("Muzzle / Fallback")]
        [SerializeField] private Vector2 fallbackMuzzleLocalOffset = new Vector2(0.9f, -0.15f);
        private Transform muzzle;

        [Header("⛽ Range Booster (Far Travel Only)")] // ★ 추가
        [Tooltip("모든 총알 속도 배수 (멀리 보이도록)")]
        [SerializeField] private float globalSpeedMultiplier = 3.0f;     // 기본 3배

        [Tooltip("Projectile 내부 TTL/사거리/오프스크린 소거를 가능한 경우 강제 조정")]
        [SerializeField] private bool tryReflectionTuning = true;        // 켜두길 권장
        [Tooltip("최소 수명(초) 강제")]
        [SerializeField] private float lifetimeOverrideSeconds = 10f;     // 예: 10초
        [Tooltip("최소 사거리(유닛) 강제")]
        [SerializeField] private float rangeOverrideDistance = 100f;      // 예: 100유닛
        [Tooltip("있다면 오프스크린 자동 소거를 끕니다.")]
        [SerializeField] private bool disableOffscreenDespawn = true;

        // ===== 패턴 파라미터 =====
        [Header("PATTERN A: Sweeping Fan")]
        [SerializeField] private float A_duration = 2.5f;
        [SerializeField] private int   A_bulletsPerBurst = 9;
        [SerializeField] private float A_spreadDeg = 75f;
        [SerializeField] private float A_burstsPerSecond = 8f;
        [SerializeField] private float A_centerSweepSpanDeg = 90f;      // 중심각이 왕복할 각도(±)
        [SerializeField] private float A_centerSweepSpeedDegPerSec = 220f;

        [Header("PATTERN B: Double Spiral")]
        [SerializeField] private float B_duration = 2.5f;
        [SerializeField] private float B_fireHzPerSpiral = 12f;         // 한 나선당 초당 발수
        [SerializeField] private float B_angularVelDegPerSec = 420f;    // 나선 각속도
        [SerializeField] private float B_speed = 14f;

        [Header("PATTERN C: Ring With Gap (rotating safe lane)")]
        [SerializeField] private float C_duration = 2.0f;
        [SerializeField] private int   C_ringBulletCount = 36;
        [SerializeField] private float C_ringInterval = 0.45f;
        [SerializeField] private float C_gapWidthDeg = 36f;
        [SerializeField] private float C_gapRotateSpeedDegPerSec = 150f;
        [SerializeField] private float C_speed = 13f;

        [Header("PATTERN D: Aimed Offset Bursts (sweep offset)")]
        [SerializeField] private float D_duration = 3.0f;
        [SerializeField] private int   D_bulletsPerBurst = 11;
        [SerializeField] private float D_spreadDeg = 70f;
        [SerializeField] private float D_burstsPerSecond = 6f;
        [SerializeField] private float D_offsetSweepSpanDeg = 60f;      // 조준 중심을 좌↔우로 이동
        [SerializeField] private float D_offsetSweepSpeedDegPerSec = 180f;
        [SerializeField] private float D_speed = 15f;

        // 내부 상태
        private float baseAimDeg; // 활성화 순간 플레이어를 향한 기준각(결정성 유지용)

        public override void Init(BossMonster monster, EntityManager entityManager, Character playerCharacter)
        {
            base.Init(monster, entityManager, playerCharacter);
            projectileIndex = entityManager.AddPoolForProjectile(bulletPrefab);

            // ✅ 자식 "Muzzle" 자동 탐색
            muzzle = monster.transform.Find("muzzle");
            if (muzzle == null)
                Debug.LogWarning("[ShotgunBossAbility] Muzzle not found; fallback offset will be used.");
        }

        public override IEnumerator Activate()
        {
            if (Time.time < nextAvailableTime || bulletPrefab == null)
                yield break;

            active = true;
            nextAvailableTime = Time.time + cooldown;

            if (anchorDuringSkill) monster.Stop();

            // 기준 조준 각(도): 스킬 시작순간의 플레이어 방향으로 고정 → 반복 가능
            Vector2 toPlayer = playerCharacter.transform.position - GetMuzzleWorldPosition();
            baseAimDeg = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;


            // === 패턴 시퀀스 ===
            // 전체 길이가 skillDurationTotal을 넘지 않도록 각 패턴 duration을 잘라서 실행
            float t = 0f;
            float dur;

            // A
            dur = Mathf.Min(A_duration, skillDurationTotal - t);
            if (dur > 0f) { yield return StartCoroutine(Pattern_A_SweepingFan(dur)); t += dur; }

            // B
            dur = Mathf.Min(B_duration, skillDurationTotal - t);
            if (dur > 0f) { yield return StartCoroutine(Pattern_B_DoubleSpiral(dur)); t += dur; }

            // C
            dur = Mathf.Min(C_duration, skillDurationTotal - t);
            if (dur > 0f) { yield return StartCoroutine(Pattern_C_RingWithGap(dur)); t += dur; }

            // D
            dur = Mathf.Min(D_duration, skillDurationTotal - t);
            if (dur > 0f) { yield return StartCoroutine(Pattern_D_AimedOffsetBursts(dur)); t += dur; }
            active = false;
        }

        public override float Score()
        {
            // 중거리 가중치 + 쿨다운 반영
            float distance = Vector2.Distance(monster.transform.position, playerCharacter.transform.position);
            float x = distance / 6f;
            float u = 0.65f;
            float o = 0.25f;
            float exp = -Mathf.Pow(x - u, 2) / (2 * o * o);
            float s = Mathf.Exp(exp);
            if (Time.time < nextAvailableTime) s *= 0.001f;
            return s;
        }

        // ===== Pattern A: Sweeping Fan =====
        private IEnumerator Pattern_A_SweepingFan(float duration)
        {
            float elapsed = 0f;
            float burstInterval = 1f / Mathf.Max(1f, A_burstsPerSecond);

            float burstTimer = 0f;
            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                burstTimer += dt;

                // 중심 각을 왕복(sin)으로 스윕: baseAimDeg ± span
                float theta = elapsed * A_centerSweepSpeedDegPerSec;
                float center = baseAimDeg + Mathf.Sin(theta * Mathf.Deg2Rad) * A_centerSweepSpanDeg * 0.5f;

                while (burstTimer >= burstInterval)
                {
                    burstTimer -= burstInterval;
                    FireFan(center, A_bulletsPerBurst, A_spreadDeg, SpeedMid());
                }

                // (선택) 보스 미세 이동으로 압박을 주고 싶다면 해제
                if (!anchorDuringSkill)
                {
                    Vector2 moveDir = (playerCharacter.transform.position - monster.transform.position).normalized;
                    monster.Move(moveDir, dt);
                    entityManager.Grid.UpdateClient(monster);
                }

                yield return null;
            }
            yield return null; // 반환값: 경과시간
        }

        // ===== Pattern B: Double Spiral (counter-rotating) =====
        private IEnumerator Pattern_B_DoubleSpiral(float duration)
        {
            float elapsed = 0f;
            float fireInterval = 1f / Mathf.Max(1f, B_fireHzPerSpiral); // 한 나선당
            float timer1 = 0f, timer2 = 0f;

            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                timer1 += dt;
                timer2 += dt;

                // 두 나선 중심각(시간에 따라 등속 회전, 서로 반대 방향)
                float ang1 = baseAimDeg + (elapsed * B_angularVelDegPerSec);
                float ang2 = baseAimDeg + 180f - (elapsed * B_angularVelDegPerSec);

                if (timer1 >= fireInterval)
                {
                    timer1 -= fireInterval;
                    FireSingle(ang1, B_speed);
                }
                if (timer2 >= fireInterval)
                {
                    timer2 -= fireInterval;
                    FireSingle(ang2, B_speed);
                }

                yield return null;
            }
            yield return null;
        }

        // ===== Pattern C: Rotating Ring with Gap =====
        private IEnumerator Pattern_C_RingWithGap(float duration)
        {
            float elapsed = 0f;
            float ringTimer = 0f;

            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                ringTimer += dt;

                if (ringTimer >= C_ringInterval)
                {
                    ringTimer -= C_ringInterval;

                    // 현재 시간에 따라 gap 중심각 회전
                    float gapCenter = baseAimDeg + (elapsed * C_gapRotateSpeedDegPerSec);
                    FireRingWithGap(C_ringBulletCount, gapCenter, C_gapWidthDeg, C_speed);
                }

                yield return null;
            }
            yield return null;
        }

        // ===== Pattern D: Aimed Offset Bursts (조준 중심을 좌↔우로 스윕) =====
        private IEnumerator Pattern_D_AimedOffsetBursts(float duration)
        {
            float elapsed = 0f;
            float burstInterval = 1f / Mathf.Max(1f, D_burstsPerSecond);
            float burstTimer = 0f;

            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                burstTimer += dt;

                // 부채 중심을 좌↔우로 왕복 스윕
                float theta = elapsed * D_offsetSweepSpeedDegPerSec;
                float center = baseAimDeg + Mathf.Sin(theta * Mathf.Deg2Rad) * D_offsetSweepSpanDeg * 0.5f;

                while (burstTimer >= burstInterval)
                {
                    burstTimer -= burstInterval;
                    FireFan(center, D_bulletsPerBurst, D_spreadDeg, D_speed);
                }

                yield return null;
            }
            yield return null;
        }

        // ===== 발사 유틸 =====

        private void FireSingle(float angleDeg, float speed)
        {
            Vector3 pos = GetMuzzleWorldPosition();
            Vector2 dir = AngleToDir(angleDeg);
            SpawnBullet(pos, dir, speed);
        }

        private void FireFan(float centerDeg, int count, float spreadDeg, float speed)
        {
            Vector3 pos = GetMuzzleWorldPosition();

            int n = Mathf.Max(1, count);
            float total = spreadDeg;
            float half = total * 0.5f;
            float step = (n == 1) ? 0f : total / (n - 1);

            for (int i = 0; i < n; i++)
            {
                float local = (step * i) - half; // -half ~ +half
                float ang = centerDeg + local;
                Vector2 dir = AngleToDir(ang);
                SpawnBullet(pos, dir, speed);
            }
        }

        private void FireRingWithGap(int count, float gapCenterDeg, float gapWidthDeg, float speed)
        {
            Vector3 pos = GetMuzzleWorldPosition();

            int n = Mathf.Max(6, count);
            float step = 360f / n;

            float halfGap = gapWidthDeg * 0.5f;

            for (int i = 0; i < n; i++)
            {
                float ang = i * step; // 0..360 균등
                float d = DeltaAngleDeg(ang, gapCenterDeg);
                if (Mathf.Abs(d) <= halfGap) continue; // 빈틈(세이프 레인)

                Vector2 dir = AngleToDir(ang);
                SpawnBullet(pos, dir, speed);
            }
        }

        private void SpawnBullet(Vector3 pos, Vector2 dir, float speed)
        {
            // ★ 속도 배수로 시각적 이동거리 확 늘리기
            float spd = Mathf.Max(0.1f, speed) * Mathf.Max(1f, globalSpeedMultiplier);

            Projectile p = entityManager.SpawnProjectile(projectileIndex, pos, damage, knockback, spd, targetLayer);
            p.Launch(dir);

            // ★ TTL/사거리/오프스크린 소거 강제 연장 (가능할 때만)
            if (tryReflectionTuning) TryExtendProjectileByReflection(p);
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

        private static Vector2 AngleToDir(float angleDeg)
        {
            float r = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
        }

        private static float DeltaAngleDeg(float a, float b)
        {
            float d = Mathf.Repeat((a - b) + 180f, 360f) - 180f;
            return d;
        }

        private float SpeedMid()
        {
            return 0.5f * (bulletSpeedMin + bulletSpeedMax);
        }

        /// <summary>
        /// Projectile 내부의 TTL/사거리/오프스크린 소거를 넓은 네이밍 커버리지로 강제 조정합니다.
        /// 실제 구현에 해당 API/필드가 없으면 조용히 패스합니다.
        /// </summary>
        private void TryExtendProjectileByReflection(Projectile p)
        {
            object obj = p;
            var t = obj.GetType();

            // 우선 공개/비공개 setter 메서드 시도
            TryInvoke(t, obj, "SetLifetime",         lifetimeOverrideSeconds);
            TryInvoke(t, obj, "SetMaxDistance",      rangeOverrideDistance);
            if (disableOffscreenDespawn)
            {
                TryInvoke(t, obj, "SetOffscreenDespawn", false);
                TryInvoke(t, obj, "SetDestroyOffscreen", false);
                TryInvoke(t, obj, "SetDespawnOffscreen", false);
            }

            // 필드/프로퍼티명 후보군
            string[] lifeNames = { "lifetime","lifeTime","maxLifetime","ttl","timeToLive","Lifetime","lifetimeInSeconds" };
            string[] rangeNames = { "maxTravelDistance","maxDistance","range","Range","despawnDistance","maxTravelDistanceSquared","maxDistanceSqr" };
            string[] offscreenBools = { "offscreenDespawn","despawnOffscreen","destroyOffscreen","destroyWhenInvisible","despawnOnInvisible","disableOffscreenDespawn" };

            foreach (var name in lifeNames) TrySetFloatMember(t, obj, name, lifetimeOverrideSeconds, onlyIfLess:true);

            foreach (var name in rangeNames)
            {
                bool isSqr = name.ToLower().Contains("sqr");
                float val = isSqr ? rangeOverrideDistance * rangeOverrideDistance : rangeOverrideDistance;
                TrySetFloatMember(t, obj, name, val, onlyIfLess:true);
            }

            if (disableOffscreenDespawn)
                foreach (var name in offscreenBools) TrySetBoolMember(t, obj, name, false);
        }

        private static void TryInvoke(System.Type t, object obj, string methodName, object arg)
        {
            var m = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m == null) return;
            var ps = m.GetParameters();
            if (ps.Length != 1) return;

            if (ps[0].ParameterType.IsAssignableFrom(arg.GetType()))
                m.Invoke(obj, new object[] { arg });
            else if (ps[0].ParameterType == typeof(float) && arg is int i)
                m.Invoke(obj, new object[] { (float)i });
        }

        private static void TrySetFloatMember(System.Type t, object obj, string name, float val, bool onlyIfLess)
        {
            var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(float))
            {
                float curr = (float)f.GetValue(obj);
                if (!onlyIfLess || curr < val) f.SetValue(obj, val);
            }
            var pinfo = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (pinfo != null && pinfo.CanWrite && pinfo.PropertyType == typeof(float))
            {
                float curr = (float)(pinfo.GetValue(obj) ?? 0f);
                if (!onlyIfLess || curr < val) pinfo.SetValue(obj, val, null);
            }
        }

        private static void TrySetBoolMember(System.Type t, object obj, string name, bool val)
        {
            var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(bool))
            {
                f.SetValue(obj, val);
            }
            var pinfo = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (pinfo != null && pinfo.CanWrite && pinfo.PropertyType == typeof(bool))
            {
                pinfo.SetValue(obj, val, null);
            }
        }

    }
}
