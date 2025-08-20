using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
        }

        // 외부 버튼에서 호출
        public void LoadGameScene()
        {
            if (selectedBlueprint == null)
            {
                Debug.LogWarning("❌ 캐릭터가 선택되지 않았습니다.");
                return;
            }
            StartCoroutine(PrepareAndLoad());
        }

        private IEnumerator PrepareAndLoad()
        {
            var gsm = FindObjectOfType<GameStateManager>();
            if (gsm != null)
            {
                yield return gsm.LoadGame(); // ✅ 로드 완료 대기
                gsm.SetSelectedCharacter(selectedBlueprint);

                // 기준 스탯 → 캐릭터 → 장비 순으로 재계산 추천
                // (RecomputeAllStatsFromBase()를 이전 답변대로 만들었다면 한 줄로 대체 가능)
                gsm.RecomputeAllStatsFromBase();

                Debug.Log($"[CharacterSelector] 게임 준비 완료: {selectedBlueprint.name}");
            }
            else
            {
                Debug.LogWarning("[CharacterSelector] GameStateManager 없음.");
            }

            if (CrossSceneData.LevelBlueprint == null)
            {
                Debug.LogError("LevelBlueprint is null!");
                yield break;
            }

            SceneManager.LoadScene(1);
        }
    }
}
