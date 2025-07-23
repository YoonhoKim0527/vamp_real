using UnityEngine;
namespace Vampire
{
    [CreateAssetMenu(fileName = "NewEquipmentBlueprint", menuName = "Equipment/Blueprint")]
    public class EquipmentBlueprint : ScriptableObject
    {
        public Equipment[] equipments; // 장비 데이터 배열
    }
}
