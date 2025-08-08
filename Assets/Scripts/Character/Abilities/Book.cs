using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class Book : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 1;
        private BookAbility bookAbility;
        private LayerMask monsterLayer;

        private float damageMultiplier = 1f;
        private Color? ghostColor = null;
        private bool isGhost = false;

        public void Init(BookAbility bookAbility, LayerMask monsterLayer, float damageMultiplier = 1f, Color? ghostColor = null)
        {
            this.bookAbility = bookAbility;
            this.monsterLayer = monsterLayer;
            this.damageMultiplier = damageMultiplier;
            this.ghostColor = ghostColor;
            this.isGhost = ghostColor.HasValue;

            // 유령 효과 적용
            if (isGhost && TryGetComponent(out SpriteRenderer sr))
                sr.color = ghostColor.Value;
        }

        void Update()
        {
            transform.RotateAround(transform.position, Vector3.back, Time.deltaTime * 100 * rotationSpeed);
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if ((monsterLayer & (1 << collider.gameObject.layer)) != 0)
            {
                IDamageable damageable = collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    float totalDamage = bookAbility.PlayerStats.attackPower * bookAbility.DamageValue.Value * damageMultiplier;

                    bool isCritical = Random.value < bookAbility.PlayerStats.criticalChance;
                    if (isCritical)
                        totalDamage *= (1 + bookAbility.PlayerStats.criticalDamage);

                    Vector2 knockbackDir = (damageable.transform.position - transform.position).normalized;
                    damageable.TakeDamage(totalDamage, bookAbility.Knockback.Value * knockbackDir, isCritical);

                    bookAbility.Character.OnDealDamage.Invoke(totalDamage);
                }
            }
        }
    }
}
