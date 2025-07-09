using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class GunAbility : ProjectileAbility
    {
        [Header("Gun Stats")]
        [SerializeField] protected UpgradeableProjectileCount projectileCount;
        [SerializeField] protected UpgradeableDamageRate firerate;

        protected override void Attack()
        {
            StartCoroutine(FireClip());
        }

        protected override void Use()
        {
            base.Use();

            if (CrossSceneData.ExtraProjectile && projectileCount != null)
            {
                projectileCount.ForceAdd(1);
            }
            if (CrossSceneData.BonusProjectile > 0 && projectileCount != null)
            {
                projectileCount.ForceAdd(CrossSceneData.BonusProjectile);
            }
        }
        protected IEnumerator FireClip()
        {
            int clipSize = projectileCount.Value;
            timeSinceLastAttack -= clipSize / firerate.Value;
            for (int i = 0; i < clipSize; i++)
            {
                LaunchProjectile();
                yield return new WaitForSeconds(1 / firerate.Value);
            }
        }
    }
}
