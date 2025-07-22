using UnityEngine;

namespace Vampire
{
    public class GloveAbility : BaseExpeditionAbility
    {
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] float stabDistance = 3f;
        [SerializeField] float stabSpeed = 3f;

        protected override void TriggerAbility()
        {
            Vector3 dir = (boss.position - transform.position).normalized;
            Vector3 targetPos = transform.position + dir * stabDistance;

            GameObject glove = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            var proj = glove.GetComponent<GloveProjectile>();
            if (proj != null)
                proj.Launch(transform.position, targetPos, stabSpeed, baseDamage);
            else
                Debug.LogError("❌ GloveProjectile component missing!");
        }
    }
}
