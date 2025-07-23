using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Vampire
{
    public class BoostManager : MonoBehaviour
    {
        public static BoostManager Instance;

        [SerializeField] TMP_Text boostStatusText; // ✅ UI 연결용 텍스트

        Dictionary<BoostType, float> activeBoosts = new();
        Dictionary<BoostType, Coroutine> runningCoroutines = new();
        Dictionary<BoostType, float> remainingTimes = new();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Update()
        {
            if (remainingTimes.Count == 0)
            {
                boostStatusText.text = "";
                return;
            }

            UpdateBoostUI();
        }

        public float GetMultiplier(BoostType type)
        {
            return activeBoosts.ContainsKey(type) ? activeBoosts[type] : 1f;
        }

        public void ActivateBoost(BoostType type, float multiplier, float duration)
        {
            // multiplier 항상 재설정
            activeBoosts[type] = multiplier;

            // 기존 시간 있으면 누적, 없으면 새로 등록
            if (remainingTimes.ContainsKey(type))
                remainingTimes[type] += duration;
            else
                remainingTimes[type] = duration;

            // 코루틴이 없을 때만 새로 시작
            if (!runningCoroutines.ContainsKey(type))
            {
                Coroutine co = StartCoroutine(BoostCoroutine(type));
                runningCoroutines[type] = co;
            }
        }

        IEnumerator BoostCoroutine(BoostType type)
        {
            while (remainingTimes[type] > 0f)
            {
                remainingTimes[type] -= Time.deltaTime;
                yield return null;
            }

            activeBoosts[type] = 1f;
            remainingTimes.Remove(type);
            runningCoroutines.Remove(type);
        }

        void UpdateBoostUI()
        {
            List<string> lines = new();

            foreach (var pair in remainingTimes)
            {
                string label = pair.Key switch
                {
                    BoostType.Damage => "damage x2",
                    BoostType.AttackSpeed => "attack speed x2",
                    _ => pair.Key.ToString()
                };

                lines.Add($"{label}: {Mathf.CeilToInt(pair.Value)}s");
            }

            boostStatusText.text = string.Join("\n", lines);
        }
    }
}
