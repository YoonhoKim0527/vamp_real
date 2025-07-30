using UnityEngine;
using System.Collections.Generic;

namespace Vampire
{
    [CreateAssetMenu(fileName = "NewShopEquipmentBlueprint", menuName = "Equipment/Shop Blueprint")]
    public class ShopEquipmentBlueprint : ScriptableObject
    {
        public List<ShopEquipment> equipments;
    }

    [System.Serializable]
    public class ShopEquipment
    {
        public string name;
        public Sprite icon;
        public EquipmentType type;
        public float multiply = 1.0f;
    }
}