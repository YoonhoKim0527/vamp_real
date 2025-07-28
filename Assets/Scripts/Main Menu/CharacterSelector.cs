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

            var gsm = FindObjectOfType<GameStateManager>();
            if (gsm != null)
            {
                gsm.LoadGame(); // ✅ 업그레이드 반영된 playerStats 불러오기

                gsm.SetSelectedCharacter(blueprint);           // ✅ 캐릭터 설정
                gsm.ApplyCharacterMultipliers();               // ✅ 곱연산 적용
                gsm.ApplyEquipmentMultipliers();         // ✅ 장비 곱셈 (추가!)
                Debug.Log($"[CharacterSelector] 캐릭터 설정 + 곱연산 적용: {blueprint.name}");
            }
            else
            {
                Debug.LogWarning("[CharacterSelector] GameStateManager 없음.");
            }

            if (CrossSceneData.LevelBlueprint == null)
            {
                Debug.LogError("LevelBlueprint is null!");
                return;
            }

            SceneManager.LoadScene(1); // GameScene
        }

    }
}