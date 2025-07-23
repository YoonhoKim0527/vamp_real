using UnityEngine;

namespace Vampire
{
    public class BombAbility : BaseExpeditionAbility
    {
        [SerializeField] GameObject projectilePrefab;

        protected override void TriggerAbility()
        {
            if (boss == null || projectilePrefab == null)
            {
                Debug.LogWarning("❌ BombAbility cannot fire. Missing boss or prefab.");
                return;
            }

            Vector3 targetPos = boss.position;
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

            GameObject bombGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            var bomb = bombGO.GetComponent<BombProjectile>();
            if (bomb != null)
                bomb.Launch(targetPos, baseDamage);
            else
                Debug.LogError("❌ BombProjectile component missing!");
        }
    }
}
