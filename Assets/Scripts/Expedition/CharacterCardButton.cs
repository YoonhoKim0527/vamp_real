using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class CharacterCardButton : MonoBehaviour
    {
        [SerializeField] CharacterBlueprint blueprint;
        [SerializeField] Image characterImage;
        [SerializeField] Button button;
        [SerializeField] TextMeshProUGUI buttonText;

        ExpeditionManager expeditionManager;

        void Start()
        {
            expeditionManager = FindObjectOfType<ExpeditionManager>();
            button.onClick.AddListener(OnClick);
            UpdateUI();
        }

        void OnClick()
        {
            if (expeditionManager.IsCharacterSelected(blueprint))
            {
                expeditionManager.RemoveSelectedCharacter(blueprint);
            }
            else
            {
                expeditionManager.TrySelectCharacter(blueprint);
            }

            UpdateUI();
        }

        void UpdateUI()
        {
            // 버튼 텍스트 설정
            bool selected = expeditionManager.IsCharacterSelected(blueprint);
            buttonText.text = selected ? "out" : "in";

            // 캐릭터 이미지 설정
            if (characterImage != null && blueprint.walkSpriteSequence != null && blueprint.walkSpriteSequence.Length > 0)
            {
                characterImage.sprite = blueprint.walkSpriteSequence[0];
                characterImage.preserveAspect = true;
                characterImage.enabled = true;
            }
            else if (characterImage != null)
            {
                characterImage.enabled = false;
            }
        }

        // 외부에서 blueprint 주입할 경우 호출
        public void SetBlueprint(CharacterBlueprint bp)
        {
            blueprint = bp;
            UpdateUI();
        }
    }
}