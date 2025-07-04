using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class GrenadeThrowableAbility : ThrowableAbility
    {
        [Header("Grenade Stats")]
        [SerializeField] protected UpgradeableProjectileCount fragmentCount;

        protected override void LaunchThrowable()
        {
            float totalDamage = playerCharacter.Stats.GetTotalDamage() * damage.Value;
            GrenadeThrowable throwable = (GrenadeThrowable) entityManager.SpawnThrowable(throwableIndex, playerCharacter.CenterTransform.position, totalDamage, knockback.Value, 0, monsterLayer);
            throwable.SetupGrenade(fragmentCount.Value);
            throwable.Throw((Vector2)playerCharacter.transform.position + Random.insideUnitCircle * throwRadius);
            throwable.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
        }
    }
}
