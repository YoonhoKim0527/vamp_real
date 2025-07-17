using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class MolotovAbility : ThrowableAbility
    {
        [Header("Molotov Stats")]
        [SerializeField] protected UpgradeableDuration duration;
        [SerializeField] protected UpgradeableAOE fireRadius;
        [SerializeField] protected UpgradeableDamageRate fireDamageRate;

        [Header("Ink Splash Settings")]
        [SerializeField] private Sprite inkSplashSprite;     // ✅ 화면에 뿌려질 먹물
        [SerializeField] private Sprite inkFaceSprite;       // ✅ 몬스터 얼굴에 붙을 먹물
        [SerializeField] private float inkRadius = 5f;
        [SerializeField] private float inkDisplayTime = 1f;
        [SerializeField] private float inkCooldown = 10f;
        [SerializeField] private float confuseDuration = 2f;
        [SerializeField] private LayerMask monsterLayer;

        private float inkTimeSinceLastUse;
        private bool isEvolved => level >= 1;

        protected override void Use()
        {
            base.Use();

            if (CrossSceneData.ExtraProjectile && throwableCount != null)
                throwableCount.ForceAdd(1);
            if (CrossSceneData.BonusProjectile > 0 && throwableCount != null)
                throwableCount.ForceAdd(CrossSceneData.BonusProjectile);

            inkTimeSinceLastUse = inkCooldown;
            StartCoroutine(InkSplashLoop());
        }

        protected override void Update()
        {
            base.Update();
            if (isEvolved)
                inkTimeSinceLastUse += Time.deltaTime;
        }

        protected override void LaunchThrowable()
        {
            if (!isEvolved)
            {
                // 기존 Molotov
                float totalDamage = playerCharacter.Stats.GetTotalDamage() * damage.Value;
                MolotovThrowable throwable = (MolotovThrowable)entityManager.SpawnThrowable(
                    throwableIndex,
                    playerCharacter.CenterTransform.position,
                    totalDamage,
                    knockback.Value,
                    0,
                    monsterLayer
                );

                throwable.SetupFire(duration.Value, fireRadius.Value, fireDamageRate.Value);

                List<ISpatialHashGridClient> nearbyEnemies = entityManager.Grid.FindNearbyInRadius(playerCharacter.transform.position, throwRadius);
                Vector2 throwPosition = (nearbyEnemies.Count > 0)
                    ? nearbyEnemies[Random.Range(0, nearbyEnemies.Count)].Position
                    : (Vector2)playerCharacter.transform.position + Random.insideUnitCircle * throwRadius;

                throwable.Throw(throwPosition);
                throwable.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
            }
            else
            {
                Debug.Log("[InkSplash] Molotov 진화 🖤 먹물 발동 준비!");
            }
        }

        private IEnumerator InkSplashLoop()
        {
            while (true)
            {
                if (isEvolved && inkTimeSinceLastUse >= inkCooldown)
                {
                    inkTimeSinceLastUse = 0f;
                    ActivateInkSplash();
                }
                yield return null;
            }
        }

        private void ActivateInkSplash()
        {
            Debug.Log("[InkSplash] 🖤 먹물 발동!");

            // ✅ 플레이어 중심에 먹물 효과
            GameObject inkZone = new GameObject("InkSplashZone");
            SpriteRenderer sr = inkZone.AddComponent<SpriteRenderer>();
            sr.sprite = inkSplashSprite;
            sr.sortingOrder = 1000;
            inkZone.transform.position = playerCharacter.CenterTransform.position;
            inkZone.transform.localScale = Vector3.one * (inkRadius);

            Destroy(inkZone, inkDisplayTime);

            Collider2D[] hitMonsters = Physics2D.OverlapCircleAll(playerCharacter.CenterTransform.position, inkRadius, monsterLayer);
            Debug.Log($"[InkSplash] 감지된 몬스터 수: {hitMonsters.Length}");

            foreach (Collider2D collider in hitMonsters)
            {
                Monster monster = collider.GetComponent<Monster>();
                if (monster != null)
                {
                    Vector2 awayDirection = ((Vector2)monster.transform.position - (Vector2)playerCharacter.transform.position).normalized;
                    float moveSpeed = 2f;

                    // ✅ 몬스터 얼굴에 먹물 추가
                    StartCoroutine(AddInkToMonsterFace(monster, confuseDuration));

                    // ✅ 도망가기
                    StartCoroutine(ForceMoveAway(monster, awayDirection, moveSpeed, confuseDuration));
                }
            }
        }

        private IEnumerator AddInkToMonsterFace(Monster monster, float duration)
        {
            // 🖤 몬스터 얼굴에 먹물 스프라이트 붙이기
            GameObject inkOnFace = new GameObject("InkFace");
            SpriteRenderer inkRenderer = inkOnFace.AddComponent<SpriteRenderer>();
            inkRenderer.sprite = inkFaceSprite;
            inkRenderer.sortingOrder = 20; // 몬스터 sprite 위에 표시

            inkOnFace.transform.SetParent(monster.CenterTransform);
            inkOnFace.transform.localPosition = Vector3.zero;
            inkOnFace.transform.localScale = Vector3.one;

            yield return new WaitForSeconds(duration);

            if (inkOnFace != null)
                Destroy(inkOnFace); // 시간 지나면 제거
        }

        private IEnumerator ForceMoveAway(Monster monster, Vector2 direction, float speed, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (monster != null)
                    monster.transform.position += (Vector3)(direction * speed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
