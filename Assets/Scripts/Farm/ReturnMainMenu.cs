using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vampire
{
    public class ReturnMainMenu : MonoBehaviour
    {
        public void OnClickBackToMenu()
        {
            SceneManager.LoadScene(0); // 메인메뉴 씬 이름이 정확히 일치해야 함
        }
    }
}
