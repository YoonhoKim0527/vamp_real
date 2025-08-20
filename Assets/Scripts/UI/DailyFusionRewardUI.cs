using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class DailyFusionRewardUI : MonoBehaviour
    {
        [SerializeField] Slider fusionSlider;
        [SerializeField] TextMeshProUGUI fusionText;
        [SerializeField] Button rewardButton;
        [SerializeField] TextMeshProUGUI rewardButtonText;
        [SerializeField] int ruby = 500;

        void Start()
        {
            rewardButton.onClick.AddListener(ClaimReward);
        }

        void Update()
        {
            var t = FusionTracker.Instance;
            if (t == null) return;

            int f = t.TotalDailyFusions;
            int target = t.DailyFusionPerReward;

            fusionSlider.value = Mathf.Clamp01((float)f / target);
            fusionText.text = $"{f}/{target}";

            bool can = t.IsDailyRewardAvailable;
            rewardButton.interactable = can;
            rewardButtonText.text = can ? $"받기 💎{ruby}" : "진행중";
        }

        void ClaimReward()
        {
            var t = FusionTracker.Instance;
            if (t == null || !t.IsDailyRewardAvailable) return;

            t.ClaimDailyFusionReward();
            RubyManager.Instance?.AddRubies(ruby);

            Debug.Log("✅ 데일리 합성 보상 지급");
        }
    }
}
