using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;

namespace Vampire
{
    public class CharacterCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image characterImage;
        [SerializeField] private RectTransform characterImageRect;
        [SerializeField] private TextMeshProUGUI baseDamageText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI armorText;
        [SerializeField] private TextMeshProUGUI mvspdText;
        [SerializeField] private TextMeshProUGUI luckText;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private LocalizedString buyLocalization, selectLocalization;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Color selectColor, buyColor;
        [SerializeField] private RectTransform startingAbilitiesParent;
        [SerializeField] private GameObject startingAbilityContainerPrefab;
        [SerializeField] private Vector2 startingAbilitiesRectSize = new Vector2(365, 85);
        private CharacterSelector characterSelector;
        private CharacterBlueprint characterBlueprint;
        private CoinDisplay coinDisplay;
        private StartingAbilityContainer[] startingAbilityContainers;
        private bool initialized;

        private void OnEnable()
        {
            buyLocalization.StringChanged += UpdateButtonText;
        }

        private void OnDisable()
        {
            buyLocalization.StringChanged -= UpdateButtonText;
        }

        public void Init(CharacterSelector characterSelector, CharacterBlueprint characterBlueprint, CoinDisplay coinDisplay)
        {
            this.characterSelector = characterSelector;
            this.characterBlueprint = characterBlueprint;
            this.coinDisplay = coinDisplay;

            characterImage.sprite = characterBlueprint.walkSpriteSequence[0];

            nameText.text = characterBlueprint.name.ToString();
            baseDamageText.text = characterBlueprint.baseDamage.ToString();
            hpText.text = characterBlueprint.hp.ToString();
            armorText.text = characterBlueprint.armor.ToString();
            mvspdText.text = Mathf.RoundToInt(characterBlueprint.movespeed/1.15f * 100f).ToString()+"%";
            luckText.text = characterBlueprint.luck.ToString();
            UpdateButtonText();
            buttonImage.color = characterBlueprint.owned ? selectColor : buyColor;

            // Instantiate the images
            startingAbilityContainers = new StartingAbilityContainer[characterBlueprint.startingAbilities.Length];
            for (int i = 0; i < characterBlueprint.startingAbilities.Length; i++)
            {
                startingAbilityContainers[i] = Instantiate(startingAbilityContainerPrefab, startingAbilitiesParent).GetComponent<StartingAbilityContainer>();
                var abilityGO = characterBlueprint.startingAbilities[i];
                if (abilityGO == null)
                {
                    Debug.LogError($"[CharacterCard] {characterBlueprint.name}의 Starting Ability {i}가 null입니다.");
                    continue;
                }

                var ability = abilityGO.GetComponent<Ability>();
                if (ability == null)
                {
                    Debug.LogError($"[CharacterCard] {abilityGO.name}에는 Ability 컴포넌트가 없습니다.");
                    continue;
                }

                if (ability.Image == null)
                {
                    Debug.LogWarning($"[CharacterCard] {abilityGO.name}의 Ability Image가 null입니다.");
                }

                startingAbilityContainers[i].AbilityImage.sprite = ability.Image;
            }

            initialized = true;
        }

        public void UpdateLayout()
        {
            // Character image layout
            float yHeight = Mathf.Abs(characterImageRect.sizeDelta.y);
            float xWidth = characterBlueprint.walkSpriteSequence[0].textureRect.width / (float) characterBlueprint.walkSpriteSequence[0].textureRect.height * yHeight;
            if (xWidth > Mathf.Abs(characterImageRect.sizeDelta.x))
            {
                xWidth = Mathf.Abs(characterImageRect.sizeDelta.x);
                yHeight = characterBlueprint.walkSpriteSequence[0].textureRect.height / (float) characterBlueprint.walkSpriteSequence[0].textureRect.width * xWidth;
            }
            ((RectTransform)characterImage.transform).sizeDelta = new Vector2(xWidth, yHeight);
            
            // Character abilities layout
            float maxImageWidth = startingAbilitiesRectSize.x / startingAbilityContainers.Length;
            for (int i = 0; i < startingAbilityContainers.Length; i++)
            {
                StartingAbilityContainer startingAbilityContainer = startingAbilityContainers[i];
                float imageHeight = startingAbilitiesRectSize.y;
                float imageWidth = startingAbilityContainer.AbilityImage.sprite.textureRect.width / (float) startingAbilityContainer.AbilityImage.sprite.textureRect.height * imageHeight;
                if (imageWidth > maxImageWidth)
                {
                    imageWidth = maxImageWidth;
                    imageHeight = startingAbilityContainer.AbilityImage.sprite.textureRect.height / (float) startingAbilityContainer.AbilityImage.sprite.textureRect.width * imageWidth;
                }
                startingAbilityContainer.ImageRect.sizeDelta = new Vector2(imageWidth, imageHeight);
            }
        }

        public void Selected()
        {
            if (!characterBlueprint.owned)
            {
                int coinCount = CoinManager.Instance.GetCoins(); // ✅ GetCoins() 사용
                if (coinCount >= characterBlueprint.cost)
                {
                    bool success = CoinManager.Instance.SpendCoins(characterBlueprint.cost); // ✅ 차감

                    if (success)
                    {
                        characterBlueprint.owned = true;
                        UpdateButtonText();
                        buttonImage.color = selectColor;
                    }
                }
                else
                {
                    Debug.LogWarning("코인이 부족합니다.");
                }
            }
            else
            {
                characterSelector.StoreSelectedCharacter(characterBlueprint);
                FindObjectOfType<SelectedCharacterDisplay>()?.Display(characterBlueprint);
            }
        }

        private void UpdateButtonText(string text)
        { 
            UpdateButtonText();
        }
        
        private void UpdateButtonText()
        {
            if (!initialized) return;
            
            if (characterBlueprint.owned)
            {
                buttonText.text = selectLocalization.GetLocalizedString();
            }
            else
            {
                buttonText.text = String.Format("{0} (${1})", buyLocalization.GetLocalizedString(), characterBlueprint.cost);
            }
        }
    }
}
