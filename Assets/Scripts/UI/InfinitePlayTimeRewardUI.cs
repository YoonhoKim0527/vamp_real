using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Vampire
{
    public class InfinitePlayTimeRewardUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] Slider playTimeSlider;
        [SerializeField] TextMeshProUGUI playTimeText;
        [SerializeField] Button rewardButton;
        [SerializeField] TextMeshProUGUI rewardButtonText;
        [SerializeField] int ruby = 1000;

        void Start()
        {
            rewardButton.onClick.AddListener(OnClickReward);
        }

        void Update()
        {
            if (InfinitePlayTimeTracker.Instance == null) return;

            float totalTime = InfinitePlayTimeTracker.Instance.TotalPlayTime;
            int rewardCount = InfinitePlayTimeTracker.Instance.AvailableRewardCount;
            int totalRuby = ruby * rewardCount;

            // 슬라이더 값
            float fill = (totalTime % 6f) / 6f;
            playTimeSlider.value = fill;

            // 텍스트
            TimeSpan t = TimeSpan.FromSeconds(totalTime % 6f);
            playTimeText.text = $"{(int)t.Minutes:D2}:{(int)t.Seconds:D2} / 00:06";

            // 버튼 활성화 여부
            rewardButton.interactable = rewardCount > 0;
            rewardButtonText.text = rewardCount > 0
            ? $"💎 {totalRuby}"
            : "None";
        }

        void OnClickReward()
        {
            int rewardCount = InfinitePlayTimeTracker.Instance?.AvailableRewardCount ?? 0;

            if (rewardCount > 0)
            {
                InfinitePlayTimeTracker.Instance?.ClaimAllRewards();
                rewardButton.interactable = false;
                RubyManager.Instance?.AddRubies(ruby * rewardCount);
            }
        }
    }
}
