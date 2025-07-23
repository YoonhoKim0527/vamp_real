using UnityEngine;
using TMPro;
using System.Collections;

namespace Vampire
{
    public class CoinGainText : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] float moveUpDistance = 50f;
        [SerializeField] float duration = 1f;

        CanvasGroup canvasGroup;
        Vector3 startPos;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            startPos = transform.localPosition;
        }

        public void Show(int amount)
        {
            text.text = $"+{amount}";
            StopAllCoroutines();
            StartCoroutine(Animate());
        }

        IEnumerator Animate()
        {
            float t = 0f;
            Vector3 endPos = startPos + Vector3.up * moveUpDistance;

            transform.localPosition = startPos;
            canvasGroup.alpha = 1f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = t / duration;
                transform.localPosition = Vector3.Lerp(startPos, endPos, progress);
                canvasGroup.alpha = 1f - progress;
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
