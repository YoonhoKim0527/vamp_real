using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Vampire
{
    public class InfiniteKillRewardUI : MonoBehaviour
    {
        [SerializeField] Slider killSlider;
        [SerializeField] TextMeshProUGUI killText;
        [SerializeField] Button rewardButton;
        [SerializeField] TextMeshProUGUI rewardButtonText;
        [SerializeField] int ruby = 1000;

        const int killPerReward = 500;

        void Start()
        {
            rewardButton.onClick.AddListener(ClaimReward);
        }

        void Update()
        {
            var tracker = EnemyKillTracker.Instance;
            if (tracker == null) return;

            int totalKills = tracker.TotalInfiniteKills;
            int rewardCount = tracker.InfiniteAvailableRewardCount;
            int totalRuby = rewardCount * ruby;

            // 슬라이더
            killSlider.value = (totalKills % killPerReward) / (float)killPerReward;

            // 텍스트
            int shownKills = totalKills % killPerReward;
            killText.text = $"{shownKills}/{killPerReward}";

            // 버튼
            rewardButton.interactable = rewardCount > 0;
            rewardButtonText.text = rewardCount > 0
                ? $"💎 {totalRuby}"
                : "None";
        }

        void ClaimReward()
        {
            var tracker = EnemyKillTracker.Instance;
            if (tracker == null || tracker.InfiniteAvailableRewardCount <= 0) return;

            int rewardCount = tracker.InfiniteAvailableRewardCount;

            tracker.ClaimInfiniteKillReward();
            rewardButton.interactable = false;
            RubyManager.Instance?.AddRubies(ruby * rewardCount);

            Debug.Log($"✅ 인피니트 처치 보상 지급 완료! {ruby * rewardCount} 루비 지급됨");
        }
    }
}
