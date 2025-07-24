using UnityEngine;

namespace Vampire
{
    public abstract class BaseExpeditionAbility : MonoBehaviour
    {
        [SerializeField] protected float fireInterval = 1f;
        protected float timer;
        protected float baseDamage = 1f;
        protected Transform boss;

        // ✅ GameStateManager 캐싱
        protected GameStateManager gameStateManager;

        protected virtual void Start()
        {
            // ✅ GameStateManager 찾고 캐싱
            gameStateManager = FindObjectOfType<GameStateManager>();
            if (gameStateManager != null)
            {
                // ✅ 곱연산 끝난 공격력 가져오기
                float updatedDamage = gameStateManager.PlayerStats.attackPower;
                SetDamage(updatedDamage);
                Debug.Log($"[BaseExpeditionAbility] Damage set to {baseDamage}");
            }
            else
            {
                Debug.LogWarning("[BaseExpeditionAbility] GameStateManager not found!");
            }
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

        // 각 Ability에서 구현할 실제 행동
        protected abstract void TriggerAbility();
    }
}
