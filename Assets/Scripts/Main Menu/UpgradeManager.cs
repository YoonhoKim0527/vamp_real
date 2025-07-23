using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Vampire
{
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private SaveManager saveManager;

        [Header("Stat Data")]
        [SerializeField] private CharacterStatBlueprint playerStats;

        [Header("UI Elements")]
        [SerializeField] private Button[] upgradeButtons = new Button[8];
        [SerializeField] private Image[] lockOverlays = new Image[8]; // 🔒 자물쇠 스프라이트
        [SerializeField] private TextMeshProUGUI[] levelTexts = new TextMeshProUGUI[8];
        [SerializeField] private TextMeshProUGUI[] nextCostTexts = new TextMeshProUGUI[8];
        [SerializeField] private TextMeshProUGUI[] upgradeAmountTexts = new TextMeshProUGUI[8];

        [Header("Coin Display")]
        [SerializeField] private CoinDisplay coinDisplay;

        [Header("Upgrade Settings")]
        [SerializeField] private int[] baseCosts = new int[8];
        [SerializeField] private float[] costMultipliers = new float[8];
        [SerializeField] private float[] upgradeIncrements = new float[8];

        [Header("Burst Growth Settings")]
        [SerializeField] private int[] burstLevelIntervals = new int[8];           // 폭풍 성장 주기
        [SerializeField] private float[] burstIncrements = new float[8];           // 폭풍 증가치
        [SerializeField] private float[] burstCostMultipliers = new float[8];      // 폭풍 비용 배율
        [SerializeField] private Color burstButtonColor = Color.red;               // 폭풍 성장 시 버튼 색
        [SerializeField] private Color normalButtonColor = Color.white;            // 일반 버튼 색

        [Header("Unlock Conditions")]
        [SerializeField] private bool[] isUnlocked = new bool[8];                  // 능력 해금 여부
        [SerializeField] private int[] unlockRequirements = new int[8];            // 해금 조건 (공격력 레벨)
        [SerializeField] private GameObject unlockParticlePrefab;                  // 🔥 해금 파티클 Prefab

        private List<UpgradeStateSaveData> upgradeStates = new List<UpgradeStateSaveData>();

        private void Start()
        {
            // ✅ SaveData 로드
            SaveData data = saveManager.LoadGame();

            playerStats = data.playerStats ?? new CharacterStatBlueprint();

            if (data.upgradeStates != null && data.upgradeStates.Count == 8)
            {
                upgradeStates = data.upgradeStates;
                Debug.Log("[UpgradeManager] Loaded upgradeStates from savefile.");
            }
            else
            {
                Debug.LogWarning("[UpgradeManager] No saved upgradeStates found. Initializing defaults.");
                InitializeDefaultUpgradeStates();
                SaveAll();
            }

            // 버튼 클릭 이벤트 등록
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                int index = i;
                upgradeButtons[i].onClick.AddListener(() => UpgradeStat(index));
            }

            RefreshAllUI();
        }

        private void UpgradeStat(int statIndex)
        {
            if (upgradeStates == null || statIndex >= upgradeStates.Count)
            {
                Debug.LogError($"[UpgradeManager] upgradeStates가 null이거나 인덱스 범위 초과: {statIndex}");
                return;
            }

            if (!isUnlocked[statIndex])
            {
                Debug.LogWarning($"[UpgradeManager] {GetUpgradeName(statIndex)}는 아직 해금되지 않았습니다.");
                return;
            }

            UpgradeStateSaveData state = upgradeStates[statIndex];
            int currentCoins = CoinManager.Instance.GetCoins();

            if (currentCoins >= state.nextCost)
            {
                // ✅ 코인 차감
                CoinManager.Instance.SpendCoins(state.nextCost);

                // ✅ 레벨 업
                state.level++;

                // ✅ 이번 레벨업이 폭풍 성장인지 확인
                bool isBurstLevel = IsBurstLevel(statIndex, state.level);
                float increment = isBurstLevel ? burstIncrements[statIndex] : state.upgradeIncrement;

                ApplyStatUpgrade(statIndex, increment);

                // ✅ 다음 비용 계산 (다음 레벨 기준으로 폭풍 성장 체크)
                state.nextCost = Mathf.RoundToInt(
                    baseCosts[statIndex] * Mathf.Pow(costMultipliers[statIndex], state.level)
                );

                if (IsBurstLevel(statIndex, state.level + 1))
                {
                    state.nextCost = Mathf.RoundToInt(state.nextCost * burstCostMultipliers[statIndex]);
                }

                SaveAll();
                RefreshAllUI();

                Debug.Log($"[UpgradeManager] 업그레이드 완료: {state.upgradeName}, New Level: {state.level}");
            }
            else
            {
                Debug.LogWarning($"[UpgradeManager] 코인이 부족합니다. 현재 코인: {currentCoins}, 필요 코인: {state.nextCost}");
            }
        }

        private void ApplyStatUpgrade(int statIndex, float increment)
        {
            switch (statIndex)
            {
                case 0: playerStats.UpgradeAttack(increment); break;
                case 1: playerStats.UpgradeHealth(increment); break;
                case 2: playerStats.UpgradeMoveSpeed(increment); break;
                case 3: playerStats.UpgradeDefense(increment); break;
                case 4: playerStats.UpgradeHealthRegen(increment); break;
                case 5: playerStats.UpgradeCriticalChance(increment); break;
                case 6: playerStats.UpgradeCriticalDamage(increment); break;
                case 7: playerStats.UpgradeExtraProjectiles(1); break;
            }

            // ✅ 공격력 업그레이드 후 해금 조건 체크
            if (statIndex == 0) // 공격력
            {
                CheckUnlocks();
            }
        }

        private void CheckUnlocks()
        {
            int attackLevel = upgradeStates[0].level;

            for (int i = 0; i < isUnlocked.Length; i++)
            {
                if (!isUnlocked[i] && unlockRequirements[i] > 0 && attackLevel >= unlockRequirements[i])
                {
                    isUnlocked[i] = true;
                    Debug.Log($"[UpgradeManager] {GetUpgradeName(i)} 해금됨!");

                    // 🔥 파티클 효과 재생 및 자물쇠 해제
                    StartCoroutine(PlayUnlockEffect(i));
                }
            }
        }

        private IEnumerator PlayUnlockEffect(int statIndex)
        {
            // 🔒 자물쇠 오브젝트
            GameObject lockObj = lockOverlays[statIndex].gameObject;

            if (unlockParticlePrefab != null)
            {
                // 🪝 자물쇠와 동일한 부모로 지정
                Transform lockParent = lockObj.transform.parent;

                // 🗺 자물쇠의 로컬 위치 계산
                Vector3 localPosition = lockParent.InverseTransformPoint(lockObj.transform.position);

                // 🧱 파티클 생성 (자물쇠와 같은 부모)
                GameObject particles = Instantiate(unlockParticlePrefab, lockParent);

                // 📌 위치 & 스케일 초기화
                RectTransform rect = particles.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.localPosition = localPosition;
                    rect.localScale = Vector3.one; // 스케일 초기화
                }
                else
                {
                    particles.transform.localPosition = localPosition;
                    particles.transform.localScale = Vector3.one;
                }

                // 📌 Root Layer를 Default로 강제
                particles.layer = LayerMask.NameToLayer("Default");

                // 📌 ParticleSystem Renderer 설정
                ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingLayerName = "UI"; // UI Sorting Layer 사용
                    renderer.sortingOrder = 1000;     // 자물쇠보다 위에 오도록 Order 높임
                }

                // 📌 맨 위로 올리기 (Draw Order)
                particles.transform.SetAsLastSibling();

                // ▶️ 파티클 재생
                ParticleSystem ps = particles.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                    yield return new WaitForSeconds(ps.main.duration); // 파티클 재생 끝날 때까지 대기
                }
                else
                {
                    Debug.LogWarning("[UpgradeManager] ParticleSystem 컴포넌트를 찾지 못했습니다.");
                    yield return new WaitForSeconds(1f); // 안전하게 대기
                }

                // 🧹 파티클 제거
                Destroy(particles);
            }

            // 🔓 자물쇠 해제
            lockObj.SetActive(false);
            upgradeButtons[statIndex].interactable = true;
        }


        public void RefreshAllUI()
        {
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                RefreshUI(i);
            }
        }

        public void RefreshUI(int statIndex)
        {
            UpgradeStateSaveData state = upgradeStates[statIndex];

            levelTexts[statIndex].text = $"Lv. {state.level}";
            nextCostTexts[statIndex].text = $"Next: {state.nextCost} Coins";

            // ✅ 다음 레벨이 폭풍 성장인지 체크
            bool isNextBurstLevel = IsBurstLevel(statIndex, state.level + 1);

            // ✅ 증가량 표시
            float displayIncrement = isNextBurstLevel ? burstIncrements[statIndex] : state.upgradeIncrement;
            upgradeAmountTexts[statIndex].text = $"+{displayIncrement}";

            // ✅ 버튼 색상 변경
            UpdateButtonColor(statIndex, isNextBurstLevel);

            // ✅ 해금 여부 처리
            bool unlocked = isUnlocked[statIndex];
            upgradeButtons[statIndex].interactable = unlocked;
            lockOverlays[statIndex].gameObject.SetActive(!unlocked);
        }

        private void UpdateButtonColor(int statIndex, bool isNextBurstLevel)
        {
            Button button = upgradeButtons[statIndex];
            Image parentImage = button.transform.parent.GetComponent<Image>();

            if (parentImage != null)
            {
                parentImage.color = isNextBurstLevel ? burstButtonColor : normalButtonColor;
            }
        }

        private bool IsBurstLevel(int statIndex, int level)
        {
            int interval = burstLevelIntervals[statIndex];
            return interval > 0 && level % interval == 0;
        }

        private void SaveAll()
        {
            saveManager.SaveGame(
                items: null,
                upgrades: null,
                playerStats: playerStats,
                upgradeStates: upgradeStates
            );
        }

        private string GetUpgradeName(int index)
        {
            switch (index)
            {
                case 0: return "Attack";
                case 1: return "Health";
                case 2: return "Speed";
                case 3: return "Defense";
                case 4: return "Regen";
                case 5: return "CritChance";
                case 6: return "CritDamage";
                case 7: return "ExtraProjectile";
                default: return "Unknown";
            }
        }

        public void InitializeDefaultUpgradeStates()
        {
            upgradeStates.Clear();
            for (int i = 0; i < 8; i++)
            {
                upgradeStates.Add(new UpgradeStateSaveData
                {
                    upgradeName = GetUpgradeName(i),
                    level = 0,
                    nextCost = baseCosts[i],
                    upgradeIncrement = upgradeIncrements[i]
                });
                isUnlocked[i] = (unlockRequirements[i] <= 0); // 기본 해금 여부
            }
        }

        public List<UpgradeStateSaveData> GetUpgradeStates()
        {
            return upgradeStates;
        }

        public void SetUpgradeStates(List<UpgradeStateSaveData> states)
        {
            upgradeStates = states;
        }
    }
}
