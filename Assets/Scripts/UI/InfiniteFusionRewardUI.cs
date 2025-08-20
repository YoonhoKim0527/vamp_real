using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class InfiniteFusionRewardUI : MonoBehaviour
    {
        [SerializeField] Slider fusionSlider;
        [SerializeField] TextMeshProUGUI fusionText;
        [SerializeField] Button rewardButton;
        [SerializeField] TextMeshProUGUI rewardButtonText;
        [SerializeField] int ruby = 1000; // 묶음 1회당

        void Start()
        {
            rewardButton.onClick.AddListener(ClaimReward);
        }

        void Update()
        {
            var t = FusionTracker.Instance;
            if (t == null) return;

            int total = t.TotalInfiniteFusions;
            int per = t.InfiniteFusionPerReward;
            int avail = t.InfiniteAvailableRewardCount;
            int totalRuby = avail * ruby;

            fusionSlider.value = (total % per) / (float)per;
            fusionText.text = $"{total % per}/{per}";

            rewardButton.interactable = avail > 0;
            rewardButtonText.text = avail > 0 ? $"받기 x{avail} 💎{totalRuby}" : "None";
        }

        void ClaimReward()
        {
            var t = FusionTracker.Instance;
            if (t == null || t.InfiniteAvailableRewardCount <= 0) return;

            int count = t.InfiniteAvailableRewardCount;
            t.ClaimInfiniteFusionReward();
            RubyManager.Instance?.AddRubies(ruby * count);

            Debug.Log($"✅ 인피니티 합성 보상 지급! {ruby * count} 루비");
        }
    }
}
