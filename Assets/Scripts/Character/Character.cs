using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

namespace Vampire
{
    public class Character : IDamageable, ISpatialHashGridClient
    {
        [Header("Dependencies")]
        [SerializeField] protected Transform centerTransform;
        [SerializeField] protected Transform lookIndicator;
        [SerializeField] protected float lookIndicatorRadius;
        [SerializeField] protected TextMeshProUGUI levelText;
        [SerializeField] protected AbilitySelectionDialog abilitySelectionDialog;
        [SerializeField] protected PointBar healthBar;
        [SerializeField] protected PointBar expBar;
        [SerializeField] protected Collider2D collectableCollider;
        [SerializeField] protected Collider2D meleeHitboxCollider;
        [SerializeField] protected ParticleSystem dustParticles;
        [SerializeField] protected Material defaultMaterial, hitMaterial, deathMaterial;
        [SerializeField] protected ParticleSystem deathParticles;

        protected CharacterBlueprint characterBlueprint;
        protected UpgradeableMovementSpeed movementSpeed;
        protected UpgradeableArmor armor;
        protected bool alive = true;
        protected int currentLevel = 1;
        protected float currentExp = 0;
        protected float nextLevelExp = 5;
        protected float expToNextLevel = 5;
        protected float currentHealth;
        protected SpriteRenderer spriteRenderer;
        protected SpriteAnimator spriteAnimator;
        protected AbilityManager abilityManager;
        protected EntityManager entityManager;
        protected StatsManager statsManager;
        protected Rigidbody2D rb;
        protected ZPositioner zPositioner;
        protected Vector2 lookDirection = Vector2.right;
        protected CoroutineQueue coroutineQueue;
        protected Coroutine hitAnimationCoroutine = null;
        protected Vector2 moveDirection;

        public Vector2 LookDirection
        {
            get { return lookDirection; }
            set { if (value != Vector2.zero) lookDirection = value; }
        }
        public Transform CenterTransform => centerTransform;
        public Collider2D CollectableCollider => collectableCollider;
        public float Luck => characterBlueprint.luck;
        public int CurrentLevel => currentLevel;
        public UnityEvent<float> OnDealDamage { get; } = new UnityEvent<float>();
        public UnityEvent OnDeath { get; } = new UnityEvent();
        public CharacterBlueprint Blueprint => characterBlueprint;
        public Vector2 Velocity => rb.velocity;

        // Spatial Hash Grid Client Interface
        public Vector2 Position => transform.position;
        public Vector2 Size => meleeHitboxCollider.bounds.size;
        public Dictionary<int, int> ListIndexByCellIndex { get; set; }
        public int QueryID { get; set; } = -1;

        private CharacterStats stats;
        public CharacterStats Stats => stats;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            zPositioner = gameObject.AddComponent<ZPositioner>();
            spriteAnimator = GetComponentInChildren<SpriteAnimator>();
            spriteRenderer = spriteAnimator.GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            characterBlueprint = CrossSceneData.CharacterBlueprint; // 캐릭터 블루프린트는 CrossSceneData 유지

            // ✅ SaveManager로부터 SaveData 불러오기
            var saveManager = FindObjectOfType<SaveManager>();
            if (saveManager != null)
            {
                SaveData saveData = saveManager.LoadGame();
                stats = new CharacterStats(characterBlueprint); // 🔥 SaveFile 기반으로 스탯 생성
                Debug.Log($"[Character] Stats loaded from SaveFile: Damage {stats.GetTotalDamage()}, HP {stats.GetTotalHP()}, Speed {stats.GetTotalSpeed()}");
            }           
            else
            {
                Debug.LogWarning("[Character] SaveManager not found! Using base stats only.");
                stats = new CharacterStats(characterBlueprint); // fallback
            }
        }

        public virtual void Init(EntityManager entityManager, AbilityManager abilityManager, StatsManager statsManager)
        {
            this.entityManager = entityManager;
            this.abilityManager = abilityManager;
            this.statsManager = statsManager;

            OnDealDamage.AddListener(statsManager.IncreaseDamageDealt);

            coroutineQueue = new CoroutineQueue(this);
            coroutineQueue.StartLoop();

            // ✅ 체력 및 업그레이드 초기화
            currentHealth = stats.GetTotalHP();
            healthBar.Setup(currentHealth, 0, stats.GetTotalHP());

            expBar.Setup(currentExp, 0, nextLevelExp);
            currentLevel = 1;
            UpdateLevelDisplay();

            spriteAnimator.Init(characterBlueprint.walkSpriteSequence, characterBlueprint.walkFrameTime, false);

            movementSpeed = new UpgradeableMovementSpeed();
            movementSpeed.Value = stats.GetTotalSpeed();
            abilityManager.RegisterUpgradeableValue(movementSpeed, true);
            UpdateMoveSpeed();

            armor = new UpgradeableArmor();
            armor.Value = characterBlueprint.armor;
            abilityManager.RegisterUpgradeableValue(armor, true);

            zPositioner.Init(transform);
        }

        public void RecalculateStats()
        {
            var saveManager = FindObjectOfType<SaveManager>();
            if (saveManager != null)
            {
                SaveData saveData = saveManager.LoadGame();
                stats.RecalculateFromSave();
                currentHealth = stats.GetTotalHP();
                healthBar.Setup(currentHealth, 0, stats.GetTotalHP());
                movementSpeed.Value = stats.GetTotalSpeed();
                UpdateMoveSpeed();

                Debug.Log($"[Character] Stats recalculated: Damage {stats.GetTotalDamage()}, HP {stats.GetTotalHP()}, Speed {stats.GetTotalSpeed()}");
            }
            else
            {
                Debug.LogWarning("[Character] SaveManager not found during recalculation.");
            }
        }

        protected virtual void Update()
        {
            lookIndicator.transform.localPosition = lookDirection * lookIndicatorRadius;
            spriteRenderer.flipX = lookDirection.x < 0;
        }

        protected virtual void FixedUpdate()
        {
            if (moveDirection != Vector2.zero)
                lookDirection = moveDirection;
            else
                StopWalkAnimation();

            if (alive)
                rb.velocity += moveDirection * characterBlueprint.acceleration * Time.deltaTime;
        }

        public void GainExp(float exp)
        {
            if (alive)
                coroutineQueue.EnqueueCoroutine(GainExpCoroutine(exp));
        }

        private IEnumerator GainExpCoroutine(float exp)
        {
            if (alive)
            {
                while (currentExp + exp >= nextLevelExp)
                {
                    float expDiff = nextLevelExp - currentExp;
                    currentExp += expDiff;
                    exp -= expDiff;
                    expBar.Setup(currentExp, 0, nextLevelExp);
                    yield return LevelUpCoroutine();
                    float prevLevelExp = nextLevelExp;
                    expToNextLevel += characterBlueprint.LevelToExpIncrease(currentLevel);
                    nextLevelExp += expToNextLevel;
                    expBar.Setup(currentExp, prevLevelExp, nextLevelExp);
                }
                currentExp += exp;
                expBar.AddPoints(exp);
            }
        }

        private IEnumerator LevelUpCoroutine()
        {
            if (alive)
            {
                currentLevel++;
                UpdateLevelDisplay();
                abilitySelectionDialog.Open();
                while (abilitySelectionDialog.MenuOpen)
                {
                    yield return null;
                }
            }
        }

        private void UpdateLevelDisplay()
        {
            levelText.text = "LV " + currentLevel;
        }

        public override void Knockback(Vector2 knockback)
        {
            rb.velocity += knockback * Mathf.Sqrt(rb.drag);
        }

        public override void TakeDamage(float damage, Vector2 knockback = default(Vector2), bool isCritical = false)
        {
            if (alive)
            {
                if (armor.Value >= damage)
                    damage = damage < 1 ? damage : 1;
                else
                    damage -= armor.Value;

                healthBar.SubtractPoints(damage);
                currentHealth -= damage;
                rb.velocity += knockback * Mathf.Sqrt(rb.drag);
                statsManager.IncreaseDamageTaken(damage);

                if (currentHealth <= 0)
                {
                    StartCoroutine(DeathAnimation());
                }
                else
                {
                    if (hitAnimationCoroutine != null) StopCoroutine(hitAnimationCoroutine);
                    hitAnimationCoroutine = StartCoroutine(HitAnimation());
                }
            }
        }

        private IEnumerator HitAnimation()
        {
            spriteRenderer.sharedMaterial = hitMaterial;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.sharedMaterial = defaultMaterial;
        }

        private IEnumerator DeathAnimation()
        {
            alive = false;
            spriteRenderer.sharedMaterial = deathMaterial;

            abilityManager.DestroyActiveAbilities();
            StopWalkAnimation();
            deathParticles.Play();
            float height = spriteRenderer.bounds.size.y;
            float t = 0;
            while (t < 1)
            {
                spriteRenderer.sharedMaterial = deathMaterial;
                deathParticles.transform.position = transform.position + Vector3.up * height * (1 - t);
                deathMaterial.SetFloat("_Wipe", t);
                t += Time.deltaTime;
                yield return null;
            }
            deathMaterial.SetFloat("_Wipe", 1.0f);

            yield return new WaitForSeconds(0.5f);

            OnDeath.Invoke();
            spriteRenderer.enabled = false;
        }

        public void GainHealth(float health)
        {
            healthBar.AddPoints(health);
            currentHealth += health;
            if (currentHealth > stats.GetTotalHP())
                currentHealth = stats.GetTotalHP();
        }

        public void SetLookDirecton(InputAction.CallbackContext context)
        {
            LookDirection = context.ReadValue<Vector2>();
        }

        public void UpdateMoveSpeed()
        {
            rb.drag = characterBlueprint.acceleration / (movementSpeed.Value * movementSpeed.Value);
        }

        public void Move(Vector2 moveDirection)
        {
            this.moveDirection = moveDirection;
        }

        public void StartWalkAnimation()
        {
            if (alive)
                spriteAnimator.StartAnimating();
        }

        public void StopWalkAnimation()
        {
            spriteAnimator.StopAnimating(true);
        }

        public void SetMoveDirection(InputAction.CallbackContext context)
        {
            moveDirection = context.action.ReadValue<Vector2>().normalized;
        }
    }
}
