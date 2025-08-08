using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Vampire
{
    public class DailyPlayTimeRewardUI : MonoBehaviour
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
            UpdateUI();
        }

        void OnEnable()
        {
            UpdateUI();
        }

        void Update()
        {
            UpdateUI();
        }

        void UpdateUI()
        {
            if (PlayTimeTracker.Instance == null) return;

            float totalTime = PlayTimeTracker.Instance.TotalPlayTime;

            // 슬라이더 (0~1)
            playTimeSlider.value = Mathf.Clamp01(totalTime / 300f);

            // 텍스트
            TimeSpan time = TimeSpan.FromSeconds(totalTime);
            playTimeText.text = $"{(int)time.Minutes:D2}:{(int)time.Seconds:D2} / 05:00";

            // 버튼 활성화
            rewardButton.interactable = PlayTimeTracker.Instance.IsRewardAvailable;
        }

        void OnClickReward()
        {
            if (PlayTimeTracker.Instance.IsRewardAvailable)
            {
                PlayTimeTracker.Instance.ClaimReward();
                rewardButton.interactable = false;
                RubyManager.Instance?.AddRubies(ruby); 
                Debug.Log("🎁 보상 지급됨!");
            }
        }
    }
}
