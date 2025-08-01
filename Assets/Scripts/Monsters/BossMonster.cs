using System.Collections;
using System.Linq;
using UnityEngine;

namespace Vampire
{
    public class BossMonster : Monster
    {
        protected new BossMonsterBlueprint monsterBlueprint;
        protected BossAbility[] abilities;
        protected Coroutine act = null;

        // 추가된 필드
        protected float moveSpeed; // 직접 선언함

        public Rigidbody2D Rigidbody => rb;
        public SpriteAnimator Animator => monsterSpriteAnimator;
        protected float timeSinceLastMeleeAttack;

        // maxHealth는 Monster.cs 내부에서 관리되므로 그대로 사용 가능
        public float HealthPercent => (float)currentHealth / maxHealth;
        public bool IsDead => currentHealth <= 0;

        public event System.Action OnDeath;

        public BossMonsterBlueprint Blueprint => monsterBlueprint;

        public bool IsSplit { get; private set; } = false;

        public Character PlayerCharacter => playerCharacter;

        public void MarkAsSplit()
        {
            IsSplit = true;
        }

        public override void Setup(int monsterIndex, Vector2 position, MonsterBlueprint monsterBlueprint, float hpBuff = 0)
        {
            base.Setup(monsterIndex, position, monsterBlueprint, hpBuff);
<<<<<<< HEAD
            this.monsterBlueprint = (BossMonsterBlueprint) monsterBlueprint;
=======
            this.monsterBlueprint = (BossMonsterBlueprint)monsterBlueprint;
>>>>>>> 4c39b7bfb6aa3a0732f898c832b745875a59236c
            abilities = new BossAbility[this.monsterBlueprint.abilityPrefabs.Length];
            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i] = Instantiate(this.monsterBlueprint.abilityPrefabs[i], transform).GetComponent<BossAbility>();
                abilities[i].Init(this, entityManager, playerCharacter);
            }

            act = StartCoroutine(Act());
        }

        protected override void Update()
        {
            base.Update();
            timeSinceLastMeleeAttack += Time.deltaTime;

            // 체력 50% 이하 시 자동 분열 트리거
            foreach (var ability in abilities)
            {
                if (ability is SplitBossAbility splitAbility && splitAbility.Score() > 0.99f)
                {
                    StartCoroutine(splitAbility.Activate());
                    break;
                }
            }
        }

        public void Move(Vector2 direction, float deltaTime)
        {
            rb.velocity += direction * monsterBlueprint.acceleration * deltaTime;
        }

        public void Freeze()
        {
            rb.velocity = Vector2.zero;
        }

        private IEnumerator Act()
        {
            while (true)
            {
                float[] abilityScores = abilities.Select(a => a.Score()).ToArray();
                float totalScore = abilityScores.Sum();

                if (totalScore == 0f)
                {
                    yield return new WaitForSeconds(1);
                    continue;
                }

                float rand = Random.Range(0f, totalScore);
                float cumulative = 0;
                int abilityIndex = -1;

                for (int i = 0; i < abilities.Length; i++)
                {
                    abilities[i].Deactivate();
                    cumulative += abilityScores[i];
                    if (abilityIndex == -1 && rand < cumulative)
                        abilityIndex = i;
                }

                if (abilityIndex == -1)
                    yield return new WaitForSeconds(1);
                else
                    yield return abilities[abilityIndex].Activate();
            }
        }

        protected override void DropLoot()
        {
            base.DropLoot();
            if (monsterBlueprint.chestBlueprint != null)
                entityManager.SpawnChest(monsterBlueprint.chestBlueprint, transform.position);
        }

        public override IEnumerator Killed(bool killedByPlayer = true)
        {
            if (IsSplit)
            {
                foreach (BossAbility ability in abilities)
                    Destroy(ability.gameObject);

                if (act != null)
                    StopCoroutine(act);

                OnDeath?.Invoke();

                yield return base.Killed(killedByPlayer); // ⚠️ LivingMonster, Sprite 제거 포함
                yield break;
            }

            if (TryGetComponent<SplitTracker>(out var tracker))
            {
                yield break;
            }

            foreach (BossAbility ability in abilities)
                Destroy(ability.gameObject);

            if (act != null)
                StopCoroutine(act);

            OnDeath?.Invoke();
            yield return base.Killed(killedByPlayer);
        }




        public void CopyStatsFrom(BossMonster original, float ratio)
        {
            maxHealth = Mathf.RoundToInt(original.maxHealth * ratio);
            currentHealth = maxHealth;
            moveSpeed = original.moveSpeed * ratio;
            monsterBlueprint = original.monsterBlueprint; // Blueprint 공유
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if ((monsterBlueprint.meleeLayer & (1 << col.collider.gameObject.layer)) != 0)
            {
                IDamageable damageable = col.collider.GetComponentInParent<IDamageable>();
                Vector2 knockbackDirection = (damageable.transform.position - transform.position).normalized;

                if (timeSinceLastMeleeAttack > monsterBlueprint.meleeAttackDelay)
                {
                    damageable.TakeDamage(monsterBlueprint.meleeDamage, monsterBlueprint.meleeKnockback * knockbackDirection);
                    timeSinceLastMeleeAttack = 0;
                }
                else
                {
                    damageable.TakeDamage(0, monsterBlueprint.meleeKnockback * knockbackDirection);
                }
            }

            if (col.gameObject.TryGetComponent<Chest>(out Chest chest))
            {
                chest.OpenChest(false);
            }
        }
    }
}
