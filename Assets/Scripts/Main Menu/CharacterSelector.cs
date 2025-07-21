using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Vampire
{
    public class CharacterSelector : MonoBehaviour
    {
        [SerializeField] CharacterBlueprint[] characterBlueprints;
        [SerializeField] GameObject characterCardPrefab;
        [SerializeField] CoinDisplay coinDisplay;
        [SerializeField] Transform cardContainer;

        CharacterCard[] characterCards;

        void OnEnable() // UI               ȣ   
        {
            Init();
        }

        public void Init()
        {
            //               ī        ( ߺ      )
            if (characterCards != null)
            {
                foreach (var card in characterCards)
                    Destroy(card.gameObject);
            }

            characterCards = new CharacterCard[characterBlueprints.Length];

            for (int i = 0; i < characterBlueprints.Length; i++)
            {
                var cardObj = Instantiate(characterCardPrefab, cardContainer);
                var card = cardObj.GetComponent<CharacterCard>();
                card.Init(this, characterBlueprints[i], coinDisplay);
                characterCards[i] = card;
            }
        }

        public void StartGame(CharacterBlueprint blueprint)
        {
            CrossSceneData.CharacterBlueprint = blueprint;

            if (CrossSceneData.LevelBlueprint == null)
            {
                Debug.LogError("LevelBlueprint is null!         õ     ʾҽ  ϴ .");
                return;
            }

            SceneManager.LoadScene(1); //              ̵ 
        }

    }
}