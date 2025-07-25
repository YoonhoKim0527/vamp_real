using UnityEngine;
namespace Vampire
{
    [System.Serializable]
    public class Equipment
    {
        public string name;        // 장비 이름
        public Sprite icon;        // 장비 아이콘
        public int tier;           // 단계 (1, 2, 3...)
        public EquipmentType type;  // ✅ 추가
    }
}
