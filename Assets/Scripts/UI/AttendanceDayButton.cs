using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Vampire
{
    public class AttendanceDayButton : MonoBehaviour
    {
        [SerializeField] TMP_Text dayText;
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text amountText;
        [SerializeField] GameObject claimedOverlay;

        public void Init(AttendanceDayData data, bool isClaimed)
        {
            dayText.text = $"Day {data.day}";
            iconImage.sprite = data.rewardIcon;
            amountText.text = $"x{data.rewardAmount}";
            claimedOverlay.SetActive(isClaimed); // 출석 시 비주얼 처리
        }
    }
}
