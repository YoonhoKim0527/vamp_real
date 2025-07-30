using UnityEngine;
using TMPro;
using System.Collections;

namespace Vampire
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class RubyDisplay : MonoBehaviour
    {
        private TextMeshProUGUI rubyText;

        private void Awake()
        {
            rubyText = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            StartCoroutine(WaitForRubyManager());
        }

        private IEnumerator WaitForRubyManager()
        {
            // RubyManager가 null이 아닐 때까지 기다림
            yield return new WaitUntil(() => RubyManager.Instance != null);

            // 이벤트 구독
            RubyManager.Instance.OnRubiesChanged += UpdateDisplay;

            // 초기 표시
            UpdateDisplay(RubyManager.Instance.GetRubies());

            Debug.Log("[RubyDisplay] RubyManager 연결 완료 및 초기 표시");
        }

        private void OnDisable()
        {
            // 안전하게 구독 해제
            if (RubyManager.Instance != null)
                RubyManager.Instance.OnRubiesChanged -= UpdateDisplay;
        }

        private void UpdateDisplay(int rubies)
        {
            rubyText.text = rubies.ToString();
            Debug.Log($"[RubyDisplay] 루비 수 갱신: {rubies}");
        }
    }
}
