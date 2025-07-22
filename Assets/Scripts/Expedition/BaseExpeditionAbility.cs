using UnityEngine;

namespace Vampire
{
    public abstract class BaseExpeditionAbility : MonoBehaviour
    {
        [SerializeField] protected float fireInterval = 1f;
        protected float timer;
        protected float baseDamage = 1f;
        protected Transform boss;

        public virtual void SetDamage(float damage)
        {
            baseDamage = damage;
        }

        public virtual void SetBoss(Transform bossTransform)
        {
            boss = bossTransform;
        }

        protected virtual void Update()
        {
            timer += Time.deltaTime;
            if (timer >= fireInterval && boss != null)
            {
                timer = 0f;
                TriggerAbility();
            }
        }

        // 각 Ability에서 구현할 실제 행동
        protected abstract void TriggerAbility();
    }
}
