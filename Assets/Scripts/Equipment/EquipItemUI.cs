using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class EquipItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject glowOutline;
        [SerializeField] private Image backgroundImage;

        [SerializeField] private GameObject equipButtonPrefab; // 🔧 Inspector 연결
        [SerializeField] private Sprite defaultBackgroundSprite;
        [SerializeField] private Sprite equippedBackgroundSprite;

        private GameObject equipButtonInstance;
        private bool isEquipped = false;
        private Equipment data;
        private EquipmentManager manager;

        public void Setup(Equipment equipment)
        {
            data = equipment;

            if (iconImage == null || backgroundImage == null)
            {
                Debug.LogError("[EquipItemUI] Image가 연결 안됨");
                return;
            }

            iconImage.sprite = equipment.icon;
            backgroundImage.sprite = defaultBackgroundSprite;
            SetSelected(false);
            SetEquipped(false); // 초기화
        }

        public void Initialize(EquipmentManager mgr)
        {
            manager = mgr;
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (manager.IsInEquipMode())
            {
                manager.ShowEquipButton(this); // 장착 버튼 표시
            }
            else
            {
                manager.RegisterSelection(this); // 합성용 선택
            }
        }

        public void ShowEquipButton()
        {
            if (equipButtonInstance != null) return;

            equipButtonInstance = Instantiate(equipButtonPrefab, transform);
            RectTransform rt = equipButtonInstance.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-10f, -10f);

            Button btn = equipButtonInstance.GetComponent<Button>();
            btn.onClick.AddListener(OnEquipClicked);
        }

        public void HideEquipButton()
        {
            if (equipButtonInstance != null)
            {
                Destroy(equipButtonInstance);
                equipButtonInstance = null;
            }
        }

        public void SetEquipped(bool equipped)
        {
            isEquipped = equipped;

            backgroundImage.sprite = equipped ? equippedBackgroundSprite : defaultBackgroundSprite;

            if (equipped)
            {
                // 체크 배경일 때만 앞으로 보내기
                backgroundImage.transform.SetAsLastSibling();
            }
            else
            {
                // 미장착일 때는 뒤로 보내기 (예: Glow보다 아래로)
                backgroundImage.transform.SetSiblingIndex(0); // 가장 뒤로 보내기
            }
        }

        public void SetSelected(bool selected)
        {
            glowOutline?.SetActive(selected);
        }

        public Equipment GetEquipmentData() => data;
        public EquipmentType GetEquipmentType() => data.type;

        public void SetBackgroundColor(Color color)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
        }

        private void OnEquipClicked()
        {
            manager.RegisterEquip(this);
            SetSelected(false);
            HideEquipButton(); // ✅ 장착 후 버튼 제거

            // ✅ 현재 activeEquipButtonTarget도 null로 초기화
            manager.ClearActiveEquipButton(this);
        }
    }
}
