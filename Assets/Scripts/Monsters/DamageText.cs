using System.Collections;
using UnityEngine;
using TMPro;

namespace Vampire
{
    public class DamageText : MonoBehaviour
    {
        private EntityManager entityManager;
        private TextMeshPro text;

        void Awake()
        {
            text = GetComponent<TextMeshPro>();
        }

        public void Init(EntityManager entityManager)
        {
            this.entityManager = entityManager;
        }

        public void Setup(Vector2 position, float damage, bool isCritical = false)
        {
            transform.position = position;
            text.text = damage.ToString("N0");

            // ✅ 크리티컬 히트면 빨간색
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
