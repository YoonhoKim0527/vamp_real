using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vampire
{
    public class SceneJumpController : MonoBehaviour
    {
        // MainMenu 씬 이름 (빌드 세팅에서 정확히 일치해야 함)
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        // 열어야 할 다이얼로그 타입
        private static string dialogToOpen = null;

        // 외부에서 호출할 메서드
        public void GoToMainMenu() => LoadMainMenu(null);
        public void GoToShop()      => LoadMainMenu("Shop");
        public void GoToEquip()     => LoadMainMenu("Equip");
        public void GoToUpgrade()   => LoadMainMenu("Upgrade");
        public void GoToStart()     => LoadMainMenu("Start");

        private void LoadMainMenu(string dialog)
        {
            dialogToOpen = dialog;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        // 메인메뉴 씬 진입 시 호출됨
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoaded()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu" && dialogToOpen != null)
            {
                var dialogManager = Object.FindObjectOfType<MainMenuDialogManager>(true);
                if (dialogManager != null)
                {
                    dialogManager.OpenDialog(dialogToOpen);
                    Debug.Log($"[SceneJumpController] '{dialogToOpen}' dialog opened.");
                }
                else
                {
                    Debug.LogWarning("[SceneJumpController] MainMenuDialogManager not found.");
                }

                dialogToOpen = null; // 초기화
            }
        }
    }
}
