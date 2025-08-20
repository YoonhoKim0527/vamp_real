using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class AttendanceDailyQuestUI : MonoBehaviour
    {
        [SerializeField] Slider progressSlider;          // 0~1
        [SerializeField] TextMeshProUGUI progressText;   // "0/1" or "1/1"
        [SerializeField] Button claimButton;
        [SerializeField] TextMeshProUGUI claimButtonText;
        [SerializeField] int rubyReward = 500;           // 출석 데일리 퀘스트 보상 루비

        void Start()
        {
            claimButton.onClick.AddListener(ClaimReward);
        }

        void Update()
        {
            var t = AttendanceQuestTracker.Instance;
            if (t == null) return;

            bool done = t.AttendanceDoneToday;
            bool canClaim = t.IsQuestRewardAvailable;

            progressSlider.value = done ? 1f : 0f;
            progressText.text = done ? "1/1" : "0/1";

            claimButton.interactable = canClaim;
            claimButtonText.text = canClaim ? $"받기 💎{rubyReward}" : (done ? "완료" : "진행중");
        }

        void ClaimReward()
        {
            var t = AttendanceQuestTracker.Instance;
            if (t == null || !t.IsQuestRewardAvailable) return;

            t.ClaimQuestReward();
            RubyManager.Instance?.AddRubies(rubyReward);

            Debug.Log($"✅ 출석 데일리 퀘스트 보상 지급: 루비 {rubyReward}");
        }
    }
}
