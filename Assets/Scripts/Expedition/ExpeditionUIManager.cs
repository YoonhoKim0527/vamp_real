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
        [SerializeField] TextMeshProUGUI bossHpText;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void InitBossUI(string stageName)
        {
            bossHpSlider.maxValue = 1f; // 슬라이더는 항상 현재 bar 기준 진행도만 보여줌
            bossHpSlider.value = 1f;
            stageNameText.text = stageName;
            bossHpText.text = $"x1000";
        }

        public void UpdateBossBar(int barIndex, float barFill)
        {
            bossHpSlider.value = barFill;
            bossHpText.text = $"x{barIndex}";
        }
    }
}
