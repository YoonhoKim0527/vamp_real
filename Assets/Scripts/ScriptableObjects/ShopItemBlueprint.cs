using System.Collections.Generic;
using UnityEngine;
namespace Vampire
{
    [CreateAssetMenu(fileName = "Item", menuName = "Blueprints/Item", order = 1)]
    public class ShopItemBlueprint : ScriptableObject
    {
        public ShopItemType type = ShopItemType.None;
        public new string itemName;
        public Sprite itemSprite;
        public int cost;
        public new string description;
        public bool owned = false;
    }

    public enum ShopItemType
    {
        None,
        ProjectileUpgrade,
        DamageUpgrade,
        HPUpgrade,
        SpeedUpgrade
    }
}