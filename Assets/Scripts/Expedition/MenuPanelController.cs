using UnityEngine;

namespace Vampire
{
    public class MenuPanelController : MonoBehaviour
    {
        [SerializeField] GameObject selectCharacterPanel;
        [SerializeField] GameObject watchAdPanel;
        [SerializeField] GameObject upgradePanel;

        GameObject activePanel;

        void Start()
        {
            // 초기 선택된 패널은 캐릭터 선택으로 가정
            ActivatePanel(selectCharacterPanel);
        }

        public void OnSelectCharacterClicked()
        {
            ActivatePanel(selectCharacterPanel);
        }

        public void OnWatchAdClicked()
        {
            ActivatePanel(watchAdPanel);
        }

        public void OnUpgradeClicked()
        {
            ActivatePanel(upgradePanel);
        }

        void ActivatePanel(GameObject target)
        {
            if (activePanel == target) return;

            selectCharacterPanel.SetActive(false);
            watchAdPanel.SetActive(false);
            upgradePanel.SetActive(false);

            target.SetActive(true);
            activePanel = target;
        }
    }
}
