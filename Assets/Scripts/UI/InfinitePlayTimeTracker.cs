using UnityEngine;

namespace Vampire
{
    public class InfinitePlayTimeTracker : MonoBehaviour
    {
        public static InfinitePlayTimeTracker Instance { get; private set; }

        const float rewardInterval = 6f;
        float sessionTime = 0f;

        public float TotalPlayTime => PlayerPrefs.GetFloat("InfinitePlayTime", 0f) + sessionTime;
        public int TotalRewardClaimed => PlayerPrefs.GetInt("InfiniteRewardClaimedCount", 0);
        public int AvailableRewardCount => Mathf.FloorToInt(TotalPlayTime / rewardInterval) - TotalRewardClaimed;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        void Update()
        {
            sessionTime += Time.deltaTime;
        }

        void OnApplicationPause(bool pause)
        {
            if (pause) Save();
        }

        void OnApplicationQuit()
        {
            Save();
        }

        void Save()
        {
            float previous = PlayerPrefs.GetFloat("InfinitePlayTime", 0f);
            PlayerPrefs.SetFloat("InfinitePlayTime", previous + sessionTime);
            PlayerPrefs.Save();
            sessionTime = 0f;
        }

        public void ClaimAllRewards()
        {
            int count = AvailableRewardCount;
            if (count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                Debug.Log($"🎁 보상 {i + 1}/{count} 지급됨!");
                // TODO: 여기에 보상 지급 로직 삽입
            }

            PlayerPrefs.SetInt("InfiniteRewardClaimedCount", TotalRewardClaimed + count);
            PlayerPrefs.Save();
        }
    }
}
