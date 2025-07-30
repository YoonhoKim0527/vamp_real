using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Vampire
{
    public class EquipmentManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private Transform gridParent;
        [SerializeField] private GameObject equipItemPrefab;

        [Header("Equipment Blueprint")]
        [SerializeField] private EquipmentBlueprint blueprint;

        [Header("Sort Buttons")]
        [SerializeField] private Button aggButton;
        [SerializeField] private Button autoAggButton;
        [SerializeField] private Button sortToggleButton;
        [SerializeField] private TMP_Text sortToggleButtonText;

        [Header("Tab Buttons")]
        [SerializeField] private Button weaponTabButton;
        [SerializeField] private Button armorTabButton;
        [SerializeField] private Button bootsTabButton;
        [SerializeField] private Button helmetTabButton;
        [SerializeField] private Button accessoryTabButton;

        private List<EquipItemUI> selectedItems = new List<EquipItemUI>();
        private bool isAutoAggRunning = false;
        private bool sortByWeapon = false;

        private EquipmentType currentFilterType = EquipmentType.Weapon; // 기본 무기

        private Dictionary<EquipmentType, EquipItemUI> equippedByType = new Dictionary<EquipmentType, EquipItemUI>();

        private EquipItemUI activeEquipButtonTarget = null;

        public enum EquipmentUIMode
        {
            Equip,
            Fusion
        }
        [SerializeField] private Button modeToggleButton;
        [SerializeField] private TMP_Text modeToggleButtonText;

        private EquipmentUIMode currentMode = EquipmentUIMode.Fusion;

        public bool isInitialized { get; private set; } = false; 
        
        private static EquipmentManager instance;

        [Header("Fusion Settings")]
        [SerializeField] private float successRate = 0.7f; // 70% 확률로 성공
        [SerializeField] private GameObject successEffectUIPrefab; // 폭죽 이펙트
        [SerializeField] private GameObject failEffectUIPrefab;    // 실패 이펙트
        [SerializeField] private RectTransform effectLayerTransform;


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            Debug.Log("instance destroy on load");
        }

        private void Start()
        {
            // 기존 버튼 이벤트
            aggButton.onClick.AddListener(OnAggButtonClick);
            autoAggButton.onClick.AddListener(StartAutoAgg);
            sortToggleButton.onClick.AddListener(ToggleSortMode);

            modeToggleButton.onClick.AddListener(ToggleMode);
            UpdateModeUI(); // 초기 모드 설정   

            // 탭 버튼 이벤트
            weaponTabButton.onClick.AddListener(() => SwitchTab(EquipmentType.Weapon));
            armorTabButton.onClick.AddListener(() => SwitchTab(EquipmentType.Armor));
            bootsTabButton.onClick.AddListener(() => SwitchTab(EquipmentType.Boots));
            helmetTabButton.onClick.AddListener(() => SwitchTab(EquipmentType.Helmet));
            accessoryTabButton.onClick.AddListener(() => SwitchTab(EquipmentType.Accessory));

            ApplyTierSort(); // 초기 정렬 및 기본 무기 탭 표시
        }

        private void SwitchTab(EquipmentType type)
        {
            currentFilterType = type;

            if (sortByWeapon) ApplyWeaponSort();
            else ApplyTierSort();
        }

        public void PopulateGrid()
        {
            PopulateGrid(blueprint.equipments);
        }

        private void PopulateGrid(List<Equipment> sortedList)
        {
            foreach (Transform child in gridParent)
            {
                Destroy(child.gameObject);
            }

            var filtered = sortedList.Where(e => e.type == currentFilterType).ToList();

            foreach (var equip in filtered)
            {
                GameObject item = Instantiate(equipItemPrefab, gridParent);
                EquipItemUI itemUI = item.GetComponent<EquipItemUI>();
                itemUI.Setup(equip);
                itemUI.Initialize(this);

                // ✅ 현재 장착된 장비라면 다시 반영
                if (equippedByType.TryGetValue(equip.type, out var equipped)
                    && equipped.GetEquipmentData() == equip)
                {
                    itemUI.SetEquipped(true);
                    equippedByType[equip.type] = itemUI; // ✅ 복구
                }

                SetTierColor(itemUI, equip.tier);
            }

            isInitialized = true; // ✅ UI 생성이 끝났을 때만 true로 설정
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
            if (selectedItems.Count < 2)
            {
                Debug.Log("2개의 장비를 선택해주세요.");
                return;
            }

            TryAgg(selectedItems[0], selectedItems[1]);
            selectedItems.Clear();
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

                // 같은 이름과 티어를 가진 장비들을 그룹핑
                var groups = blueprint.equipments
                    .GroupBy(e => (e.name, e.tier))
                    .Where(g => g.Count() >= 2)
                    .ToList();

                foreach (var group in groups)
                {
                    var list = group.ToList();

                    // 가능한 모든 쌍 처리 (ex. 4개면 2쌍 → 2개 합성 → 다음 루프에서 다시 검사)
                    for (int i = 0; i + 1 < list.Count; i += 2)
                    {
                        var eq1 = list[i];
                        var eq2 = list[i + 1];

                        // GameObject 찾기
                        var uiList = gridParent.GetComponentsInChildren<EquipItemUI>().ToList();
                        var ui1 = uiList.FirstOrDefault(ui => ui.GetEquipmentData() == eq1);
                        var ui2 = uiList.FirstOrDefault(ui => ui.GetEquipmentData() == eq2);

                        if (ui1 != null && ui2 != null)
                        {
                            TryAgg(ui1, ui2);
                            didAgg = true;
                            yield return new WaitForSeconds(0.5f);
                            break; // 하나만 처리하고 다시 리스트 재검사
                        }
                    }

                    if (didAgg) break; // 리스트가 바뀌었으니 루프 재시작
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
                // 장비 제거
                Destroy(item1.gameObject);
                Destroy(item2.gameObject);

                blueprint.equipments = blueprint.equipments
                    .Where(e => e != eq1 && e != eq2)
                    .ToList();

                bool isSuccess = Random.value < successRate;

                if (isSuccess)
                {
                    Equipment upgradedEquip = new Equipment
                    {
                        name = eq1.name,
                        icon = eq1.icon,
                        tier = eq1.tier + 1,
                        type = eq1.type
                    };

                    blueprint.equipments.Add(upgradedEquip);

                    ShowFusionEffect(true);
                }
                else
                {
                    ShowFusionEffect(false);
                }

                if (sortByWeapon) ApplyWeaponSort();
                else ApplyTierSort();
            }
            else
            {
                Debug.Log("❌ 이름과 티어가 같은 장비만 합성할 수 있습니다.");
            }
        }

        private void ShowFusionEffect(bool isSuccess)
        {
            GameObject prefab = isSuccess ? successEffectUIPrefab : failEffectUIPrefab;

            if (effectLayerTransform == null)
            {
                Debug.LogWarning("Effect Layer가 설정되지 않았습니다.");
                return;
            }

            GameObject effect = Instantiate(prefab, effectLayerTransform);

            // 화면 중앙 위치로 정렬
            RectTransform rt = effect.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
            }

            Destroy(effect, 2f);
        }


        private void SetTierColor(EquipItemUI itemUI, int tier)
        {
            switch (tier)
            {
                case 2:
                    itemUI.SetBackgroundColor(Color.red);
                    break;
                case 3:
                    itemUI.SetBackgroundColor(Color.yellow);
                    break;
                case 4:
                    itemUI.SetBackgroundColor(Color.white);
                    break;
                default:
                    itemUI.SetBackgroundColor(Color.blue);
                    break;
            }
        }

        private void ToggleSortMode()
        {
            if (sortByWeapon)
            {
                ApplyTierSort();
                sortToggleButtonText.text = "sort(weap)";
            }
            else
            {
                ApplyWeaponSort();
                sortToggleButtonText.text = "sort(tier)";
            }

            sortByWeapon = !sortByWeapon;
        }


        private void ApplyTierSort()
        {
            blueprint.equipments = blueprint.equipments
                .OrderByDescending(e => e.tier)
                .ToList();
            PopulateGrid(blueprint.equipments);
        }

        private void ApplyWeaponSort()
        {
            blueprint.equipments = blueprint.equipments
                .OrderBy(e => e.name)
                .ThenByDescending(e => e.tier)
                .ToList();
            PopulateGrid(blueprint.equipments);
        }

        public void RegisterEquip(EquipItemUI itemUI)
        {
            EquipmentType type = itemUI.GetEquipmentType();

            // 이전 장착 해제
            if (equippedByType.TryGetValue(type, out var oldEquipped))
            {
                oldEquipped.SetEquipped(false);
            }

            // 새로 장착
            equippedByType[type] = itemUI;
            itemUI.SetEquipped(true);

            Debug.Log($"[RegisterEquip] Equipped: {type} -> {itemUI.GetEquipmentData().name} (tier {itemUI.GetEquipmentData().tier})");

            // ✅ 장착 완료 후 즉시 저장
            SaveEquippedItemsToFile();

        }

        private void SaveEquippedItemsToFile()
        {
            GameStateManager gsm = FindObjectOfType<GameStateManager>();
            if (gsm != null)
            {
                gsm.SaveGame(); // GameStateManager가 equippedItems 포함해서 저장
            }
            else
            {
                Debug.LogWarning("[EquipmentManager] GameStateManager를 찾을 수 없어 장착 저장 실패");
            }
        }


        public void ShowEquipButton(EquipItemUI newTarget)
        {
            // ✅ 기존 버튼이 있으면 제거
            if (activeEquipButtonTarget != null && activeEquipButtonTarget != newTarget)
            {
                activeEquipButtonTarget.HideEquipButton();
            }

            // ✅ 새 버튼 생성
            activeEquipButtonTarget = newTarget;
            newTarget.ShowEquipButton();
        }

        public void ClearActiveEquipButton(EquipItemUI target)
        {
            if (activeEquipButtonTarget == target)
                activeEquipButtonTarget = null;
        }

        private void ToggleMode()
        {
            currentMode = currentMode == EquipmentUIMode.Equip
                ? EquipmentUIMode.Fusion
                : EquipmentUIMode.Equip;

            // 🔁 UI 업데이트
            UpdateModeUI();

            // 🔁 선택 상태 초기화
            ClearAllSelections();
        }

        private void UpdateModeUI()
        {
            if (currentMode == EquipmentUIMode.Equip)
            {
                modeToggleButtonText.text = "Mode: Equip";
                aggButton.gameObject.SetActive(false);
                autoAggButton.gameObject.SetActive(false);
            }
            else
            {
                modeToggleButtonText.text = "Mode: Fusion";
                aggButton.gameObject.SetActive(true);
                autoAggButton.gameObject.SetActive(true);
                ClearActiveEquipButton(); // 버튼 제거
            }
        }

        public bool IsInEquipMode()
        {
            return currentMode == EquipmentUIMode.Equip;
        }

        private void ClearAllSelections()
        {
            foreach (var item in selectedItems)
                item.SetSelected(false);

            selectedItems.Clear();
        }

        private void ClearActiveEquipButton()
        {
            if (activeEquipButtonTarget != null)
            {
                activeEquipButtonTarget.HideEquipButton();
                activeEquipButtonTarget = null;
            }
        }

        public List<EquippedItemSaveData> GetEquippedItemsForSave()
        {
            Debug.Log($"[EquipmentManager] equippedByType.Count = {equippedByType.Count}");

            return equippedByType.Select(kvp =>
            {
                var eq = kvp.Value.GetEquipmentData();
                Debug.Log($"[EquipmentManager] Saving equipped: {kvp.Key} -> {eq.name} (tier {eq.tier})");

                return new EquippedItemSaveData
                {
                    type = kvp.Key,
                    equipmentName = eq.name,
                    tier = eq.tier
                };
            }).ToList();
        }

        public void LoadEquippedItems(List<EquippedItemSaveData> equippedList)
        {
            if (equippedList == null) return;

            foreach (var saved in equippedList)
            {
                var matchedEquip = blueprint.equipments.FirstOrDefault(e =>
                    e.name == saved.equipmentName &&
                    e.type == saved.type &&
                    e.tier == saved.tier); // ✅ 정확한 tier까지 비교


                if (matchedEquip == null) continue;

                var ui = gridParent.GetComponentsInChildren<EquipItemUI>()
                    .FirstOrDefault(ui => ui.GetEquipmentData() == matchedEquip);

                if (ui != null)
                {
                    Debug.Log($"[LoadEquippedItems] Setting equipped: {matchedEquip.name} (Tier {matchedEquip.tier})");
                    ui.SetEquipped(true);
                    equippedByType[saved.type] = ui;
                }
                else
                {
                    Debug.LogWarning($"[LoadEquippedItems] UI not found for: {matchedEquip.name} (Tier {matchedEquip.tier})");
                }
            }
            Debug.Log("hi");
        }

        public List<EquipItemUI> GetEquippedItems()
        {
            return equippedByType.Values.ToList();
        }

        public EquipmentBlueprint GetPlayerBlueprint()
        {
            return blueprint;
        }

        public void RefreshCurrentTab()
        {
            if (sortByWeapon)
            {
                ApplyWeaponSort();
            }
            else
            {
                ApplyTierSort();
            }
        }
    }
}
