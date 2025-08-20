using UnityEngine;
using System;

namespace Vampire
{
    public class GachaDrawTracker : MonoBehaviour
    {
        public static GachaDrawTracker Instance { get; private set; }

        [SerializeField] string dateFormat = "yyyy-MM-dd";
        [Header("Thresholds")]
        [SerializeField] int dailyDrawPerReward = 10;     // 일일 보상 기준 뽑기 수
        [SerializeField] int infiniteDrawPerReward = 100; // 인피니티 보상 기준 뽑기 수

        // 저장 값
        int dailyCount;                 // 오늘 뽑기 수
        int lifetimeCount;              // 누적 뽑기 수
        int infiniteClaimedCount;       // 인피니티 보상 이미 수령한 개수
        string dailyDate;               // 마지막 기록 날짜 (yyyy-MM-dd)
        bool dailyClaimedToday;         // 오늘 보상 이미 수령했는지

        public int TotalDailyDraws => dailyCount;
        public int TotalInfiniteDraws => lifetimeCount;
        public int DailyDrawPerReward => dailyDrawPerReward;
        public int InfiniteDrawPerReward => infiniteDrawPerReward;

        public bool IsDailyRewardAvailable
        {
            get
            {
                ResetDailyIfNeeded();
                return dailyCount >= dailyDrawPerReward
                       && !dailyClaimedToday
                       && dailyDate == Today();
            }
        }

        public int InfiniteAvailableRewardCount
        {
            get
            {
                int earned = lifetimeCount / infiniteDrawPerReward;
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

        public void AddDraws(int count)
        {
            if (count <= 0) return;
            ResetDailyIfNeeded();
            dailyCount += count;
            lifetimeCount += count;
            Save();
            //GameEvents.OnGachaDrawsAdded?.Invoke(count);
        }

        public void ClaimDailyDrawReward()
        {
            if (!IsDailyRewardAvailable) return;
            dailyClaimedToday = true;
            Save();
        }

        public void ClaimInfiniteDrawReward()
        {
            int n = InfiniteAvailableRewardCount;
            if (n <= 0) return;
            infiniteClaimedCount += n;
            Save();
        }

        // 간단히 PlayerPrefs로 저장 (원하면 SaveManager 연동으로 교체)
        const string K_DailyCount = "Gacha_DailyCount";
        const string K_LifetimeCount = "Gacha_LifetimeCount";
        const string K_InfiniteClaimed = "Gacha_InfiniteClaimed";
        const string K_DailyDate = "Gacha_DailyDate";
        const string K_DailyClaimedToday = "Gacha_DailyClaimedToday";

        void Load()
        {
            dailyCount = PlayerPrefs.GetInt(K_DailyCount, 0);
            lifetimeCount = PlayerPrefs.GetInt(K_LifetimeCount, 0);
            infiniteClaimedCount = PlayerPrefs.GetInt(K_InfiniteClaimed, 0);
            dailyDate = PlayerPrefs.GetString(K_DailyDate, "");
            dailyClaimedToday = PlayerPrefs.GetInt(K_DailyClaimedToday, 0) == 1;
        }

        void Save()
        {
            PlayerPrefs.SetInt(K_DailyCount, dailyCount);
            PlayerPrefs.SetInt(K_LifetimeCount, lifetimeCount);
            PlayerPrefs.SetInt(K_InfiniteClaimed, infiniteClaimedCount);
            PlayerPrefs.SetString(K_DailyDate, dailyDate);
            PlayerPrefs.SetInt(K_DailyClaimedToday, dailyClaimedToday ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
