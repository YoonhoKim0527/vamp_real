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
        CharacterBlueprint selectedBlueprint;

        void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            // 기존 카드 정리
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

        // 선택된 캐릭터 저장만 함
        public void StoreSelectedCharacter(CharacterBlueprint blueprint)
        {
            selectedBlueprint = blueprint;
            CrossSceneData.CharacterBlueprint = blueprint;
            Debug.Log($"[CharacterSelector] 캐릭터 선택됨: {blueprint.name}");
            LoadGameScene();
        }

        // 외부 버튼에서 호출
        public void LoadGameScene()
        {
            if (selectedBlueprint == null)
            {
                Debug.LogWarning("❌ 캐릭터가 선택되지 않았습니다.");
                return;
            }

            var gsm = FindObjectOfType<GameStateManager>();
            if (gsm != null)
            {
                gsm.LoadGame(); // ✅ 업그레이드 반영된 playerStats 불러오기
                gsm.SetSelectedCharacter(selectedBlueprint);
                gsm.ApplyCharacterMultipliers();
                gsm.ApplyEquipmentMultipliers();
                Debug.Log($"[CharacterSelector] 게임 준비 완료: {selectedBlueprint.name}");
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
