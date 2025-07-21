using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        [SerializeField] private TextMeshProUGUI[] levelTexts = new TextMeshProUGUI[8];
        [SerializeField] private TextMeshProUGUI[] nextCostTexts = new TextMeshProUGUI[8];
        [SerializeField] private TextMeshProUGUI[] upgradeAmountTexts = new TextMeshProUGUI[8];

        [Header("Coin Display")]
        [SerializeField] private CoinDisplay coinDisplay; // ✅ 코인 UI 연결

        [Header("Upgrade Settings")]
        [SerializeField] private int[] baseCosts = new int[8];             // 업그레이드 초기 비용
        [SerializeField] private float[] costMultipliers = new float[8];  // 비용 증가 배율
        [SerializeField] private float[] upgradeIncrements = new float[8]; // 업그레이드 증가량

        private List<UpgradeStateSaveData> upgradeStates = new List<UpgradeStateSaveData>();

        private void Start()
        {
            // ✅ SaveData 로드
            SaveData data = saveManager.LoadGame();

            // ✅ playerStats 로드
            playerStats = data.playerStats ?? new CharacterStatBlueprint();

            // ✅ upgradeStates 로드 또는 기본 초기화
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
                int index = i; // 클로저 문제 방지
                upgradeButtons[i].onClick.AddListener(() => UpgradeStat(index));
            }

            // 초기 UI 세팅
            RefreshAllUI();
        }

        private void UpgradeStat(int statIndex)
        {
            if (upgradeStates == null || statIndex >= upgradeStates.Count)
            {
                Debug.LogError($"[UpgradeManager] upgradeStates가 null이거나 인덱스 범위 초과: {statIndex}");
                return;
            }

            UpgradeStateSaveData state = upgradeStates[statIndex];
            int currentCoins = CoinManager.Instance.GetCoins(); // ✅ 코인 가져오기

            if (currentCoins >= state.nextCost)
            {
                // ✅ 코인 차감
                CoinManager.Instance.SpendCoins(state.nextCost); // PlayerPrefs 대신 CoinManager

                // ✅ 레벨 업
                state.level++;

                // ✅ 능력치 업그레이드
                ApplyStatUpgrade(statIndex, state.upgradeIncrement);

                // ✅ 다음 비용 계산
                state.nextCost = Mathf.RoundToInt(baseCosts[statIndex] * Mathf.Pow(costMultipliers[statIndex], state.level));

                // ✅ savefile 저장
                SaveAll();

                // ✅ UI 갱신
                RefreshUI(statIndex);

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
            upgradeAmountTexts[statIndex].text = $"+{state.upgradeIncrement}";
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

        public List<UpgradeStateSaveData> GetUpgradeStates()
        {
            return upgradeStates;
        }

        public void SetUpgradeStates(List<UpgradeStateSaveData> states)
        {
            upgradeStates = states;
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
            }
        }
    }
}
