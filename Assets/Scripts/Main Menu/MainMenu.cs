using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vampire
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private DialogBox levelSelectDialog;
        [SerializeField] private Shop shop;
        [SerializeField] private Upgrade upgrade;

        public void OnClickStart()
        {
            levelSelectDialog.Open(); 
        }
        public void OnClickGoToFarm()
        {
            SceneManager.LoadScene(2); // �̸����� �ε�
        }

        void Start()
        {
            upgrade.Init();
        }
    }
}
