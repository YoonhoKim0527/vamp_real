using UnityEngine;
using System;

namespace Vampire
{
    public class AttendanceQuestTracker : MonoBehaviour
    {
        public static AttendanceQuestTracker Instance { get; private set; }

        [SerializeField] string dateFormat = "yyyy-MM-dd";

        // 오늘 출석 완료?
        bool attendanceDoneToday;
        // 오늘 출석 퀘스트 보상 수령?
        bool questClaimedToday;
        // 상태 기준 날짜
        string dailyDate;

        public bool AttendanceDoneToday
        {
            get
            {
                ResetIfNeeded();
                return attendanceDoneToday;
            }
        }

        public bool IsQuestRewardAvailable
        {
            get
            {
                ResetIfNeeded();
                return attendanceDoneToday && !questClaimedToday && dailyDate == Today();
            }
        }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
            ResetIfNeeded();
        }

        string Today() => DateTime.Now.ToString(dateFormat);

        void ResetIfNeeded()
        {
            string today = Today();
            if (dailyDate != today)
            {
                dailyDate = today;
                attendanceDoneToday = false;
                questClaimedToday = false;
                Save();
            }
        }

        // ✅ 출석 수령 시 AttendanceManager가 호출
        public void MarkAttendanceDoneToday()
        {
            ResetIfNeeded();
            attendanceDoneToday = true;
            Save();
        }

        // ✅ 퀘스트 보상 UI에서 호출 (루비 지급은 UI에서)
        public void ClaimQuestReward()
        {
            if (!IsQuestRewardAvailable) return;
            questClaimedToday = true;
            Save();
        }

        // ---- 저장 ----
        const string K_Date = "AttQuest_Date";
        const string K_Done = "AttQuest_Done";
        const string K_Claimed = "AttQuest_Claimed";

        void Load()
        {
            dailyDate = PlayerPrefs.GetString(K_Date, "");
            attendanceDoneToday = PlayerPrefs.GetInt(K_Done, 0) == 1;
            questClaimedToday = PlayerPrefs.GetInt(K_Claimed, 0) == 1;
        }

        void Save()
        {
            PlayerPrefs.SetString(K_Date, dailyDate);
            PlayerPrefs.SetInt(K_Done, attendanceDoneToday ? 1 : 0);
            PlayerPrefs.SetInt(K_Claimed, questClaimedToday ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
