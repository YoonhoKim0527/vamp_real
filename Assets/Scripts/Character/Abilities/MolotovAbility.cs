using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class MolotovAbility : ThrowableAbility
    {
        [Header("Molotov Stats")]
        [SerializeField] protected UpgradeableDuration duration;
        [SerializeField] protected UpgradeableAOE fireRadius;
        [SerializeField] protected UpgradeableDamageRate fireDamageRate;


        protected override void Use()
        {
            base.Use();

            if (CrossSceneData.ExtraProjectile && throwableCount != null)
            {
                throwableCount.ForceAdd(1);
            }
            if (CrossSceneData.BonusProjectile > 0 && throwableCount != null)
            {
                throwableCount.ForceAdd(CrossSceneData.BonusProjectile);
            }
        }
        protected override void LaunchThrowable()
        {
            float totalDamage = playerCharacter.Stats.GetTotalDamage() * damage.Value;
            MolotovThrowable throwable = (MolotovThrowable)entityManager.SpawnThrowable(throwableIndex, playerCharacter.CenterTransform.position, totalDamage, knockback.Value, 0, monsterLayer);
            throwable.SetupFire(duration.Value, fireRadius.Value, fireDamageRate.Value);
            // Throw randomly at nearby enemies
            List<ISpatialHashGridClient> nearbyEnemies = entityManager.Grid.FindNearbyInRadius(playerCharacter.transform.position, throwRadius);

            Vector2 throwPosition;
            if (nearbyEnemies.Count > 0)
                throwPosition = nearbyEnemies[Random.Range(0, nearbyEnemies.Count)].Position;
            else
                throwPosition = (Vector2)playerCharacter.transform.position + Random.insideUnitCircle * throwRadius;
            throwable.Throw(throwPosition);
            throwable.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
        }
    }
}
