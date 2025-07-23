using UnityEngine;

namespace Vampire
{
    public class ExpeditionIceAbility : BaseExpeditionAbility
    {
        [SerializeField] GameObject projectilePrefab;

        protected override void TriggerAbility()
        {
            if (boss == null) return;

            var go = Instantiate(projectilePrefab);
            var proj = go.GetComponent<ExpeditionIceProjectile>();
            proj.Init(transform.position, boss, baseDamage);
        }
    }
}
