using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class StatsManagerUI : MonoBehaviour
    {
        [SerializeField] private GameObject statRowPrefab;
        [SerializeField] private Transform statListContainer;

        IEnumerator Start()
        {
            // ✅ GameStateManager가 초기화될 때까지 대기
            GameStateManager gameStateManager = null;
            while (gameStateManager == null || !gameStateManager.IsInitialized)
            {
                gameStateManager = FindObjectOfType<GameStateManager>();
                yield return null;
            }

            Debug.Log("[StatsManagerUI] GameStateManager is initialized. Displaying stats.");
            var stats = gameStateManager.PlayerStats;

            Dictionary<string, string> statMap = new Dictionary<string, string>
            {
                { "Attack Power", stats.attackPower.ToString("F1") },
                { "Max Health", stats.maxHealth.ToString("F1") },
                { "Move Speed", stats.moveSpeed.ToString("F1") },
                { "Defense", stats.defense.ToString("F1") },
                { "Health Regen", stats.healthRegen.ToString("F1") },
                { "Critical Chance", $"{(stats.criticalChance * 100f):F1}%" },
                { "Critical Damage", stats.criticalDamage.ToString("F1") },
                { "Extra Projectiles", stats.extraProjectiles.ToString() }
            };

            foreach (var stat in statMap)
            {
                GameObject row = Instantiate(statRowPrefab, statListContainer);
                StatsUI ui = row.GetComponent<StatsUI>();
                if (ui != null)
                    ui.SetStat(stat.Key, stat.Value);
                else
                    Debug.LogWarning("[StatsManagerUI] StatsUI component missing on prefab!");
            }
        }
    }
}
