using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class SceneButtonBinder : MonoBehaviour
    {
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button startButton;

        private void Start()
        {
            if (SceneJumpController.Instance == null)
            {
                Debug.LogWarning("[SceneButtonBinder] SceneJumpController not found");
                return;
            }

            mainMenuButton?.onClick.AddListener(SceneJumpController.Instance.GoToMainMenu);
            shopButton?.onClick.AddListener(SceneJumpController.Instance.GoToShop);
            equipButton?.onClick.AddListener(SceneJumpController.Instance.GoToEquip);
            upgradeButton?.onClick.AddListener(SceneJumpController.Instance.GoToUpgrade);
            startButton?.onClick.AddListener(SceneJumpController.Instance.GoToStart);
        }
    }
}
