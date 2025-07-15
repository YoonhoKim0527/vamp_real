using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class BookAbility : Ability
    {
        [Header("Book Stats")]
        [SerializeField] protected GameObject bookPrefab;
        [SerializeField] protected LayerMask monsterLayer;
        [SerializeField] protected UpgradeableProjectileCount projectileCount;
        [SerializeField] protected UpgradeableAOE radius;
        [SerializeField] protected UpgradeableDamage damage;
        [SerializeField] protected UpgradeableKnockback knockback;
        [SerializeField] protected UpgradeableRotationSpeed speed;
        [SerializeField] protected int bonusProjectile;

        [Header("Awakening Settings")]
        [SerializeField] private bool isAwakened = false;
        [SerializeField] private float expandInterval = 5f; // 몇 초마다 확장
        [SerializeField] private float expandDuration = 2f; // 확장 유지 시간
        [SerializeField] private float expandMultiplier = 2f; // 궤도 확장 배율
        [SerializeField] private float knockbackRadius = 3f;  // 확장 시 밀어내기 반경
        [SerializeField] private float awakenedPushForce = 10f; // ✅ 각성 스킬 사용 시 밀어내기 힘

        private float awakenTimer = 0f;
        private bool isExpanded = false;

        private List<Book> books;

        protected override void Use()
        {
            base.Use();

            bonusProjectile = 0;
            if (CrossSceneData.BonusProjectile > 0 && projectileCount != null)
            {
                bonusProjectile = CrossSceneData.BonusProjectile;
            }
            if (CrossSceneData.ExtraProjectile && projectileCount != null)
            {
                bonusProjectile += 1;
            }

            gameObject.SetActive(true);
            projectileCount.OnChanged.AddListener(RefreshBooks);
            RefreshBooks();

            // ✅ 초기 각성 여부 체크
            CheckAwakening();

            // ✅ 각성 상태라면 radius 범위 내 적들 밀어내기
            if (isAwakened)
            {
                PushEnemiesOutsideRadius();
            }
        }

        protected override void Upgrade()
        {
            base.Upgrade();
            RefreshBooks();

            // ✅ 레벨업 시 각성 체크
            CheckAwakening();
        }

        void Update()
        {
            float currentSpeed = speed.Value;

            if (isAwakened)
            {
                // 각성 패턴 로직
                awakenTimer += Time.deltaTime;

                if (isExpanded)
                {
                    // ✅ 확장 상태일 때만 속도 3배
                    currentSpeed *= 3f;
                }

                if (!isExpanded && awakenTimer >= expandInterval)
                {
                    StartCoroutine(ExpandAndContract());
                    awakenTimer = 0f;
                }
            }

            float currentRadius = radius.Value;
            if (isAwakened && isExpanded)
            {
                currentRadius *= expandMultiplier;
            }

            for (int i = 0; i < books.Count; i++)
            {
                float theta = (2 * Mathf.PI * i) / books.Count;
                books[i].transform.localPosition = new Vector3(
                    Mathf.Sin(theta + Time.time * currentSpeed) * currentRadius,
                    Mathf.Cos(theta + Time.time * currentSpeed) * currentRadius,
                    0
                );
            }
        }


        private void CheckAwakening()
        {
            if (!isAwakened && level >= 1)
            {
                isAwakened = true;
                Debug.Log("📘 BookAbility: Awakened skill activated!");
            }
        }

        private IEnumerator ExpandAndContract()
        {
            isExpanded = true;
            Debug.Log("📘 BookAbility: Expanding orbit!");

            // ✅ 확장 시 주변 적 밀어내기
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(playerCharacter.transform.position, knockbackRadius, monsterLayer);
            foreach (var enemy in hitEnemies)
            {
                Vector2 direction = (enemy.transform.position - playerCharacter.transform.position).normalized;
                if (enemy.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(0, knockback.Value * direction); // 데미지 0, 넉백만
                }
            }

            yield return new WaitForSeconds(expandDuration);
            isExpanded = false;
            Debug.Log("📘 BookAbility: Contracting orbit!");
        }

        private void PushEnemiesOutsideRadius()
        {
            // ✅ 플레이어 중심으로부터 radius.Value 안의 적 탐색
            Collider2D[] enemies = Physics2D.OverlapCircleAll(playerCharacter.transform.position, radius.Value, monsterLayer);
            foreach (var enemy in enemies)
            {
                Vector2 playerPos = playerCharacter.transform.position;
                Vector2 enemyPos = enemy.transform.position;
                Vector2 direction = (enemyPos - playerPos).normalized;

                // ✅ 목표 위치 계산: 플레이어로부터 radius.Value 거리의 지점
                Vector2 targetPos = playerPos + direction * radius.Value;

                // ✅ 적을 radius 바깥으로 강제로 이동시킴
                enemy.transform.position = targetPos;

                // ✅ 강한 넉백 적용
                if (enemy.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(0, awakenedPushForce * direction); // 데미지 없이 넉백만
                }
            }
            Debug.Log("📘 BookAbility: Forced enemies outside radius!");
        }

        public void Damage(IDamageable damageable)
        {
            float totalDamage = playerCharacter.Stats.GetTotalDamage() * damage.Value;
            Vector2 knockbackDirection = (damageable.transform.position - playerCharacter.transform.position).normalized;
            damageable.TakeDamage(totalDamage, knockback.Value * knockbackDirection);
            playerCharacter.OnDealDamage.Invoke(totalDamage);
        }

        private void RefreshBooks()
        {
            if (books != null)
            {
                foreach (var book in books)
                {
                    if (book != null)
                        Destroy(book.gameObject);
                }
                books.Clear();
            }
            else
            {
                books = new List<Book>();
            }

            int totalProjectiles = projectileCount.Value + bonusProjectile;
            Debug.Log($"[BookAbility] RefreshBooks: totalProjectiles = {totalProjectiles}");

            for (int i = 0; i < totalProjectiles; i++)
            {
                AddBook();
            }
        }

        private void AddBook()
        {
            Book book = Instantiate(bookPrefab, playerCharacter.transform).GetComponent<Book>();
            book.Init(this, monsterLayer);
            books.Add(book);
        }

        private void OnDrawGizmosSelected()
        {
            // 🔵 넉백 반경 디버그 표시
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radius.Value); // ✅ radius 디버그
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, knockbackRadius); // ✅ 확장 반경 디버그
        }
    }
}
