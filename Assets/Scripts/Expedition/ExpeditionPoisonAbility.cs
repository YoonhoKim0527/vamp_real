using UnityEngine;

namespace Vampire
{
    public class ExpeditionPoisonAbility : BaseExpeditionAbility
    {
        [SerializeField] GameObject projectilePrefab;

        protected override void TriggerAbility()
        {
            if (boss == null) return;

            Vector3 spawnPos = boss.position + new Vector3(0f, -1.5f, 0f);

            var zoneObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            var zone = zoneObj.GetComponent<ExpeditionPoisonZone>();
            zone?.Init(boss, baseDamage); // ✅ baseDamage 전달
        }
    }
}
