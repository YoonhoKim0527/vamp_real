using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class InfiniteDrawRewardUI : MonoBehaviour
    {
        [SerializeField] Slider drawSlider;
        [SerializeField] TextMeshProUGUI drawText;
        [SerializeField] Button rewardButton;
        [SerializeField] TextMeshProUGUI rewardButtonText;
        [SerializeField] int ruby = 1000; // 1회 보상 루비

        void Start()
        {
            rewardButton.onClick.AddListener(ClaimReward);
        }

        void Update()
        {
            var tracker = GachaDrawTracker.Instance;
            if (tracker == null) return;

            int total = tracker.TotalInfiniteDraws;
            int per = tracker.InfiniteDrawPerReward;
            int available = tracker.InfiniteAvailableRewardCount;
            int totalRuby = available * ruby;

            // 슬라이더: 다음 보상까지 진행도
            drawSlider.value = (total % per) / (float)per;

            // 텍스트
            int shown = total % per;
            drawText.text = $"{shown}/{per}";

            // 버튼
            rewardButton.interactable = available > 0;
            rewardButtonText.text = available > 0 ? $"받기 x{available} 💎{totalRuby}" : "None";
        }

        void ClaimReward()
        {
            var tracker = GachaDrawTracker.Instance;
            if (tracker == null || tracker.InfiniteAvailableRewardCount <= 0) return;

            int count = tracker.InfiniteAvailableRewardCount;
            tracker.ClaimInfiniteDrawReward();
            RubyManager.Instance?.AddRubies(ruby * count);

            Debug.Log($"✅ 인피니티 뽑기 보상 지급! {ruby * count} 루비");
        }
    }
}
