using UnityEngine;

namespace Vampire
{
    public class PoisonTrailAbility : Ability
    {
        [Header("Poison Trail Settings")]
        [SerializeField] GameObject poisonCloudPrefab;
        [SerializeField] UpgradeableDamage damage;
        [SerializeField] UpgradeableAOE radius;
        [SerializeField] UpgradeableDuration duration;
        [SerializeField] float dropInterval = 0.5f;
        [SerializeField] LayerMask monsterLayer;

        float timeSinceLastDrop;
        int cloudPoolIndex;

        protected override void Use()
        {
            base.Use();
            timeSinceLastDrop = dropInterval;
            gameObject.SetActive(true);

            // 기존의 AddPoolForProjectile은 ❌
            cloudPoolIndex = entityManager.AddPoolForPoisonCloud(poisonCloudPrefab);
        }

        void SpawnPoisonCloud()
        {
            Vector3 pos = playerCharacter.CenterTransform.position;

            // Projectile 사용 ❌
            PoisonCloud pc = entityManager.SpawnPoisonCloud(cloudPoolIndex, pos);
            pc.Init(damage.Value, radius.Value, duration.Value, monsterLayer, playerCharacter, playerCharacter);
        }

        void Update()
        {
            timeSinceLastDrop += Time.deltaTime;
            if (timeSinceLastDrop >= dropInterval)
            {
                timeSinceLastDrop = 0f;
                SpawnPoisonCloud();
            }
        }
    }
}