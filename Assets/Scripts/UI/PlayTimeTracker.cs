using UnityEngine;
using System;

namespace Vampire
{
    public class PlayTimeTracker : MonoBehaviour
    {
        public static PlayTimeTracker Instance { get; private set; }

        const float maxPlayTime = 300f;
        float sessionTime = 0f;

        public float TotalPlayTime => Mathf.Min(PlayerPrefs.GetFloat("TodayPlayTime", 0f) + sessionTime, maxPlayTime);
        public bool IsRewardClaimed => PlayerPrefs.GetInt("TodayRewardClaimed", 0) == 1;
        public bool IsRewardAvailable => TotalPlayTime >= maxPlayTime && !IsRewardClaimed;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ResetIfNewDay();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            sessionTime += Time.deltaTime;
        }

        void OnApplicationPause(bool pause)
        {
            if (pause) SavePlayTime();
        }

        void OnApplicationQuit()
        {
            SavePlayTime();
        }

        void SavePlayTime()
        {
            float previous = PlayerPrefs.GetFloat("TodayPlayTime", 0f);
            PlayerPrefs.SetFloat("TodayPlayTime", previous + sessionTime);
            PlayerPrefs.SetString("LastPlayDate", DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
            sessionTime = 0f;
        }

        void ResetIfNewDay()
        {
            string lastDate = PlayerPrefs.GetString("LastPlayDate", "");
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            if (lastDate != today)
            {
                PlayerPrefs.SetFloat("TodayPlayTime", 0f);
                PlayerPrefs.SetInt("TodayRewardClaimed", 0);
                PlayerPrefs.SetString("LastPlayDate", today);
                PlayerPrefs.Save();
            }
        }

        public void ClaimReward()
        {
            if (IsRewardAvailable)
            {
                PlayerPrefs.SetInt("TodayRewardClaimed", 1);
                PlayerPrefs.Save();
            }
        }
    }
}
