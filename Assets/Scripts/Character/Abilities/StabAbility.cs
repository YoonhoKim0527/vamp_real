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
            StartCoroutine(Stab());
        }

        protected virtual IEnumerator Stab()
        {
            hitMonsters = new FastList<GameObject>();
            timeSinceLastAttack -= stabTime;
            float t = 0;
            weaponSpriteRenderer.enabled = true;
            Vector2 dir = playerCharacter.LookDirection;

            while (t < stabTime)
            {
                Vector2 attackBoxPosition = (Vector2)playerCharacter.CenterTransform.position
                                            + dir * (weaponSize.x / 2 + stabOffset + stabDistance / stabTime * t);
                float attackAngle = Vector2.SignedAngle(Vector2.right, dir);
                Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackBoxPosition, weaponSize, attackAngle, targetLayer);

                weaponSpriteRenderer.transform.position = attackBoxPosition;
                weaponSpriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, attackAngle);

                foreach (Collider2D collider in hitColliders)
                {
                    if (!hitMonsters.Contains(collider.gameObject))
                    {
                        hitMonsters.Add(collider.gameObject);
                        Monster monster = collider.gameObject.GetComponentInParent<Monster>();

                        // ✅ CharacterStatBlueprint 기반 데미지 계산
                        float totalDamage = playerStats.attackPower * damage.Value;

                        // ✅ 치명타 여부 계산
                        bool isCritical = false;
                        if (Random.value < playerStats.criticalChance)
                        {
                            totalDamage *= (1 + playerStats.criticalDamage);
                            isCritical = true;
                            Debug.Log("💥 [StabAbility] Critical hit!");
                        }

                        // ✅ 넉백 계산
                        Vector2 knockbackForce = dir * knockback.Value * (1 + playerStats.defense * 0.1f);

                        // ✅ 데미지 처리 (치명타 포함)
                        DamageMonster(monster, totalDamage, knockbackForce, isCritical);

                        // ✅ 플레이어의 총 데미지 이벤트 호출
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
                weaponSpriteRenderer.transform.localPosition = (Vector2)playerCharacter.CenterTransform.position
                    + dir * (weaponSpriteRenderer.transform.localScale.x / initialScale.x * weaponSize.x / 2
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
    }
}
