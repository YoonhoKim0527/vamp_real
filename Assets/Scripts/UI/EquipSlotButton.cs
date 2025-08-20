// EquipSlotButton.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class EquipSlotButton : MonoBehaviour
    {
        public EquipmentType slotType;

        [Header("UI")]
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text tierText;
        [SerializeField] Sprite emptyIcon;

        Equipment current;

        public void SetEquipment(Equipment eq)
        {
            current = eq;
            if (iconImage != null)
                iconImage.sprite = (eq != null && eq.icon != null) ? eq.icon : emptyIcon;

            if (tierText != null)
                tierText.text = (eq != null) ? $"T{eq.tier}" : "";
        }

        public void SetEmpty()
        {
            SetEquipment(null);
        }

        // 버튼 OnClick에 바인딩
        public void OnClickOpenEquipment()
        {
            var mgr = FindObjectOfType<EquipmentManager>();
            mgr?.OpenEquipmentPanelForType(slotType);
        }
    }
}
