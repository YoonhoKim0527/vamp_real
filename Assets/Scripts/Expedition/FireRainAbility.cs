using UnityEngine;

namespace Vampire
{
    public class FireRainAbility : BaseExpeditionAbility
    {
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] float spawnHeight = 5f;

        protected override void TriggerAbility()
        {
            Vector3 spawnPosition = boss.position + new Vector3(Random.Range(-2f, 2f), spawnHeight, 0);
            GameObject projGO = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            var proj = projGO.GetComponent<FireRainProjectile>();
            if (proj != null)
                proj.Launch(Vector3.down, baseDamage);
        }
    }
}
