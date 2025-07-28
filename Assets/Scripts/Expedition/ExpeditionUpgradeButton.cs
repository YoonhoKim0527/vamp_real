using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class ExpeditionUpgradeButton : MonoBehaviour
    {
        [SerializeField] string statName;
        [SerializeField] TextMeshProUGUI levelText;

        SaveManager saveManager;

        void Start()
        {
            saveManager = FindObjectOfType<SaveManager>();
            UpdateUI();
        }

        public void OnClickUpgrade()
        {
            var data = saveManager.GetExpeditionUpgradeData();

            // ✅ 업그레이드 수치 증가
            switch (statName)
            {
                case "Damage": data.damageLevel++; break;
                case "Interval": data.intervalLevel++; break;
                case "Gold": data.goldGainLevel++; break;
                case "Emerald": data.emeraldGainLevel++; break;
            }

            // ✅ 저장
            saveManager.SetExpeditionUpgradeData(data);

            // ✅ 능력 전부 찾아서 데미지 + 주기 모두 갱신
            var abilities = FindObjectsOfType<BaseExpeditionAbility>();
            foreach (var ability in abilities)
            {
                ability.RefreshStats(); // 👈 한 번에 다 처리
            }

            UpdateUI();
        }

        void UpdateUI()
        {
            var data = saveManager.GetExpeditionUpgradeData();
            int level = statName switch
            {
                "Damage" => data.damageLevel,
                "Interval" => data.intervalLevel,
                "Gold" => data.goldGainLevel,
                "Emerald" => data.emeraldGainLevel,
                _ => 0
            };
            levelText.text = $"Lv {level}";
        }
    }

}
