using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class DaggerAbility : StabAbility
    {
        [Header("Dagger Stats")]
        [SerializeField] protected UpgradeableBleedDamage bleedDamage;
        [SerializeField] protected UpgradeableBleedRate bleedRate;
        [SerializeField] protected UpgradeableBleedDuration bleedDuration;

        [Header("Punch Attack Settings")]
        [SerializeField] private GameObject punchPrefab;              // ✅ 기존 주먹 prefab
        [SerializeField] private float punchRadius = 2f;              // ✅ 캐릭터 주변 반경
        [SerializeField] private float attackDuration = 3f;           // ✅ 주먹 폭우 지속시간
        [SerializeField] private float idleDuration = 3f;             // ✅ 대기시간
        [SerializeField] private float punchesPerSecond = 25f;        // ✅ 초당 주먹 개수
        [SerializeField] private float punchDamage = 30f;             // ✅ 기본 주먹 데미지
        [SerializeField] private float punchKnockback = 2f;           // ✅ 기본 넉백 세기
        [SerializeField] private float punchImpactRadius = 1f;        // ✅ 주먹 충격 범위

        [Header("Warning Settings")]
        [SerializeField] private Sprite dangerSprite;                 // ✅ 빨간 느낌표 스프라이트
        [SerializeField] private float warningDuration = 3f;          // ✅ 경고 깜빡임 시간
        [SerializeField] private float warningBlinkInterval = 0.2f;   // ✅ 깜빡임 간격

        [Header("Impact Effects")]
        [SerializeField] private Sprite dustSprite;                   // ✅ 황사(흙먼지) 스프라이트
        [SerializeField] private float dustScale = 1.5f;              // ✅ 황사 크기
        [SerializeField] private float dustDuration = 1f;             // ✅ 황사 유지 시간

        [SerializeField] private LayerMask monsterLayer;              // ✅ 몬스터 감지용 레이어

        private bool isActive = false;

        protected override void Use()
        {
            base.Use();
            CheckActivation();
        }

        protected override void Upgrade()
        {
            base.Upgrade();
            CheckActivation();
        }

        private void CheckActivation()
        {
            if (!isActive && level >= 0)
            {
                isActive = true;
                StartCoroutine(PunchAttackRoutine());
            }
        }

        private IEnumerator PunchAttackRoutine()
        {
            while (true)
            {
                SpawnDangerZoneWithPunchRain();
                yield return new WaitForSeconds(attackDuration + idleDuration);
            }
        }

        private void SpawnDangerZoneWithPunchRain()
        {
            Vector2 randomOffset = Random.insideUnitCircle * Random.Range(0, punchRadius);
            Vector2 dangerZonePosition = (Vector2)playerCharacter.transform.position + randomOffset;

            GameObject warning = CreateWarningEffect(dangerZonePosition, scaleMultiplier: 3f);
            float exactAreaRadius = warning.GetComponent<SpriteRenderer>().bounds.extents.x;

            StartCoroutine(WarningAndPunchRainRoutine(warning, dangerZonePosition, exactAreaRadius));
        }

        private GameObject CreateWarningEffect(Vector2 position, float scaleMultiplier = 1f)
        {
            // 🔴 Danger Zone
            GameObject warning = new GameObject("DangerZone");
            SpriteRenderer sr = warning.AddComponent<SpriteRenderer>();
            sr.sprite = dangerSprite;
            sr.sortingOrder = 10;
            warning.transform.position = position;
            warning.transform.localScale = Vector3.one * scaleMultiplier;

            // 🌫️ 황사 이펙트
            if (dustSprite != null)
            {
                GameObject haze = new GameObject("DustHaze");
                SpriteRenderer hazeSR = haze.AddComponent<SpriteRenderer>();
                hazeSR.sprite = dustSprite;
                hazeSR.sortingOrder = 5; // Danger Zone 밑에 표시
                hazeSR.color = new Color(1f, 1f, 1f, 0.4f); // 살짝 불투명

                haze.transform.position = position;
                haze.transform.localScale = Vector3.one * scaleMultiplier * 1.2f;

                // 🌫️ 부드러운 깜빡임 효과
                StartCoroutine(FadeHazeAlpha(hazeSR, 0.3f, 0.5f, 1.5f));

                // 황사를 Danger Zone의 자식으로 설정
                haze.transform.parent = warning.transform;
            }

            return warning;
        }

        private IEnumerator FadeHazeAlpha(SpriteRenderer hazeSR, float minAlpha, float maxAlpha, float cycleTime)
        {
            float t = 0f;
            bool fadingOut = false;

            while (hazeSR != null)
            {
                t += Time.deltaTime / cycleTime;
                float alpha = fadingOut
                    ? Mathf.Lerp(maxAlpha, minAlpha, t)
                    : Mathf.Lerp(minAlpha, maxAlpha, t);

                hazeSR.color = new Color(1f, 1f, 1f, alpha);

                if (t >= 1f)
                {
                    t = 0f;
                    fadingOut = !fadingOut;
                }
                yield return null;
            }
        }

        private IEnumerator WarningAndPunchRainRoutine(GameObject warning, Vector2 centerPosition, float exactAreaRadius)
        {
            SpriteRenderer sr = warning.GetComponent<SpriteRenderer>();
            float elapsed = 0f;

            while (elapsed < warningDuration)
            {
                sr.enabled = !sr.enabled;
                yield return new WaitForSeconds(warningBlinkInterval);
                elapsed += warningBlinkInterval;
            }

            Destroy(warning);

            StartCoroutine(PunchRain(centerPosition, exactAreaRadius / 2, attackDuration, punchesPerSecond));
        }

        private IEnumerator PunchRain(Vector2 centerPosition, float areaRadius, float rainDuration, float punchesPerSecond)
        {
            float elapsed = 0f;
            float spawnInterval = 1f / punchesPerSecond;

            while (elapsed < rainDuration)
            {
                float angle = Random.Range(0f, Mathf.PI * 2);
                float radius = areaRadius * Random.value; // 중심 밀집 분포
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                Vector2 spawnPosition = centerPosition + offset + Vector2.up * 10f;
                Vector2 targetPosition = centerPosition + offset;

                GameObject punch = Instantiate(punchPrefab, spawnPosition, Quaternion.Euler(0f, 0f, 180f));
                punch.transform.localScale = punchPrefab.transform.localScale * 1.5f;

                StartCoroutine(MovePunchToGround2D(punch, targetPosition, punchImpactRadius));
                Destroy(punch, 1f);

                yield return new WaitForSeconds(spawnInterval);
                elapsed += spawnInterval;
            }
        }

        private IEnumerator MovePunchToGround2D(GameObject punch, Vector2 targetPosition, float impactRadius)
        {
            Vector2 startPos = punch.transform.position;
            float duration = 0.02f; // 빠르게 낙하
            float elapsed = 0f;

            while (elapsed < duration)
            {
                punch.transform.position = Vector2.Lerp(startPos, targetPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            punch.transform.position = targetPosition;

            // ✅ CharacterStatBlueprint 기반 데미지 계산
            float totalDamage = playerStats.attackPower * punchDamage;
            float totalKnockback = punchKnockback * (1 + playerStats.defense * 0.1f);

            // ✅ 치명타 여부 계산
            bool isCriticalHit = false;
            if (Random.value < playerStats.criticalChance)
            {
                totalDamage *= (1 + playerStats.criticalDamage);
                isCriticalHit = true;
                Debug.Log("🥊 DaggerAbility: Critical Punch!");
            }

            // ✅ 몬스터 데미지 처리
            Collider2D[] hitMonsters = Physics2D.OverlapCircleAll(targetPosition, impactRadius, monsterLayer);
            foreach (Collider2D collider in hitMonsters)
            {
                Monster monster = collider.GetComponent<Monster>();
                if (monster != null)
                {
                    Vector2 monsterPos = (Vector2)monster.transform.position;
                    Vector2 knockbackDir = (monsterPos - targetPosition).normalized;
                    monster.TakeDamage(totalDamage, knockbackDir * totalKnockback, isCriticalHit); // ✅ 크리티컬 여부 전달
                }
            }
        }
    }
}
