using UnityEngine;
using System;

namespace Vampire
{
    public class EnemyKillTracker : MonoBehaviour
    {
        public static EnemyKillTracker Instance { get; private set; }

        const int dailyKillGoal = 50;
        const int infiniteKillGoal = 100;

        int sessionKills = 0;

        public int TotalDailyKills => PlayerPrefs.GetInt("TodayKillCount", 0) + sessionKills;
        public int TotalInfiniteKills => PlayerPrefs.GetInt("InfiniteKillCount", 0) + sessionKills;

        public bool IsDailyRewardClaimed => PlayerPrefs.GetInt("TodayKillRewardClaimed", 0) == 1;
        public bool IsDailyRewardAvailable => TotalDailyKills >= dailyKillGoal && !IsDailyRewardClaimed;

        public int TotalInfiniteRewardClaimed => PlayerPrefs.GetInt("InfiniteKillRewardClaimed", 0);
        public int InfiniteAvailableRewardCount => Mathf.FloorToInt((float)TotalInfiniteKills / infiniteKillGoal) - TotalInfiniteRewardClaimed;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ResetDailyKillIfNewDay();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ReportKill()
        {
            sessionKills++;
        }

        void OnApplicationPause(bool pause)
        {
            if (pause) SaveKills();
        }

        void OnApplicationQuit()
        {
            SaveKills();
        }

        void SaveKills()
        {
            PlayerPrefs.SetInt("TodayKillCount", PlayerPrefs.GetInt("TodayKillCount", 0) + sessionKills);
            PlayerPrefs.SetInt("InfiniteKillCount", PlayerPrefs.GetInt("InfiniteKillCount", 0) + sessionKills);
            PlayerPrefs.SetString("LastKillDate", DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
            sessionKills = 0;
        }

        void ResetDailyKillIfNewDay()
        {
            string lastDate = PlayerPrefs.GetString("LastKillDate", "");
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            if (lastDate != today)
            {
                PlayerPrefs.SetInt("TodayKillCount", 0);
                PlayerPrefs.SetInt("TodayKillRewardClaimed", 0);
                PlayerPrefs.SetString("LastKillDate", today);
                PlayerPrefs.Save();
            }
        }

        public void ClaimDailyKillReward()
        {
            if (IsDailyRewardAvailable)
            {
                PlayerPrefs.SetInt("TodayKillRewardClaimed", 1);
                PlayerPrefs.Save();
            }
        }

        public void ClaimInfiniteKillReward()
        {
            int count = InfiniteAvailableRewardCount;
            if (count <= 0) return;

            PlayerPrefs.SetInt("InfiniteKillRewardClaimed", TotalInfiniteRewardClaimed + count);
            PlayerPrefs.Save();

            for (int i = 0; i < count; i++)
                Debug.Log($"🎯 인피니트 처치 보상 {i + 1}/{count} 지급됨!");
        }

#if UNITY_EDITOR
        [ContextMenu("🧪 데일리 처치 50회 채우기")]
        void Debug_SetDailyMax()
        {
            PlayerPrefs.SetInt("TodayKillCount", 50);
            PlayerPrefs.SetInt("TodayKillRewardClaimed", 0);
            PlayerPrefs.SetString("LastKillDate", DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
        }

        [ContextMenu("🧪 인피니트 처치 500회")]
        void Debug_SetInfiniteKills()
        {
            PlayerPrefs.SetInt("InfiniteKillCount", 500);
            PlayerPrefs.SetInt("InfiniteKillRewardClaimed", 0);
            PlayerPrefs.Save();
        }

        [ContextMenu("🧹 킬 데이터 초기화")]
        void Debug_ResetKills()
        {
            PlayerPrefs.DeleteKey("TodayKillCount");
            PlayerPrefs.DeleteKey("TodayKillRewardClaimed");
            PlayerPrefs.DeleteKey("InfiniteKillCount");
            PlayerPrefs.DeleteKey("InfiniteKillRewardClaimed");
            PlayerPrefs.DeleteKey("LastKillDate");
            PlayerPrefs.Save();
        }
#endif
    }
}
