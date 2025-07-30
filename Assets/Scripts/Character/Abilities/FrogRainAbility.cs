using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class FrogRainAbility : ThrowableAbility
    {
        [SerializeField] float minInterval = 1f;
        [SerializeField] float maxInterval = 3f;
        [SerializeField] float spawnHeight = 8f;
        [SerializeField] float horizontalRange = 6f;

        protected override void Use()
        {
            base.Use();
            StartCoroutine(SpawnFrogRain());
        }

        IEnumerator SpawnFrogRain()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

                Vector2 spawnPos = playerCharacter.transform.position;
                spawnPos += new Vector2(Random.Range(-horizontalRange, horizontalRange), spawnHeight);

                float totalDamage = playerStats.attackPower * damage.Value;
                bool isCritical = Random.value < playerStats.criticalChance;
                if (isCritical) totalDamage *= (1 + playerStats.criticalDamage);
                float effectiveKnockback = knockback.Value * (1 + playerStats.defense * 0.1f);

                Throwable frog = entityManager.SpawnThrowable(
                    throwableIndex,
                    spawnPos,
                    totalDamage,
                    effectiveKnockback,
                    0,
                    monsterLayer
                );

                frog.Throw(spawnPos + Vector2.down * 2f, isCritical);
                frog.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
            }
        }
    }
}
