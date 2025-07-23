using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Vampire
{
    public class ExpeditionDamageText : MonoBehaviour
    {
        private ExpeditionEntityManager entityManager;
        private TextMeshPro text;

        void Awake()
        {
            text = GetComponent<TextMeshPro>();
        }

        public void Init(ExpeditionEntityManager entityManager)
        {
            this.entityManager = entityManager;
        }

        public void Setup(Vector2 position, float damage, bool isCritical = false)
        {
            // 🔄 무작위로 x/y를 살짝 흔들기
            Vector2 randomOffset = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0f, 1f));
            transform.position = position + randomOffset;

            text.text = damage.ToString("N0");
            text.color = isCritical ? Color.red : Color.white;
            text.fontSize = isCritical ? 5f : 3f;

            StopAllCoroutines();
            StartCoroutine(AnimateText());
        }

        IEnumerator AnimateText()
        {
            float t = 0;
            while (t < 1)
            {
                transform.position += Vector3.up * Time.deltaTime * 0.5f;
                transform.localScale = Vector3.one * EasingUtils.EaseOutBack(1 - t);
                yield return null;
                t += Time.deltaTime * 2;
            }
            entityManager.DespawnDamageText(this);
        }
    }
}
