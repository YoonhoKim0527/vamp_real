using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class QuestPanelController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] Button dailyQuestButton;
        [SerializeField] Button infiniteQuestButton;
        [SerializeField] Button exitButton;

        [Header("Panels")]
        [SerializeField] GameObject dailyQuestPanel;
        [SerializeField] GameObject infiniteQuestPanel;
        [SerializeField] GameObject questPanel; // 전체 퀘스트 패널 (닫기용)

        void Start()
        {
            // 시작 시: Daily 켜기, Infinite 끄기
            dailyQuestPanel.SetActive(true);
            infiniteQuestPanel.SetActive(false);

            // 버튼 연결
            dailyQuestButton.onClick.AddListener(() => OpenOnly(dailyQuestPanel));
            infiniteQuestButton.onClick.AddListener(() => OpenOnly(infiniteQuestPanel));
            exitButton.onClick.AddListener(CloseQuestPanel);
        }

        void OpenOnly(GameObject panelToOpen)
        {
            // 둘 다 끄고
            dailyQuestPanel.SetActive(false);
            infiniteQuestPanel.SetActive(false);

            // 선택된 것만 켜기
            panelToOpen.SetActive(true);
        }

        void CloseQuestPanel()
        {
            questPanel.SetActive(false);
        }
    }
}
