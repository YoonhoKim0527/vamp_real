using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class BackgroundUIController : MonoBehaviour
    {
        [Header("Open Buttons")]
        [SerializeField] Button settingsButton;
        [SerializeField] Button questButton;
        [SerializeField] Button messengerButton;
        [SerializeField] Button attendanceButton;

        [Header("Panels")]
        [SerializeField] GameObject settingsPanel;
        [SerializeField] GameObject questPanel;
        [SerializeField] GameObject messengerPanel;
        [SerializeField] GameObject attendancePanel;

        [Header("Close Buttons")]
        [SerializeField] Button settingCloseButton;
        [SerializeField] Button questCloseButton;
        [SerializeField] Button messengerCloseButton;
        [SerializeField] Button attendanceCloseButton;

        void Start()
        {
            // 열기 버튼 먼저 연결
            settingsButton.onClick.AddListener(() => OpenExclusive(settingsPanel));
            questButton.onClick.AddListener(() => OpenExclusive(questPanel));
            messengerButton.onClick.AddListener(() => OpenExclusive(messengerPanel));
            attendanceButton.onClick.AddListener(() => OpenExclusive(attendancePanel));

            // 닫기 버튼 연결
            settingCloseButton.onClick.AddListener(() => ClosePanel(settingsPanel));
            questCloseButton.onClick.AddListener(() => ClosePanel(questPanel));
            messengerCloseButton.onClick.AddListener(() => ClosePanel(messengerPanel));
            attendanceCloseButton.onClick.AddListener(() => ClosePanel(attendancePanel));

            // 패널 비활성화는 마지막에
            Invoke(nameof(CloseAllPanels), 0.1f);
        }

        void OpenExclusive(GameObject targetPanel)
        {
            // 모두 닫고
            settingsPanel.SetActive(false);
            questPanel.SetActive(false);
            messengerPanel.SetActive(false);
            attendancePanel.SetActive(false);

            // 대상만 열기
            targetPanel.SetActive(true);
        }

        void ClosePanel(GameObject panel)
        {
            panel.SetActive(false);
        }

        void CloseAllPanels()
        {
            settingsPanel.SetActive(false);
            questPanel.SetActive(false);
            messengerPanel.SetActive(false);
            attendancePanel.SetActive(false);
        }
    }
}
