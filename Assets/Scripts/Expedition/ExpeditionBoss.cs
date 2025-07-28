using UnityEngine;
using System;

namespace Vampire
{
    public class ExpeditionBoss : MonoBehaviour
    {
        const int BarCount = 1000;

        float currentHp;
        float maxHp;
        float hpPerBar;

        ExpeditionBossBlueprint blueprint;
        ExpeditionEntityManager entityManager;

        public event Action OnDeath;
        public float HP => currentHp;
        bool isFrozen;
        float freezeTimer;
        SpriteRenderer spriteRenderer;
        float damageAccumulated = 0f;

        public void Initialize(ExpeditionBossBlueprint blueprint, ExpeditionEntityManager entityManager)
        {
            this.blueprint = blueprint;
            this.entityManager = entityManager;

            maxHp = blueprint.hp;
            currentHp = maxHp;
            hpPerBar = maxHp / BarCount;
            

            ExpeditionUIManager.Instance?.InitBossUI(blueprint.stageName);
            UpdateHpUI();

            spriteRenderer = GetComponent<SpriteRenderer>();
            var animator = GetComponent<SpriteAnimator>();

            if (animator != null)
            {
                animator.Init(blueprint.breatheAnimation, blueprint.frameTime);
                animator.StartAnimating();
            }
        }
        public void SetHP(float hp)
        {
            currentHp = Mathf.Clamp(hp, 0f, maxHp);
            UpdateHpUI();
        }

        public void TakeDamage(float damage)
        {
            float multiplier = BoostManager.Instance != null
                ? BoostManager.Instance.GetMultiplier(BoostType.Damage)
                : 1f;

            damage *= multiplier;

            if (isFrozen)
                damage *= 2f;

            currentHp -= damage;
            currentHp = Mathf.Max(0f, currentHp);

            UpdateHpUI();

            if (entityManager != null)
            {
                Vector2 spawnPos = transform.position + Vector3.up * 2;
                entityManager.SpawnDamageText(spawnPos, damage);
            }
            damageAccumulated += damage;

            if (damageAccumulated >= 100f)
            {
                int coinsToAdd = Mathf.FloorToInt(damageAccumulated / 100f);
                CoinManager.Instance.AddCoins(coinsToAdd);
                damageAccumulated -= coinsToAdd * 100f;
            }
            if (currentHp <= 0f)
                Die();
        }

        void UpdateHpUI()
        {
            int currentBar = Mathf.FloorToInt(currentHp / hpPerBar);
            float currentBarFill = (currentHp % hpPerBar) / hpPerBar;

            ExpeditionUIManager.Instance?.UpdateBossBar(currentBar, currentBarFill);
        }

        void Die()
        {
            var saveManager = FindObjectOfType<SaveManager>();
            var upgradeData = saveManager?.GetExpeditionUpgradeData();

            float goldMultiplier = 1f + 0.1f * upgradeData.goldGainLevel;
            float emeraldMultiplier = 1f + 0.1f * upgradeData.emeraldGainLevel;

            int goldReward = Mathf.RoundToInt(blueprint.rewardGold * goldMultiplier);
            int emeraldReward = Mathf.RoundToInt(blueprint.rewardEmerald * emeraldMultiplier);

            Debug.Log($"Boss defeated! Reward: {goldReward} Gold, {emeraldReward} Emerald");

            // ✅ 보상 상자 드롭
            if (blueprint.rewardChestPrefab != null)
            {
                Vector3 chestPos = transform.position + new Vector3(-1f, -0.5f, 0f);
                var chest = Instantiate(blueprint.rewardChestPrefab, chestPos, Quaternion.identity);

                var reward = chest.GetComponent<ExpeditionChestReward>();
                if (reward != null)
                    reward.Init(goldReward, emeraldReward); // ← 보스에서 직접 넘겨줌
            }

            OnDeath?.Invoke();
            Destroy(gameObject);
        }

        public void Freeze(float duration)
        {
            if (isFrozen) return;

            isFrozen = true;
            freezeTimer = duration;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.color = Color.cyan;
        }

        void Update()
        {
            if (isFrozen)
            {
                freezeTimer -= Time.deltaTime;
                if (freezeTimer <= 0f)
                    Unfreeze();
            }
        }

        void Unfreeze()
        {
            isFrozen = false;
            spriteRenderer.color = Color.white;
        }
    }
}
  