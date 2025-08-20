// EquipSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class EquipSlotUI : MonoBehaviour
    {
        [Header("Slot Type")]
        public EquipmentType slotType;            // ✅ EquipmentManager가 참조하는 필드

        [Header("UI")]
        [SerializeField] private Image iconImage; // 슬롯 아이콘
        [SerializeField] private TMP_Text tierText; // "T1" 같은 표시 (선택)

        [Header("Placeholder")]
        [SerializeField] private Sprite emptyIcon; // 비었을 때 아이콘

        /// <summary>
        /// 장착 장비를 슬롯에 표시
        /// </summary>
        public void SetEquipment(Equipment eq)
        {
            if (iconImage != null)
                iconImage.sprite = (eq != null && eq.icon != null) ? eq.icon : emptyIcon;

            if (tierText != null)
                tierText.text = (eq != null) ? $"T{eq.tier}" : "";
        }

        /// <summary>
        /// 비어있는 상태로 표시
        /// </summary>
        public void SetEmpty()
        {
            SetEquipment(null);
        }
        public void OnClickOpen()
        {
            var mgr = EquipmentManager.Instance ?? FindObjectOfType<EquipmentManager>(true);
            if (mgr == null) return;
            mgr.OpenEquipmentPanelForType(slotType);
        }
    }
}
