using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Vampire
{
    public class AttendanceManager : MonoBehaviour
    {
        [Header("출석표 데이터")]
        [SerializeField] AttendanceTableSO table;

        [Header("출석표 버전 (수동 설정)")]
        [SerializeField] int currentVersion = 1;

        [Header("UI 관련")]
        [SerializeField] GameObject buttonPrefab;
        [SerializeField] Transform gridParent;

        [Header("보상 수령")]
        [SerializeField] Button claimButton;
        [SerializeField] TMP_Text claimButtonText;

        const string VERSION_KEY = "Attendance_Version";

        int claimedCount;

        void Start()
        {
            CheckVersion();
            GenerateCalendarUI();
            UpdateClaimButton();
        }

        void CheckVersion()
        {
            int savedVersion = PlayerPrefs.GetInt(VERSION_KEY, 0);

            if (savedVersion != currentVersion)
            {
                Debug.Log("출석표 버전 변경 감지됨! 상태 초기화됨.");
                PlayerPrefs.SetInt(VERSION_KEY, currentVersion);
                PlayerPrefs.SetInt(GetCountKey(), 0);
                PlayerPrefs.SetString(GetLastDateKey(), ""); // 출석 날짜 초기화
            }

            claimedCount = PlayerPrefs.GetInt(GetCountKey(), 0);
        }

        void GenerateCalendarUI()
        {
            for (int i = 0; i < table.dayData.Length; i++)
            {
                var obj = Instantiate(buttonPrefab, gridParent);
                var btn = obj.GetComponent<AttendanceDayButton>();
                bool claimed = i < claimedCount;
                btn.Init(table.dayData[i], claimed);
            }
        }

        void TryMarkToday()
        {
            string today = DateTime.Now.ToString("yyyyMMdd");
            string lastDate = PlayerPrefs.GetString(GetLastDateKey(), "");

            if (lastDate == today)
            {
                Debug.Log("오늘 이미 출석함");
                return;
            }

            if (claimedCount >= 28)
            {
                Debug.Log("출석 완료. 리셋 필요");
                return;
            }

            // ✅ 출석 성공
            claimedCount++;
            PlayerPrefs.SetInt(GetCountKey(), claimedCount);
            PlayerPrefs.SetString(GetLastDateKey(), today);
            PlayerPrefs.Save();

            Debug.Log($"출석 성공! {claimedCount}일차 보상 지급");
            // TODO: 보상 지급 로직 추가
        }
        void UpdateClaimButton()
        {
            string today = DateTime.Now.ToString("yyyyMMdd");
            string lastDate = PlayerPrefs.GetString(GetLastDateKey(), "");

            bool alreadyClaimed = lastDate == today;
            bool allClaimed = claimedCount >= table.dayData.Length;

            if (alreadyClaimed || allClaimed)
            {
                claimButton.interactable = false;
                claimButtonText.text = alreadyClaimed ? "Received" : "Received all this Month";
            }
            else
            {
                claimButton.interactable = true;
                claimButtonText.text = "Receive";
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(ClaimTodayReward);
            }
        }
        void ClaimTodayReward()
        {
            var todayData = table.dayData[claimedCount];

            // TODO: 여기에 보상 지급 로직 추가
            Debug.Log($"💰 {todayData.rewardName} x{todayData.rewardAmount} 지급!");
            RubyManager.Instance.AddRubies(todayData.rewardAmount);

            claimedCount++;
            PlayerPrefs.SetInt(GetCountKey(), claimedCount);
            PlayerPrefs.SetString(GetLastDateKey(), DateTime.Now.ToString("yyyyMMdd"));
            PlayerPrefs.Save();

            UpdateCalendarUI();
            AttendanceQuestTracker.Instance?.MarkAttendanceDoneToday();
            UpdateClaimButton();
        }
        void UpdateCalendarUI()
        {
            // 간단하게 씬 다시 그리는 방법: 버튼 다 지우고 재생성
            foreach (Transform child in gridParent)
                Destroy(child.gameObject);

            GenerateCalendarUI();
        }


        string GetCountKey() => $"Attendance_Count_v{currentVersion}";
        string GetLastDateKey() => $"Attendance_LastDate_v{currentVersion}";
    }
}
