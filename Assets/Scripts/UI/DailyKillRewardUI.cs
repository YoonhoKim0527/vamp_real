using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class DailyKillRewardUI : MonoBehaviour
    {
        [SerializeField] Slider killSlider;
        [SerializeField] TextMeshProUGUI killText;
        [SerializeField] Button rewardButton;
        [SerializeField] TextMeshProUGUI rewardButtonText;
        [SerializeField] int ruby = 1000;
        const int killPerReward = 50;

        void Start()
        {
            rewardButton.onClick.AddListener(ClaimReward);
        }

        void Update()
        {
            var tracker = EnemyKillTracker.Instance;
            if (tracker == null) return;

            int kills = tracker.TotalDailyKills;
            killSlider.value = Mathf.Clamp01((float)kills / killPerReward);
            killText.text = $"{kills}/{killPerReward}";
            rewardButton.interactable = tracker.IsDailyRewardAvailable;
        }

        void ClaimReward()
        {
            var tracker = EnemyKillTracker.Instance;
            if (tracker == null || !tracker.IsDailyRewardAvailable) return;

            tracker.ClaimDailyKillReward();
            RubyManager.Instance?.AddRubies(ruby); 
            Debug.Log("✅ 데일리 처치 보상 지급 완료");
        }
    }
}
