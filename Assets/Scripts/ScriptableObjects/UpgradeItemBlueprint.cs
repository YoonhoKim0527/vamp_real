using System.Collections.Generic;
using UnityEngine;
namespace Vampire

{
    [CreateAssetMenu(fileName = "UpgradeItem", menuName = "Blueprints/UpgradeItem", order = 2)]
    public class UpgradeItemBlueprint : ScriptableObject
    {
        public string upgradeName;
        public string description;
        public Sprite upgradeSprite;
        public int cost;
        public int level = 0;
        public int maxLevel = 5;
        public UpgradeType type =  UpgradeType.None;
    }

    public enum UpgradeType
    {
        None,
        ProjectileUpgrade,
        DamageUpgrade,
        HPUpgrade,
        SpeedUpgrade
    }
}