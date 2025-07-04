using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private CharacterSelector characterSelector;
        [SerializeField] private Shop shop;
        [SerializeField] private Upgrade upgrade;

        void Start()
        {
            characterSelector.Init();
            shop.Init();
            upgrade.Init();
        }
    }
}
