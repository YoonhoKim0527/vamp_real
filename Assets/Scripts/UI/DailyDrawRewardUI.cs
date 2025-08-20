using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class DailyDrawRewardUI : MonoBehaviour
    {
        [SerializeField] Slider drawSlider;
        [SerializeField] TextMeshProUGUI drawText;
        [SerializeField] Button rewardButton;
        [SerializeField] TextMeshProUGUI rewardButtonText;
        [SerializeField] int ruby = 1000; // 보상 루비

        void Start()
        {
            rewardButton.onClick.AddListener(ClaimReward);
        }

        void Update()
        {
            var tracker = GachaDrawTracker.Instance;
            if (tracker == null) return;

            int draws = tracker.TotalDailyDraws;
            int target = tracker.DailyDrawPerReward;

            drawSlider.value = Mathf.Clamp01((float)draws / target);
            drawText.text = $"{draws}/{target}";

            bool canClaim = tracker.IsDailyRewardAvailable;
            rewardButton.interactable = canClaim;
            rewardButtonText.text = canClaim ? $"받기 💎{ruby}" : "진행중";
        }

        void ClaimReward()
        {
            var tracker = GachaDrawTracker.Instance;
            if (tracker == null || !tracker.IsDailyRewardAvailable) return;

            tracker.ClaimDailyDrawReward();
            RubyManager.Instance?.AddRubies(ruby);
            Debug.Log("✅ 데일리 뽑기 보상 지급 완료");
        }
    }
}
