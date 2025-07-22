using UnityEngine;
using System;

namespace Vampire
{
    public class ExpeditionBoss : MonoBehaviour
    {
        float currentHp;
        ExpeditionBossBlueprint blueprint;

        public event Action OnDeath;

        public void Initialize(ExpeditionBossBlueprint blueprint)
        {
            this.blueprint = blueprint;
            currentHp = blueprint.hp;

            ExpeditionUIManager.Instance?.InitBossUI(blueprint.stageName, blueprint.hp);

            var animator = GetComponent<SpriteAnimator>();
            if (animator != null)
            {
                animator.Init(blueprint.breatheAnimation, blueprint.frameTime);
                animator.StartAnimating();
            }
            else
            {
                Debug.LogWarning("[ExpeditionBoss] SpriteAnimator not found on boss prefab!");
            }
        }

        public void TakeDamage(float damage)
        {
            currentHp -= damage;
            currentHp = Mathf.Max(0f, currentHp);

            ExpeditionUIManager.Instance?.UpdateBossHP(currentHp);

            if (currentHp <= 0f)
                Die();
        }

        void Die()
        {
            Debug.Log($"Boss defeated! Reward: {blueprint.rewardGold} Gold, {blueprint.rewardExp} EXP");
            OnDeath?.Invoke(); // ✅ 이벤트 호출
            Destroy(gameObject);
        }
    }
}
