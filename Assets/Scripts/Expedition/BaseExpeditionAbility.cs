using UnityEngine;

namespace Vampire
{
    public abstract class BaseExpeditionAbility : MonoBehaviour
    {
        [SerializeField] protected float fireInterval = 1f;
        protected float initialFireInterval = 1f;
        protected float timer;
        protected float baseDamage = 1f;
        protected Transform boss;

        // ✅ GameStateManager 캐싱
        protected GameStateManager gameStateManager;
        SaveManager saveManager;
        protected virtual void Start()
        {
            // ✅ GameStateManager 찾고 캐싱
            gameStateManager = FindObjectOfType<GameStateManager>();
            saveManager = FindAnyObjectByType<SaveManager>();
            RefreshStats(); 
        }

        public virtual void SetDamage(float damage)
        {
            baseDamage = damage;
        }

        public virtual void SetBoss(Transform bossTransform)
        {
            boss = bossTransform;
        }

        public void SetFireInterval(float interval)
        {
            initialFireInterval = interval;
            fireInterval = interval;
        }

        protected virtual void Update()
        {
            float speedMultiplier = BoostManager.Instance != null
                ? BoostManager.Instance.GetMultiplier(BoostType.AttackSpeed)
                : 1f;

            float currentInterval = fireInterval / speedMultiplier;

            timer += Time.deltaTime;

            if (timer >= currentInterval && boss != null)
            {
                timer = 0f;
                TriggerAbility();
            }
        }

        protected abstract void TriggerAbility();
        public void RefreshStats()
        {
            if (gameStateManager == null) gameStateManager = FindObjectOfType<GameStateManager>();
            if (saveManager == null) saveManager = FindObjectOfType<SaveManager>();
            if (gameStateManager == null || saveManager == null)
                return;

            var upgradeData = saveManager.GetExpeditionUpgradeData();

            // ✅ 데미지
            float damageMultiplier = 1f + 0.1f * upgradeData.damageLevel;
            float updatedDamage = gameStateManager.PlayerStats.attackPower * damageMultiplier;
            SetDamage(updatedDamage);

            // ✅ 주기
            float intervalMultiplier = Mathf.Clamp(1f - 0.01f * upgradeData.intervalLevel, 0.5f, 1f);
            fireInterval = initialFireInterval * intervalMultiplier;

            Debug.Log($"[RefreshStats] Damage: {updatedDamage}, Interval: {fireInterval}");
        }
    }
}
