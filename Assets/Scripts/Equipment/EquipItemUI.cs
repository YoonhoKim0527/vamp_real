using UnityEngine;
using UnityEngine.UI;
namespace Vampire
{
    public class EquipItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject glowOutline; // 선택 효과
        [SerializeField] private Image backgroundImage;  // 배경 색 변경용

        private Equipment data;
        private EquipmentManager manager;

        public void Setup(Equipment equipment)
        {
            data = equipment;
            
            if (backgroundImage == null)
            {
                Debug.LogError("[EquipItemUI] backgroundImage가 Inspector에서 연결되지 않았습니다!");
                return;
            }

            if (iconImage == null)
            {
                Debug.LogError("[EquipItemUI] iconImage가 Inspector에서 연결되지 않았습니다!");
                return;
            }

            if (glowOutline == null)
            {
                Debug.LogWarning("[EquipItemUI] glowOutline이 Inspector에 연결 안됨 (선택 강조 불가)");
            }

            // 데이터 주입
            iconImage.sprite = equipment.icon;
            backgroundImage.color = Color.blue; // Tier1 기본 파랑
            SetSelected(false);
        }

        public void Initialize(EquipmentManager mgr)
        {
            manager = mgr;
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            manager.RegisterSelection(this);
        }

        public void SetSelected(bool selected)
        {
            glowOutline.SetActive(selected);
        }

        public void SetBackgroundColor(Color color)
        {
            backgroundImage.color = color;
        }

        public Equipment GetEquipmentData()
        {
            if (data == null)
            {
                Debug.LogError("[EquipItemUI] Equipment 데이터가 null입니다!");
            }
            return data;
        }
    }
}