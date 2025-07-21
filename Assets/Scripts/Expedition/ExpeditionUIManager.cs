using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class ExpeditionUIManager : MonoBehaviour
    {
        public static ExpeditionUIManager Instance { get; private set; }

        [SerializeField] Slider bossHpSlider;
        [SerializeField] TextMeshProUGUI stageNameText;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void InitBossUI(string stageName, float maxHp)
        {
            bossHpSlider.maxValue = maxHp;
            bossHpSlider.value = maxHp;
            stageNameText.text = stageName;
        }

        public void UpdateBossHP(float currentHp)
        {
            bossHpSlider.value = currentHp;
        }
    }
}
