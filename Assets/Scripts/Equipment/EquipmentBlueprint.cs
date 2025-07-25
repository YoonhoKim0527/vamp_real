using UnityEngine;
using System.Collections.Generic;

namespace Vampire
{
    [CreateAssetMenu(fileName = "NewEquipmentBlueprint", menuName = "Equipment/Blueprint")]
    public class EquipmentBlueprint : ScriptableObject
    {
        public List<Equipment> equipments; // ✅ 배열 → 리스트로 변경
    }
}