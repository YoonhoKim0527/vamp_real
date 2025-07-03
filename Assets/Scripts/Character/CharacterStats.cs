using UnityEngine;
using Vampire;

namespace Vampire
{
    public class CharacterStats
    {
        float baseDamage;
        float upgradeMultiplier = 1f;
        float shopMultiplier = 1f;

        public CharacterStats(CharacterBlueprint blueprint)
        {
            baseDamage = blueprint.baseDamage;

            if (CrossSceneData.ExtraDamage)
                shopMultiplier *= 1.2f;

            Debug.Log($"[Stats Init] Base: {baseDamage}, Shop: x{shopMultiplier}");
        }

        public void ApplyUpgradeMultiplier(float value)
        {
            upgradeMultiplier = 1 + 0.1f * CrossSceneData.BonusDamage;
            Debug.Log($"[Upgrade] New upgradeMultiplier: x{upgradeMultiplier}");
        }

        public float GetTotalDamage()
        {
            return baseDamage * (upgradeMultiplier + shopMultiplier -1);
        }
    }
}