using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vampire
{
    public class ReturnMainMenu : MonoBehaviour
    {
        public void OnClickBackToMenu()
        {
            var expeditionManager = FindObjectOfType<ExpeditionManager>();
            if (expeditionManager != null)
            {
                Debug.Log("🏠 메인메뉴 이동 전 저장 실행");
                expeditionManager.SendMessage("SaveExpeditionData");
            }

            SceneManager.LoadScene(0); // 또는 정확한 메인메뉴 씬 이름 사용
        }
    }
}
