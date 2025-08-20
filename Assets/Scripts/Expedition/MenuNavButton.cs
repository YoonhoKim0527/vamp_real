using UnityEngine;

namespace Vampire
{
    public class MenuNavButton : MonoBehaviour
    {
        public enum Target { MainMenu, Shop, Equip, Upgrade, Start }
        [SerializeField] Target target;

        public void Jump()
        {
            var nav = SceneJumpController.Instance;
            if (nav == null)
            {
                Debug.LogError("[MenuNavButton] SceneJumpController.Instance is null");
                return;
            }

            switch (target)
            {
                case Target.MainMenu: nav.GoToMainMenu(); break;
                case Target.Shop:     nav.GoToShop();     break;
                case Target.Equip:    nav.GoToEquip();    break;
                case Target.Upgrade:  nav.GoToUpgrade();  break;
                case Target.Start:    nav.GoToStart();    break;
            }
        }
    }
}