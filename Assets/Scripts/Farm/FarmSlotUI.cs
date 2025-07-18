using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Vampire
{

    public class FarmSlotUI : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] Image icon;
        [SerializeField] TMP_Text rewardText;
        [SerializeField] Button collectButton;

        FarmSlot linkedSlot;

        public void Bind(FarmSlot slot, System.Action onCollect)
        {
            linkedSlot = slot;
            icon.sprite = slot.character.walkSpriteSequence[0];
            collectButton.onClick.RemoveAllListeners();
            collectButton.onClick.AddListener(() => onCollect());
        }

        public void Refresh()
        {
            float reward = linkedSlot?.PendingReward ?? 0;
            rewardText.text = $"💰 {Mathf.FloorToInt(reward)}";
        }
    }

}
