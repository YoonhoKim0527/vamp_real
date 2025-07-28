using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public void OnSingleDrawButtonClick()
        {
            if (RubyManager.Instance == null || !RubyManager.Instance.SpendRubies(singleDrawCost))
            {
                RubyManager.Instance.ShowNotEnoughRubyPopup();
                return;
            }

            Equipment drawn = DrawAndSaveEquipment(); // ✅ 저장까지

            resultIcon.sprite = drawn.icon;
            resultIcon.enabled = true;
            resultNameText.text = drawn.name;

            SetTierColor(resultIcon.GetComponentInParent<Image>(), drawn.tier);
        }

        public void OnMultiDrawButtonClick()
        {
            if (RubyManager.Instance == null || !RubyManager.Instance.SpendRubies(multiDrawCost))
            {
                Debug.Log("루비 부족 (10연차)");
                return;
            }

            StartCoroutine(PlayMultiDrawAnimation());
        }

        private IEnumerator PlayMultiDrawAnimation()
        {
            foreach (Transform child in gridParent)
                Destroy(child.gameObject);

            gacha10ResultPanel.SetActive(true);

            for (int i = 0; i < drawCount; i++)
            {
                Equipment equip = DrawAndSaveEquipment(); // ✅ 저장까지

                GameObject slot = Instantiate(itemSlotPrefab, gridParent);
                Image icon = slot.transform.Find("Icon").GetComponent<Image>();
                Image background = slot.GetComponent<Image>();

                icon.sprite = equip.icon;
                SetTierColor(background, equip.tier);

                yield return new WaitForSeconds(0.5f);
            }
        }

        /// <summary>
        /// Shop Blueprint에서 무작위로 장비를 뽑고, tier를 붙여 Equipment로 변환한 후 Player Blueprint에 저장함
        /// </summary>
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

            // ✅ Player Blueprint에 저장
            EquipmentBlueprint playerBlueprint = FindObjectOfType<EquipmentManager>()?.GetPlayerBlueprint();
            if (playerBlueprint != null)
            {
                playerBlueprint.equipments.Add(fullEquip);

                // ✅ 추가: 바로 UI 새로고침
                EquipmentManager eqManager = FindObjectOfType<EquipmentManager>();
                if (eqManager != null && eqManager.isInitialized)
                {
                    eqManager.RefreshCurrentTab(); // 🔧 이 함수는 우리가 새로 추가한 것
                }
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
                case 2:
                    background.color = Color.red;
                    break;
                case 3:
                    background.color = Color.yellow;
                    break;
                case 4:
                    background.color = Color.white;
                    break;
                default:
                    background.color = Color.blue;
                    break;
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
