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
        public bool purchased = false;

        public UpgradeType type;
    }

    public enum UpgradeType
    {
        None,
        DamageUp,
        MaxHPUp,
        MoveSpeedUp,
        ProjectilePlus
    }
}