using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Vampire
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Monster : IDamageable, ISpatialHashGridClient
    {
        [SerializeField] protected Material defaultMaterial, whiteMaterial, dissolveMaterial;
        [SerializeField] protected ParticleSystem deathParticles;
        [SerializeField] protected GameObject shadow;
        protected BoxCollider2D monsterHitbox;
        protected CircleCollider2D monsterLegsCollider;
        protected int monsterIndex;
        protected MonsterBlueprint monsterBlueprint;
        protected SpriteAnimator monsterSpriteAnimator;
        protected SpriteRenderer monsterSpriteRenderer;
        protected ZPositioner zPositioner;
        protected float currentHealth;  // 血量
        protected EntityManager entityManager;  // 怪物池
        protected Character playerCharacter;  // 角色
        protected Rigidbody2D rb;
        protected int currWalkSequenceFrame = 0;
        protected bool knockedBack = false;
        protected Coroutine hitAnimationCoroutine = null;
        protected bool alive = true;
        protected Transform centerTransform;
        public Transform CenterTransform { get => centerTransform; }
        public UnityEvent<Monster> OnKilled { get; } = new UnityEvent<Monster>();
        public float HP => currentHealth;
        // Spatial Hash Grid Client Interface
        public Vector2 Position => transform.position;
        public Vector2 Size => monsterLegsCollider.bounds.size;
        public Dictionary<int, int> ListIndexByCellIndex { get; set; }
        public int QueryID { get; set; } = -1;

        protected float maxHealth; // ✅ 추가

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            monsterLegsCollider = GetComponent<CircleCollider2D>();
            monsterSpriteAnimator = GetComponentInChildren<SpriteAnimator>();
            monsterSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            zPositioner = gameObject.AddComponent<ZPositioner>();
            monsterHitbox = monsterSpriteRenderer.gameObject.AddComponent<BoxCollider2D>();
            monsterHitbox.isTrigger = true;
        }

        public virtual void Init(EntityManager entityManager, Character playerCharacter)
        {
            this.entityManager = entityManager;
            this.playerCharacter = playerCharacter;
            zPositioner.Init(playerCharacter.transform);
        }

        public virtual void Setup(int monsterIndex, Vector2 position, MonsterBlueprint monsterBlueprint, float hpBuff = 0)
        {
            this.monsterIndex = monsterIndex;
            this.monsterBlueprint = monsterBlueprint;
            rb.position = position;
            transform.position = position;
            // Reset health to max
            // ✅ 체력 설정
            maxHealth = monsterBlueprint.hp + hpBuff;
            currentHealth = maxHealth;

            // Toggle alive flag on
            alive = true;
            // Add to list of living monsters
            entityManager.LivingMonsters.Add(this);
            // Initialize the animator
            monsterSpriteAnimator.Init(monsterBlueprint.walkSpriteSequence, monsterBlueprint.walkFrameTime, true);
            // Start and reset animation
            monsterSpriteAnimator.StartAnimating(true);
            // Ensure colliders are enabled and sized correctly
            monsterHitbox.enabled = true;
            monsterHitbox.size = monsterSpriteRenderer.bounds.size;
            monsterHitbox.offset = Vector2.up * monsterHitbox.size.y/2;
            monsterLegsCollider.radius = monsterHitbox.size.x/2.5f;
            centerTransform = (new GameObject("Center Transform")).transform;
            centerTransform.SetParent(transform);
            centerTransform.position = transform.position + (Vector3)monsterHitbox.offset;
            // Set the drag based on acceleration and movespeed
            float spd = Random.Range(monsterBlueprint.movespeed-0.1f, monsterBlueprint.movespeed+0.1f);
            rb.drag = monsterBlueprint.acceleration / (spd * spd);
            // Reset the velocity
            rb.velocity = Vector2.zero;
            StopAllCoroutines();
        }

        protected virtual void Update()
        {
            if (playerCharacter == null) return;
            // Direction
            monsterSpriteRenderer.flipX = ((playerCharacter.transform.position.x - rb.position.x) < 0);
        }

        protected virtual void FixedUpdate()
        {

        }

        public override void Knockback(Vector2 knockback)
        {
            rb.velocity += knockback * Mathf.Sqrt(rb.drag);
        }

        public override void TakeDamage(float damage, Vector2 knockback = default(Vector2), bool isCritical = false)
        {
            if (alive)
            {
                // ✅ 크리티컬 여부 전달
                entityManager.SpawnDamageText(monsterHitbox.transform.position, damage, isCritical);
                currentHealth -= damage;

                if (hitAnimationCoroutine != null) StopCoroutine(hitAnimationCoroutine);
                if (knockback != default(Vector2))
                {
                    rb.velocity += knockback * Mathf.Sqrt(rb.drag);
                    knockedBack = true;
                }
                if (currentHealth > 0)
                    hitAnimationCoroutine = StartCoroutine(HitAnimation());
                else
                    StartCoroutine(Killed());
            }
        }
        protected IEnumerator HitAnimation()
        {
            monsterSpriteRenderer.sharedMaterial = whiteMaterial;
            yield return new WaitForSeconds(0.15f);
            monsterSpriteRenderer.sharedMaterial = defaultMaterial;
            knockedBack = false;
        }

        public virtual IEnumerator Killed(bool killedByPlayer = true)
        {
            alive = false;
            monsterHitbox.enabled = false;
            entityManager.LivingMonsters.Remove(this);

            if (killedByPlayer)
                DropLoot();

            if (deathParticles != null)
                deathParticles.Play();

            yield return HitAnimation();

            if (deathParticles != null)
            {
                monsterSpriteRenderer.enabled = false;
                shadow.SetActive(false);

                // ✅ 스프라이트 재활성화 제거 → 잔류 방지
                // yield return new WaitForSeconds(deathParticles.main.duration - 0.15f);
                // monsterSpriteRenderer.enabled = true;
                // shadow.SetActive(true);
            }

            OnKilled.Invoke(this);
            OnKilled.RemoveAllListeners();

            // ✅ Index 검증
            if (monsterIndex >= 0)
                entityManager.DespawnMonster(monsterIndex, this, true);
            else
                Destroy(gameObject); // 분열된 보스 등 수동 삭제
        }


        protected virtual void DropLoot()
        {
            if (monsterBlueprint.gemLootTable.TryDropLoot(out GemType gemType))
                entityManager.SpawnExpGem((Vector2)transform.position, gemType);
            if (monsterBlueprint.coinLootTable.TryDropLoot(out CoinType coinType))
                entityManager.SpawnCoin((Vector2)transform.position, coinType);
        }

        public void RunAwayFrom(Vector2 playerPosition, float duration, float speedMultiplier)
        {
            Vector2 runDirection = (transform.position - (Vector3)playerPosition).normalized;
            StartCoroutine(RunAwayRoutine(runDirection, duration, speedMultiplier));
        }

        private IEnumerator RunAwayRoutine(Vector2 direction, float duration, float speedMultiplier)
        {
            float timer = 0f;
            float originalDrag = rb.drag;
            float originalSpeed = rb.velocity.magnitude;

            rb.drag = 0f; // 도망칠 때 저항 제거
            Vector2 fleeVelocity = direction * originalSpeed * speedMultiplier;
            rb.velocity = fleeVelocity;

            while (timer < duration)
            {
                rb.velocity = fleeVelocity; // 일정 속도로 도망
                timer += Time.deltaTime;
                yield return null;
            }

            rb.drag = originalDrag; // 저항 복구
        }
    }
}
