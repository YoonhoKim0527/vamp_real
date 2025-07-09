using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vampire
{
    public class CharacterSelector : MonoBehaviour
    {
        [SerializeField] CharacterBlueprint[] characterBlueprints;
        [SerializeField] GameObject characterCardPrefab;
        [SerializeField] CoinDisplay coinDisplay;
        [SerializeField] Transform cardContainer;

        CharacterCard[] characterCards;

        void OnEnable() // UI가 켜질 때마다 호출됨
        {
            Init();
        }

        public void Init()
        {
            // 이전에 생성된 카드 제거 (중복 방지)
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
                Debug.LogError("LevelBlueprint is null! 맵이 선택되지 않았습니다.");
                return;
            }

            SceneManager.LoadScene(1); // 게임 씬으로 이동
        }
    }
}