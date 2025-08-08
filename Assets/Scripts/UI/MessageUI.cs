using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Vampire
{
    public class MessageUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI contentText;
        [SerializeField] TextMeshProUGUI rewardText;
        [SerializeField] Button claimButton;

        MessageData currentMessage;

        public void Init(MessageData message)
        {
            currentMessage = message;
            titleText.text = message.title;
            contentText.text = message.content;
            rewardText.text = $"{message.rewardType} x{message.rewardAmount}";
            claimButton.interactable = !message.claimed;

            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(() =>
            {
                MessageManager.Instance.ClaimReward(message.id);
                claimButton.interactable = false;
            });
        }
    }
}
