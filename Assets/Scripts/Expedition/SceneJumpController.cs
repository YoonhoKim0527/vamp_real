using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vampire
{
    public class SceneJumpController : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private static SceneJumpController _instance;
        public static SceneJumpController Instance => _instance; // ✅ 외부 접근 허용
        private static string _pendingDialog = null;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // 중복 등록 방지
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ---------- 버튼용 메서드 ----------
        public void GoToMainMenu() => LoadMainMenu(null);
        public void GoToShop()      => LoadMainMenu("Shop");
        public void GoToEquip()     => LoadMainMenu("Equip");
        public void GoToUpgrade()   => LoadMainMenu("Upgrade");
        public void GoToStart()     => LoadMainMenu("Start");

        private void LoadMainMenu(string dialog)
        {
            // ✅ 씬 전환 전에 익스페디션 진행상황 저장
            var expeditionMgr = FindObjectOfType<Vampire.ExpeditionManager>(true);
            if (expeditionMgr != null)
            {
                Debug.Log("[SceneJumpController] Pre-save before loading MainMenu");
                expeditionMgr.ForceSave();
            }

            _pendingDialog = dialog;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != mainMenuSceneName || _pendingDialog == null) return;

            var dialogManager = FindObjectOfType<MainMenuDialogManager>(true);
            if (dialogManager != null)
            {
                dialogManager.OpenDialog(_pendingDialog);
                Debug.Log($"[SceneJumpController] Opened dialog '{_pendingDialog}' in scene '{scene.name}'");
            }
            else
            {
                Debug.LogWarning("[SceneJumpController] MainMenuDialogManager not found.");
            }

            _pendingDialog = null;
        }
    }
}
