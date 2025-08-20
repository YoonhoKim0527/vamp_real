// CharacterSelectionEquipBar.cs
using UnityEngine;
using System.Collections.Generic;

namespace Vampire
{
    public class CharacterSelectionEquipBar : MonoBehaviour
    {
        [SerializeField] EquipSlotButton[] slots; // 인스펙터에서 5개 연결

        EquipmentManager equipMgr;

        void OnEnable()
        {
            equipMgr = FindObjectOfType<EquipmentManager>(true);
            if (equipMgr == null)
            {
                Debug.LogWarning("[CharacterSelectionEquipBar] EquipmentManager not found.");
                return;
            }

            // 장착 변경 이벤트 구독
            equipMgr.EquippedChanged += OnEquippedChanged;

            // 처음 진입 시, 현재 장착 상태로 슬롯들 초기화
            InitFromCurrentEquip();
        }

        void OnDisable()
        {
            if (equipMgr != null)
                equipMgr.EquippedChanged -= OnEquippedChanged;
        }

        void InitFromCurrentEquip()
        {
            // EquipmentManager에서 현재 장착 데이터(Equipment) 얻기
            var map = new Dictionary<EquipmentType, Equipment>();
            foreach (var ui in equipMgr.GetEquippedItems())
            {
                if (ui == null) continue;
                var eq = ui.GetEquipmentData();
                map[eq.type] = eq;
            }

            foreach (var s in slots)
            {
                if (s == null) continue;
                map.TryGetValue(s.slotType, out var eq);
                if (eq != null) s.SetEquipment(eq);
                else s.SetEmpty();
            }
        }

        void OnEquippedChanged(EquipmentType type, Equipment eq)
        {
            // 해당 타입 슬롯만 갱신
            foreach (var s in slots)
            {
                if (s != null && s.slotType == type)
                {
                    if (eq != null) s.SetEquipment(eq);
                    else s.SetEmpty();
                    break;
                }
            }
        }
    }
}
