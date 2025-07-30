using UnityEngine;

namespace Vampire
{
    public class MainMenuDialogManager : MonoBehaviour
    {
        [SerializeField] private GameObject shopDialog;
        [SerializeField] private GameObject equipDialog;
        [SerializeField] private GameObject upgradeDialog;
        [SerializeField] private GameObject startDialog;

        public void OpenDialog(string dialogName)
        {
            // 모든 다이얼로그 비활성화
            shopDialog?.SetActive(false);
            equipDialog?.SetActive(false);
            upgradeDialog?.SetActive(false);
            startDialog?.SetActive(false);

            switch (dialogName)
            {
                case "Shop":    shopDialog?.SetActive(true); break;
                case "Equip":   equipDialog?.SetActive(true); break;
                case "Upgrade": upgradeDialog?.SetActive(true); break;
                case "Start":   startDialog?.SetActive(true); break;
                default: Debug.LogWarning($"[MainMenuDialogManager] Unknown dialog: {dialogName}"); break;
            }
        }
    }
}
