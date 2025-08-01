using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class EquipmentGacha : MonoBehaviour
    {
        [Header("Shop Blueprint (Tier 없음)")]
        [SerializeField] private ShopEquipmentBlueprint shopBlueprint;

        [Header("UI - Single")]
        [SerializeField] private Image resultIcon;
        [SerializeField] private TMP_Text resultNameText;

        [Header("UI - 10연차")]
        [SerializeField] private GameObject gacha10ResultPanel;
        [SerializeField] private Transform gridParent;
        [SerializeField] private GameObject itemSlotPrefab;

        [Header("Gacha Settings")]
        [SerializeField] private int singleDrawCost = 100;
        [SerializeField] private int multiDrawCost = 1000;

        private const int drawCount = 10;

        [SerializeField] private GameObject rubyShopPanel;
        [SerializeField] private GachaChestController chestController;

        [Header("Single Result Popup")]
        [SerializeField] private GameObject singleResultPopup;
        [SerializeField] private Image popupIconImage;
        [SerializeField] private TMP_Text popupNameText;

        public void OnSingleDrawButtonClick()
        {
            if (RubyManager.Instance == null || !RubyManager.Instance.SpendRubies(singleDrawCost))
            {
                RubyManager.Instance.ShowNotEnoughRubyPopup();
                return;
            }

            StartCoroutine(SingleDrawRoutine());
        }

        private IEnumerator SingleDrawRoutine()
        {
            chestController.ResetGlowImmediately();
            yield return chestController.PlayChestSequence();

            Equipment drawn = DrawAndSaveEquipment();

            // ✅ 기존 UI는 비활성화 (선택)
            resultIcon.enabled = false;

            // ✅ 새 팝업 UI에 정보 반영
            popupIconImage.sprite = drawn.icon;
            popupNameText.text = drawn.name;

            // ✅ 팝업 활성화
            singleResultPopup.SetActive(true);
        }


        public void OnCloseSingleResultPopup()
        {
            singleResultPopup.SetActive(false);
        }

        public void OnMultiDrawButtonClick()
        {
            if (RubyManager.Instance == null || !RubyManager.Instance.SpendRubies(multiDrawCost))
            {
                Debug.Log("루비 부족 (10연차)");
                return;
            }

            StartCoroutine(MultiDrawRoutine());
        }

        private IEnumerator MultiDrawRoutine()
        {
            chestController.ResetGlowImmediately();
            yield return chestController.PlayChestSequence();

            yield return StartCoroutine(PlayMultiDrawAnimation());
        }

        private IEnumerator PlayMultiDrawAnimation()
        {
            foreach (Transform child in gridParent)
                Destroy(child.gameObject);

            gacha10ResultPanel.SetActive(true);

            for (int i = 0; i < drawCount; i++)
            {
                Equipment equip = DrawAndSaveEquipment();

                GameObject slot = Instantiate(itemSlotPrefab, gridParent);
                Image icon = slot.transform.Find("Icon").GetComponent<Image>();
                Image background = slot.GetComponent<Image>();

                icon.sprite = equip.icon;
                SetTierColor(background, equip.tier);

                yield return new WaitForSeconds(0.5f);
            }
        }

        private Equipment DrawAndSaveEquipment()
        {
            int tier = RollTier();
            ShopEquipment baseEquip = shopBlueprint.equipments[
                Random.Range(0, shopBlueprint.equipments.Count)];

            Equipment fullEquip = new Equipment
            {
                name = baseEquip.name,
                icon = baseEquip.icon,
                type = baseEquip.type,
                multiply = baseEquip.multiply,
                tier = tier
            };

            EquipmentBlueprint playerBlueprint = FindObjectOfType<EquipmentManager>()?.GetPlayerBlueprint();
            if (playerBlueprint != null)
            {
                playerBlueprint.equipments.Add(fullEquip);
                EquipmentManager eqManager = FindObjectOfType<EquipmentManager>();
                if (eqManager != null && eqManager.isInitialized)
                    eqManager.RefreshCurrentTab();
            }
            else
            {
                Debug.LogWarning("[Gacha] EquipmentManager 또는 Blueprint를 찾을 수 없습니다.");
            }

            return fullEquip;
        }

        private int RollTier()
        {
            float roll = Random.value;
            if (roll < 0.50f) return 1;
            else if (roll < 0.80f) return 2;
            else if (roll < 0.95f) return 3;
            else return 4;
        }

        private void SetTierColor(Image background, int tier)
        {
            switch (tier)
            {
                case 2: background.color = Color.red; break;
                case 3: background.color = Color.yellow; break;
                case 4: background.color = Color.white; break;
                default: background.color = Color.blue; break;
            }
        }

        private void OpenRubyShop()
        {
            if (rubyShopPanel != null)
                rubyShopPanel.SetActive(true);
            else
                Debug.LogWarning("루비 상점 UI가 연결되지 않았습니다.");
        }
    }
}
