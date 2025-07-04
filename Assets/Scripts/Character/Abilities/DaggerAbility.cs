using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class DaggerAbility : StabAbility
    {
        [Header("Dagger Stats")]
        [SerializeField] protected UpgradeableBleedDamage bleedDamage;
        [SerializeField] protected UpgradeableBleedRate bleedRate;
        [SerializeField] protected UpgradeableBleedDuration bleedDuration;

        protected override void DamageMonster(Monster monster, float damage, Vector2 knockback)
        {
            float totalDamage = playerCharacter.Stats.GetTotalDamage() * damage;
            base.DamageMonster(monster, totalDamage, knockback);

            Coroutine monsterBleed = StartCoroutine(BleedMonster(monster));
            monster.OnKilled.AddListener(delegate { StopMonsterBleed(monsterBleed); });
        }

        protected IEnumerator BleedMonster(Monster monster)
        {
            float bleedDelay = 1 / bleedRate.Value;
            int bleedCount = Mathf.RoundToInt(bleedDuration.Value * bleedRate.Value);
            float totalDamage = playerCharacter.Stats.GetTotalDamage() * bleedDamage.Value;

            for (int i = 0; i < bleedCount; i++)
            {
                yield return new WaitForSeconds(bleedDelay);
                base.DamageMonster(monster, totalDamage, Vector2.zero);
                playerCharacter.OnDealDamage.Invoke(totalDamage);

                if (monster.HP <= 0)
                    break;
            }
        }

        protected void StopMonsterBleed(Coroutine monsterBleed)
        {
            StopCoroutine(monsterBleed);
        }
    }
}
