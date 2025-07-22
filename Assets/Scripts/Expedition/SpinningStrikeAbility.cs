using UnityEngine;

namespace Vampire
{
    public class SpinningStrikeAbility : BaseExpeditionAbility
    {
        
            [SerializeField] GameObject projectilePrefab;
            [SerializeField] float spinRadius = 1f;
            [SerializeField] float spinDuration = 2.8f;

            protected override void TriggerAbility()
            {
                if (boss == null) return;

                GameObject go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                var proj = go.GetComponent<SpinningStrikeProjectile>();
                if (proj != null)
                    proj.Launch(transform.position, boss, spinRadius, spinDuration, baseDamage);
                else
                    Debug.LogError("❌ SpinningHomingProjectile component missing!");
            }
    }
}
