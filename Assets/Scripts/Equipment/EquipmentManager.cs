using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Vampire
{
    public class EquipmentManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private Transform gridParent; // GridLayoutGroup이 붙은 오브젝트
        [SerializeField] private GameObject equipItemPrefab; // 장비 아이템 프리팹

        [Header("Equipment Blueprint")]
        [SerializeField] private EquipmentBlueprint blueprint; // ✅ ScriptableObject 연결

        [SerializeField] private Button aggButton;   // AGG 버튼
        [SerializeField] private Button autoAggButton; // AutoAGG 버튼

        private List<EquipItemUI> selectedItems = new List<EquipItemUI>();
        private bool isAutoAggRunning = false;

        private void Start()
        {
            PopulateGrid();
            aggButton.onClick.AddListener(OnAggButtonClick);
            autoAggButton.onClick.AddListener(StartAutoAgg);
        }

        private void PopulateGrid()
        {
            foreach (Transform child in gridParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var equip in blueprint.equipments)
            {
                GameObject item = Instantiate(equipItemPrefab, gridParent);

                RectTransform rt = item.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(20, 20);

                EquipItemUI itemUI = item.GetComponent<EquipItemUI>();
                itemUI.Setup(equip);
                itemUI.Initialize(this);
            }
        }

        public void RegisterSelection(EquipItemUI itemUI)
        {
            if (selectedItems.Contains(itemUI))
            {
                selectedItems.Remove(itemUI);
                itemUI.SetSelected(false);
            }
            else
            {
                if (selectedItems.Count < 2)
                {
                    selectedItems.Add(itemUI);
                    itemUI.SetSelected(true);
                }
                else
                {
                    Debug.Log("2개까지만 선택할 수 있습니다.");
                }
            }
        }

        public void OnAggButtonClick()
        {
            TryAgg(selectedItems[0], selectedItems[1]);
        }

        public void StartAutoAgg()
        {
            if (!isAutoAggRunning)
            {
                StartCoroutine(AutoAggCoroutine());
            }
        }

        private IEnumerator AutoAggCoroutine()
        {
            isAutoAggRunning = true;
            Debug.Log("🔄 AutoAGG 시작");

            while (true)
            {
                bool didAgg = false;

                // 현재 Grid의 모든 아이템 찾기
                List<EquipItemUI> allItems = new List<EquipItemUI>();
                foreach (Transform child in gridParent)
                {
                    var itemUI = child.GetComponent<EquipItemUI>();
                    if (itemUI != null)
                    {
                        allItems.Add(itemUI);
                    }
                }

                // 같은 이름과 Tier를 가진 두 개 찾기
                for (int i = 0; i < allItems.Count; i++)
                {
                    for (int j = i + 1; j < allItems.Count; j++)
                    {
                        var eq1 = allItems[i].GetEquipmentData();
                        var eq2 = allItems[j].GetEquipmentData();

                        if (eq1.name == eq2.name && eq1.tier == eq2.tier)
                        {
                            Debug.Log($"🛠️ 자동 합성: {eq1.name} Tier {eq1.tier} → Tier {eq1.tier + 1}");

                            TryAgg(allItems[i], allItems[j]);

                            didAgg = true;
                            yield return new WaitForSeconds(1f); // 1초 대기
                            break; // 한 번 합성 후 리스트 다시 갱신
                        }
                    }
                    if (didAgg) break;
                }

                if (!didAgg)
                {
                    Debug.Log("✅ 합성 가능한 아이템 없음. AutoAGG 종료");
                    break;
                }
            }

            isAutoAggRunning = false;
        }

        private void TryAgg(EquipItemUI item1, EquipItemUI item2)
        {
            Equipment eq1 = item1.GetEquipmentData();
            Equipment eq2 = item2.GetEquipmentData();

            if (eq1.name == eq2.name && eq1.tier == eq2.tier)
            {
                Destroy(item1.gameObject);
                Destroy(item2.gameObject);

                // 합성된 아이템 생성
                Equipment upgradedEquip = new Equipment
                {
                    name = eq1.name,
                    icon = eq1.icon,
                    tier = eq1.tier + 1
                };

                GameObject newItem = Instantiate(equipItemPrefab, gridParent);
                EquipItemUI newItemUI = newItem.GetComponent<EquipItemUI>();
                newItemUI.Setup(upgradedEquip);
                newItemUI.Initialize(this);

                // Tier별 배경색
                if (upgradedEquip.tier == 2)
                {
                    newItemUI.SetBackgroundColor(Color.red);
                }
                else if (upgradedEquip.tier == 3)
                {
                    newItemUI.SetBackgroundColor(Color.yellow);
                }
                else
                {
                    newItemUI.SetBackgroundColor(Color.blue);
                }
            }
            else
            {
                Debug.LogWarning("합성 실패: 조건 불일치");
            }
        }
    }
}
