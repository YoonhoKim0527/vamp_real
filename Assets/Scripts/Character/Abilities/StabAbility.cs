using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class StabAbility : MeleeAbility
    {
        [Header("Stab Stats")]
        [SerializeField] protected float stabOffset;
        [SerializeField] protected float stabDistance;
        [SerializeField] protected float stabTime;
        protected Vector2 weaponSize;
        protected FastList<GameObject> hitMonsters;

        protected override void Use()
        {
            base.Use();
            weaponSize = weaponSpriteRenderer.bounds.size;
            weaponSpriteRenderer.enabled = false;
        }

        protected override void Attack()
        {
            StartCoroutine(Stab(playerCharacter.CenterTransform.position, playerCharacter.LookDirection, isMirror: false));
        }

        protected virtual IEnumerator Stab(Vector2 origin, Vector2 direction, bool isMirror)
        {
            hitMonsters = new FastList<GameObject>();
            timeSinceLastAttack -= stabTime;
            float t = 0;
            weaponSize = weaponSpriteRenderer.bounds.size;
            weaponSpriteRenderer.enabled = true;

            while (t < stabTime)
            {
                Vector2 attackBoxPosition = origin + direction * (weaponSize.x / 2 + stabOffset + stabDistance / stabTime * t);
                float attackAngle = Vector2.SignedAngle(Vector2.right, direction);
                Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackBoxPosition, weaponSize, attackAngle, targetLayer);

                weaponSpriteRenderer.transform.position = attackBoxPosition;
                weaponSpriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, attackAngle);

                foreach (Collider2D collider in hitColliders)
                {
                    if (!hitMonsters.Contains(collider.gameObject))
                    {
                        hitMonsters.Add(collider.gameObject);
                        Monster monster = collider.gameObject.GetComponentInParent<Monster>();

                        float totalDamage = playerStats.attackPower * damage.Value;
                        float totalKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);
                        bool isCritical = false;

                        if (!isMirror && Random.value < playerStats.criticalChance)
                        {
                            totalDamage *= (1 + playerStats.criticalDamage);
                            isCritical = true;
                        }

                        if (isMirror)
                        {
                            totalDamage *= 0.6f;
                            totalKnockback *= 0.5f;
                        }

                        Vector2 knockbackForce = direction * totalKnockback;
                        DamageMonster(monster, totalDamage, knockbackForce, isCritical);

                        if (!isMirror)
                            playerCharacter.OnDealDamage.Invoke(totalDamage);
                    }
                }

                t += Time.deltaTime;
                yield return null;
            }

            Vector2 initialScale = weaponSpriteRenderer.transform.localScale;
            t = 0;
            while (t < 1)
            {
                weaponSpriteRenderer.transform.localPosition = origin
                    + direction * (weaponSpriteRenderer.transform.localScale.x / initialScale.x * weaponSize.x / 2
                                   + stabOffset + stabDistance);
                weaponSpriteRenderer.transform.localScale = Vector2.Lerp(initialScale, Vector2.zero, EasingUtils.EaseInQuart(t));
                t += Time.deltaTime * 4;
                yield return null;
            }

            weaponSpriteRenderer.transform.localScale = initialScale;
            weaponSpriteRenderer.enabled = false;
        }

        protected virtual void DamageMonster(Monster monster, float damage, Vector2 knockback, bool isCritical)
        {
            if (monster != null)
            {
                monster.TakeDamage(damage, knockback, isCritical);
                entityManager.SpawnDamageText(monster.CenterTransform.position, damage, isCritical);
            }
        }

        // 👻 ghost용 MirrorActivate
        public virtual void MirrorActivate(Vector2 spawnPosition, Vector2 direction)
        {
            StartCoroutine(Stab(spawnPosition, direction.normalized, true));
        }
    }
}
