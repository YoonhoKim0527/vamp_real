using UnityEngine;
namespace Vampire
{
    [System.Serializable]
    public class Equipment
    {
        public string name;
        public Sprite icon;
        public int tier;
        public EquipmentType type;
        public float multiply = 1.0f; // ✅ 기본값 1.0 (효과 없음)
    }
}
