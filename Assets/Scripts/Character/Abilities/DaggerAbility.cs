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
        [SerializeField] private GameObject punchPrefab;
        [SerializeField] private float punchRadius = 2f;
        [SerializeField] private float attackDuration = 3f;
        [SerializeField] private float idleDuration = 3f;
        [SerializeField] private float punchesPerSecond = 25f;
        [SerializeField] private float punchDamage = 30f;
        [SerializeField] private float punchKnockback = 2f;
        [SerializeField] private float punchImpactRadius = 1f;

        [Header("Warning Settings")]
        [SerializeField] private Sprite dangerSprite;
        [SerializeField] private float warningDuration = 3f;
        [SerializeField] private float warningBlinkInterval = 0.2f;

        [Header("Impact Effects")]
        [SerializeField] private Sprite dustSprite;
        [SerializeField] private float dustScale = 1.5f;
        [SerializeField] private float dustDuration = 1f;

        [SerializeField] private LayerMask monsterLayer;

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
            if (!isActive && level >= 1)
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
            GameObject warning = new GameObject("DangerZone");
            SpriteRenderer sr = warning.AddComponent<SpriteRenderer>();
            sr.sprite = dangerSprite;
            sr.sortingOrder = 10;
            warning.transform.position = position;
            warning.transform.localScale = Vector3.one * scaleMultiplier;

            if (dustSprite != null)
            {
                GameObject haze = new GameObject("DustHaze");
                SpriteRenderer hazeSR = haze.AddComponent<SpriteRenderer>();
                hazeSR.sprite = dustSprite;
                hazeSR.sortingOrder = 5;
                hazeSR.color = new Color(1f, 1f, 1f, 0.4f);
                haze.transform.position = position;
                haze.transform.localScale = Vector3.one * scaleMultiplier * 1.2f;
                StartCoroutine(FadeHazeAlpha(hazeSR, 0.3f, 0.5f, 1.5f));
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

        private IEnumerator PunchRain(Vector2 centerPosition, float areaRadius, float rainDuration, float punchesPerSecond, bool isMirror = false)
        {
            float elapsed = 0f;
            float spawnInterval = 1f / punchesPerSecond;

            while (elapsed < rainDuration)
            {
                float angle = Random.Range(0f, Mathf.PI * 2);
                float radius = areaRadius * Random.value;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                Vector2 spawnPosition = centerPosition + offset + Vector2.up * 10f;
                Vector2 targetPosition = centerPosition + offset;

                GameObject punch = Instantiate(punchPrefab, spawnPosition, Quaternion.Euler(0f, 0f, 180f));
                punch.transform.localScale = punchPrefab.transform.localScale * 1.5f;

                StartCoroutine(MovePunchToGround2D(punch, targetPosition, punchImpactRadius, isMirror));
                Destroy(punch, 1f);

                yield return new WaitForSeconds(spawnInterval);
                elapsed += spawnInterval;
            }
        }

        private IEnumerator MovePunchToGround2D(GameObject punch, Vector2 targetPosition, float impactRadius, bool isMirror = false)
        {
            Vector2 startPos = punch.transform.position;
            float duration = 0.02f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                punch.transform.position = Vector2.Lerp(startPos, targetPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            punch.transform.position = targetPosition;

            float totalDamage = playerStats.attackPower * punchDamage;
            float totalKnockback = punchKnockback * (1 + playerStats.defense * 0.1f);

            if (isMirror)
            {
                totalDamage *= 0.6f;
                totalKnockback *= 0.5f;
            }

            bool isCriticalHit = false;
            if (!isMirror && Random.value < playerStats.criticalChance)
            {
                totalDamage *= (1 + playerStats.criticalDamage);
                isCriticalHit = true;
                Debug.Log("🥊 DaggerAbility: Critical Punch!");
            }

            Collider2D[] hitMonsters = Physics2D.OverlapCircleAll(targetPosition, impactRadius, monsterLayer);
            foreach (Collider2D collider in hitMonsters)
            {
                Monster monster = collider.GetComponent<Monster>();
                if (monster != null)
                {
                    Vector2 monsterPos = (Vector2)monster.transform.position;
                    Vector2 knockbackDir = (monsterPos - targetPosition).normalized;
                    monster.TakeDamage(totalDamage, knockbackDir * totalKnockback, isCriticalHit);
                }
            }
        }

        // 👻 Mirror용 외부 호출 함수
        public void MirrorActivate(Vector2 spawnPosition)
        {
            float radius = punchRadius * 0.7f;
            float duration = attackDuration * 0.8f;
            float pps = punchesPerSecond * 0.7f;

            StartCoroutine(PunchRain(spawnPosition, radius, duration, pps, isMirror: true));
        }
    }
}
