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


        [Header("Fusion Settings")]
        [SerializeField] private float successRate = 0.7f; // 70% 확률로 성공
        [SerializeField] private GameObject successEffectUIPrefab; // 폭죽 이펙트
        [SerializeField] private GameObject failEffectUIPrefab;    // 실패 이펙트
        [SerializeField] private RectTransform effectLayerTransform;

        public event System.Action<EquipmentType, Equipment> EquippedChanged;
        [SerializeField] private GameObject equipmentPanelRoot;
        [SerializeField] private EquipSlotUI[] equipSlots;     // (옵션) 무기/방어구/부츠/헬멧/악세 슬롯들
        private readonly Dictionary<EquipmentType, EquipSlotUI> slotByType = new();
        public static EquipmentManager Instance { get; private set; }


        private void Awake()
        {
            // 싱글턴 고정
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // 패널만 쓰는 구조면 굳이 유지할 필요 없음.
            // DontDestroyOnLoad(gameObject);

            // 슬롯 매핑
            slotByType.Clear();
            if (equipSlots != null)
            {
                foreach (var s in equipSlots)
                {
                    if (s != null) slotByType[s.slotType] = s;
                }
            }
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

        private void UpdateSlotUI(EquipmentType type)
        {
            // 슬롯 UI를 안 쓰면 그냥 리턴
            if (!slotByType.TryGetValue(type, out var slot) || slot == null)
                return;

            if (equippedByType.TryGetValue(type, out var itemUI) && itemUI != null)
            {
                var eq = itemUI.GetEquipmentData();
                slot.SetEquipment(eq);
            }
            else
            {
                slot.SetEmpty();
            }
        }

        private void RefreshAllSlots()
        {
            if (slotByType.Count == 0) return; // 슬롯 UI 없음 → 무시
            foreach (var kv in slotByType)
                UpdateSlotUI(kv.Key);
        }

        public void OpenEquipmentPanelForType(EquipmentType type)
        {
            currentFilterType = type;
            if (!equipmentPanelRoot)
            {
                Debug.LogError("[Equip] equipmentPanelRoot is NULL");
                return;
            }

            // 1) 활성 & 부모 체인 활성
            equipmentPanelRoot.SetActive(true);
            Transform t = equipmentPanelRoot.transform;
            while (t != null)
            {
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                t = t.parent;
            }

            // 2) 렌더 보정
            equipmentPanelRoot.transform.SetAsLastSibling();     // Canvas 자식 순서 최상단
            equipmentPanelRoot.transform.localScale = Vector3.one;  // 애니메이션/닫기에서 0으로 남는 것 방지

            var cg = equipmentPanelRoot.GetComponent<CanvasGroup>() 
                    ?? equipmentPanelRoot.AddComponent<CanvasGroup>();
            cg.alpha = 1f; 
            cg.interactable = true; 
            cg.blocksRaycasts = true;

            // (선택) 캔버스 정렬 우선순위 올리기 — 다른 패널이 덮지 않도록
            var cv = equipmentPanelRoot.GetComponent<Canvas>();
            if (!cv) cv = equipmentPanelRoot.AddComponent<Canvas>();
            cv.overrideSorting = true; 
            cv.sortingOrder = 3000;

            if (!equipmentPanelRoot.GetComponent<UnityEngine.UI.GraphicRaycaster>())
                equipmentPanelRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // 3) RectTransform 보정 (화면 밖/크기0 방지)
            var rt = equipmentPanelRoot.GetComponent<RectTransform>();
            if (rt)
            {
                rt.anchorMin = Vector2.zero; 
                rt.anchorMax = Vector2.one;
                rt.anchoredPosition = Vector2.zero; 
                rt.sizeDelta = Vector2.zero; 
                rt.localScale = Vector3.one;
            }

            // 4) 탭/그리드 갱신 (동일 탭 재오픈이어도 리스트 재구성)
            if (sortByWeapon) ApplyWeaponSort();
            else ApplyTierSort();

            // 5) 레이아웃 강제 재빌드 (닫았다가 열 때 레이아웃 멈춤 방지)
            Canvas.ForceUpdateCanvases();
            if (gridParent)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
                    gridParent as RectTransform
                );
            }

            // 6) 슬롯 아이콘도 재동기화 (보이면 패스)
            RefreshAllSlots();
        }
        
        private void EnsureGridBuiltForType(EquipmentType type)
        {
            var prevType = currentFilterType;
            var prevSortByWeapon = sortByWeapon;

            currentFilterType = type;
            if (prevSortByWeapon) ApplyWeaponSort();
            else ApplyTierSort();

            // 원래 상태로 되돌리기
            currentFilterType = prevType;
            if (prevSortByWeapon) ApplyWeaponSort();
            else ApplyTierSort();
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
                FusionTracker.Instance?.AddFusions(1);
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

            // 이전 장착 해제 (안전 가드)
            if (equippedByType.TryGetValue(type, out var oldEquipped))
            {
                if (oldEquipped != null && !oldEquipped.Equals(null) && oldEquipped.gameObject != null)
                {
                    oldEquipped.SetEquipped(false);
                }
                else
                {
                    // 죽은 참조 정리
                    equippedByType[type] = null;
                }
            }

            // 새로 장착
            equippedByType[type] = itemUI;
            itemUI.SetEquipped(true);

            SaveEquippedItemsToFile();
            UpdateSlotUI(type);
            EquippedChanged?.Invoke(type, itemUI.GetEquipmentData());
            FindObjectOfType<GameStateManager>()?.RecomputeAllStatsFromBase();
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

            equippedByType.Clear();

            foreach (var saved in equippedList)
            {
                EnsureGridBuiltForType(saved.type); // 이전 답변의 보강안 사용 권장

                var matchedEquip = blueprint.equipments.FirstOrDefault(e =>
                    e.name == saved.equipmentName && e.type == saved.type && e.tier == saved.tier);

                if (matchedEquip == null) continue;

                var ui = gridParent.GetComponentsInChildren<EquipItemUI>(true)
                    .FirstOrDefault(u => u.GetEquipmentData() == matchedEquip);

                if (ui != null)
                {
                    ui.SetEquipped(true);
                    equippedByType[saved.type] = ui;

                    // 타입별 개별 이벤트
                    EquippedChanged?.Invoke(saved.type, matchedEquip);
                }
            }

            RefreshAllSlots(); // 쓰는 중이면
            Debug.Log("[LoadEquippedItems] Equipped mapping restored.");

            var gsm = FindObjectOfType<GameStateManager>();
            gsm?.RecomputeAllStatsFromBase();
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
        public List<Equipment> GetEquippedEquipmentData()
        {
            var list = new List<Equipment>();
            foreach (var kv in equippedByType)
            {
                if (kv.Value == null) continue;
                var eq = kv.Value.GetEquipmentData();
                if (eq != null) list.Add(eq);
            }
            return list;
        }
    }
}
