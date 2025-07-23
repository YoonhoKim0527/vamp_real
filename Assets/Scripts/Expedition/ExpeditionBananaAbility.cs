using UnityEngine;

namespace Vampire
{
    public class ExpeditionBananaAbility : BaseExpeditionAbility
    {
        [SerializeField] GameObject projectilePrefab;

        protected override void TriggerAbility()
        {
            var projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            var banana = projObj.GetComponent<ExpeditionBananaProjectile>();
            banana.Init(boss, transform, baseDamage);
        }
    }
}
