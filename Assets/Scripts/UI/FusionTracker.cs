using UnityEngine;
using System;

namespace Vampire
{
    public class FusionTracker : MonoBehaviour
    {
        public static FusionTracker Instance { get; private set; }

        [SerializeField] string dateFormat = "yyyy-MM-dd";
        [Header("Thresholds")]
        [SerializeField] int dailyFusionPerReward = 5;    // 데일리 보상 기준 합성 수
        [SerializeField] int infiniteFusionPerReward = 50; // 인피니티 보상 기준 합성 수

        int dailyCount;           // 오늘 합성 횟수
        int lifetimeCount;        // 누적 합성 횟수
        int infiniteClaimedCount; // 인피니티 이미 수령한 묶음 수
        string dailyDate;         // 마지막 갱신 날짜
        bool dailyClaimedToday;   // 오늘 데일리 보상 수령 여부

        public int TotalDailyFusions => dailyCount;
        public int TotalInfiniteFusions => lifetimeCount;
        public int DailyFusionPerReward => dailyFusionPerReward;
        public int InfiniteFusionPerReward => infiniteFusionPerReward;

        public bool IsDailyRewardAvailable
        {
            get
            {
                ResetDailyIfNeeded();
                return dailyCount >= dailyFusionPerReward
                       && !dailyClaimedToday
                       && dailyDate == Today();
            }
        }

        public int InfiniteAvailableRewardCount
        {
            get
            {
                int earned = lifetimeCount / infiniteFusionPerReward;
                int available = earned - infiniteClaimedCount;
                return Mathf.Max(available, 0);
            }
        }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
            ResetDailyIfNeeded();
        }

        string Today() => DateTime.Now.ToString(dateFormat);

        public void ResetDailyIfNeeded()
        {
            string today = Today();
            if (dailyDate != today)
            {
                dailyDate = today;
                dailyCount = 0;
                dailyClaimedToday = false;
                Save();
            }
        }

        // ✅ 합성 1회 시도할 때 호출 (성공/실패 무관)
        public void AddFusions(int count)
        {
            if (count <= 0) return;
            ResetDailyIfNeeded();
            dailyCount += count;
            lifetimeCount += count;
            Save();
        }

        public void ClaimDailyFusionReward()
        {
            if (!IsDailyRewardAvailable) return;
            dailyClaimedToday = true;
            Save();
        }

        public void ClaimInfiniteFusionReward()
        {
            int n = InfiniteAvailableRewardCount;
            if (n <= 0) return;
            infiniteClaimedCount += n;
            Save();
        }

        // ---- PlayerPrefs 저장 ----
        const string K_Daily = "Fusion_DailyCount";
        const string K_Life = "Fusion_LifetimeCount";
        const string K_InfClaim = "Fusion_InfiniteClaimed";
        const string K_Date = "Fusion_DailyDate";
        const string K_DailyClaimed = "Fusion_DailyClaimed";

        void Load()
        {
            dailyCount = PlayerPrefs.GetInt(K_Daily, 0);
            lifetimeCount = PlayerPrefs.GetInt(K_Life, 0);
            infiniteClaimedCount = PlayerPrefs.GetInt(K_InfClaim, 0);
            dailyDate = PlayerPrefs.GetString(K_Date, "");
            dailyClaimedToday = PlayerPrefs.GetInt(K_DailyClaimed, 0) == 1;
        }

        void Save()
        {
            PlayerPrefs.SetInt(K_Daily, dailyCount);
            PlayerPrefs.SetInt(K_Life, lifetimeCount);
            PlayerPrefs.SetInt(K_InfClaim, infiniteClaimedCount);
            PlayerPrefs.SetString(K_Date, dailyDate);
            PlayerPrefs.SetInt(K_DailyClaimed, dailyClaimedToday ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
