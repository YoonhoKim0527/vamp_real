using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class SelectedCharacterDisplay : MonoBehaviour
    {
        [Header("Character Animation")]
        [SerializeField] Image characterImage;

        [Header("Character Stats Text")]
        [SerializeField] TextMeshProUGUI damageText;
        [SerializeField] TextMeshProUGUI hpText;
        [SerializeField] TextMeshProUGUI hpRecoveryText;
        [SerializeField] TextMeshProUGUI armorText;
        [SerializeField] TextMeshProUGUI moveSpeedText;
        [SerializeField] TextMeshProUGUI luckText;

        [Header("Starting Ability")]
        [SerializeField] Image startingAbilityImage;

        Sprite[] walkSprites;
        float frameTime;
        int frameIndex;
        float timer;
        bool isActive = false;

        public void Display(CharacterBlueprint blueprint)
        {
            // ✅ 애니메이션 설정
            if (blueprint.walkSpriteSequence == null || blueprint.walkSpriteSequence.Length == 0)
            {
                Debug.LogWarning("⚠️ 캐릭터 스프라이트 없음!");
                return;
            }

            walkSprites = blueprint.walkSpriteSequence;
            frameTime = blueprint.walkFrameTime;
            frameIndex = 0;
            timer = 0f;
            isActive = true;
            characterImage.sprite = walkSprites[0];

            // ✅ 능력치 텍스트 설정
            damageText.text = $"Damage : x {blueprint.baseDamage:F1}";
            hpText.text = $"HP : x {blueprint.hp:F1}";
            hpRecoveryText.text = $"HP Recovery : x {blueprint.recovery:F1}";
            armorText.text = $"Armor : x {blueprint.armor:F1}";
            moveSpeedText.text = $"Move Speed : x {blueprint.movespeed:F2}";
            luckText.text = $"Luck : x {blueprint.luck:F2}";

            // ✅ 시작 어빌리티 아이콘 표시
            if (blueprint.startingAbilities != null && blueprint.startingAbilities.Length > 0)
            {
                var abilityGO = blueprint.startingAbilities[0];
                if (abilityGO != null)
                {
                    var ability = abilityGO.GetComponent<Ability>();
                    if (ability != null && ability.Image != null)
                    {
                        startingAbilityImage.sprite = ability.Image;
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Ability가 없거나 이미지가 비어 있습니다: {abilityGO.name}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Starting Ability가 없습니다.");
            }
        }

        void Update()
        {
            if (!isActive || walkSprites == null || walkSprites.Length == 0) return;

            timer += Time.deltaTime;
            if (timer >= frameTime)
            {
                frameIndex = (frameIndex + 1) % walkSprites.Length;
                characterImage.sprite = walkSprites[frameIndex];
                timer = 0f;
            }
        }
    }
}
