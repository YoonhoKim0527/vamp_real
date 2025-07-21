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
        [SerializeField] private float expandInterval = 5f;
        [SerializeField] private float expandDuration = 2f;
        [SerializeField] private float expandMultiplier = 2f;
        [SerializeField] private float knockbackRadius = 3f;
        [SerializeField] private float awakenedPushForce = 10f;

        private float awakenTimer = 0f;
        private bool isExpanded = false;

        private List<Book> books;

        protected override void Use()
        {
            base.Use();

            // ✅ CharacterStatBlueprint 기반 bonusProjectile 계산
            bonusProjectile = playerStats.extraProjectiles;
            Debug.Log($"[BookAbility] Blueprint Stats -> ExtraProjectiles: {playerStats.extraProjectiles}, Attack: {playerStats.attackPower}");

            gameObject.SetActive(true);
            projectileCount.OnChanged.AddListener(RefreshBooks);
            RefreshBooks();

            CheckAwakening();

            if (isAwakened)
            {
                PushEnemiesOutsideRadius();
            }
        }

        protected override void Upgrade()
        {
            base.Upgrade();
            RefreshBooks();
            CheckAwakening();
        }

        void Update()
        {
            float currentSpeed = speed.Value;

            if (isAwakened)
            {
                awakenTimer += Time.deltaTime;

                if (isExpanded)
                {
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
                Debug.Log("[BookAbility] Awakened!");
            }
        }

        private IEnumerator ExpandAndContract()
        {
            isExpanded = true;
            Debug.Log("[BookAbility] Expanding orbit!");

            // ✅ CharacterStatBlueprint의 defense 사용
            float effectiveKnockback = awakenedPushForce * (1 + playerStats.defense * 0.1f);

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(playerCharacter.transform.position, knockbackRadius, monsterLayer);
            foreach (var enemy in hitEnemies)
            {
                Vector2 direction = (enemy.transform.position - playerCharacter.transform.position).normalized;
                if (enemy.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(0, effectiveKnockback * direction); // 데미지 없이 넉백만
                }
            }

            yield return new WaitForSeconds(expandDuration);
            isExpanded = false;
            Debug.Log("[BookAbility] Contracting orbit!");
        }

        private void PushEnemiesOutsideRadius()
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(playerCharacter.transform.position, radius.Value, monsterLayer);
            foreach (var enemy in enemies)
            {
                Vector2 playerPos = playerCharacter.transform.position;
                Vector2 enemyPos = enemy.transform.position;
                Vector2 direction = (enemyPos - playerPos).normalized;
                Vector2 targetPos = playerPos + direction * radius.Value;

                enemy.transform.position = targetPos;

                if (enemy.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(0, awakenedPushForce * direction); // 넉백
                }
            }
            Debug.Log("[BookAbility] Forced enemies outside radius!");
        }

        public void Damage(IDamageable damageable)
        {
            // ✅ CharacterStatBlueprint 기반 데미지 계산
            float totalDamage = playerStats.attackPower * damage.Value;

            // ✅ 치명타 확률 적용
            if (Random.value < playerStats.criticalChance)
            {
                totalDamage *= (1 + playerStats.criticalDamage);
                Debug.Log("[BookAbility] Critical hit!");
            }

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
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radius.Value);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, knockbackRadius);
        }
    }
}
