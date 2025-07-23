using UnityEngine;

namespace Vampire
{
    public class ExpeditionSimpleAbility : BaseExpeditionAbility
    {
        [SerializeField] GameObject projectilePrefab;

        protected override void TriggerAbility()
        {
            if (boss == null) return;

            Vector3 dir = (boss.position - transform.position).normalized;
            if (dir == Vector3.zero) return;

            Vector3 spawnPos = transform.position + dir * 0.5f;
            GameObject projGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            var proj = projGO.GetComponent<ExpeditionProjectile>();
            if (proj != null)
                proj.Launch(dir, baseDamage);
        }
    }
}
