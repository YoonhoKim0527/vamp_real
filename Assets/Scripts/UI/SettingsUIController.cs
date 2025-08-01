using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class SettingsUIController : MonoBehaviour
    {
        [SerializeField] Button settingsButton;
        [SerializeField] Button closeButton;
        [SerializeField] GameObject settingsPanel;

        void Start()
        {
            // 시작 시 비활성화
            settingsPanel.SetActive(false);

            // 이벤트 연결
            settingsButton.onClick.AddListener(OpenSettings);
            closeButton.onClick.AddListener(CloseSettings);
        }

        void OpenSettings()
        {
            settingsPanel.SetActive(true);
        }

        void CloseSettings()
        {
            settingsPanel.SetActive(false);
        }
    }
}
