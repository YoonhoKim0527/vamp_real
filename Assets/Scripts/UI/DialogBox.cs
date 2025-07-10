using System.Collections;
using UnityEngine;
using TMPro;

namespace Vampire
{
    public class DialogBox : MonoBehaviour
    {
        [SerializeField] private bool appearInstantly = false;
        [SerializeField] private float animationSpeed;
        [SerializeField] private DialogBox previousDialog, nextDialog;

        // ✅ TMP fallback 폰트 (Inspector에서 지정)
        [SerializeField] private TMP_FontAsset fallbackFont;

        public virtual void Open()
        {
            // ✅ 다른 DialogBox 비활성화
            CloseAllOtherDialogs();

            // ✅ LocalizeFontEvent 방어: 폰트 없으면 fallback 적용
            var localizeFontEvent = GetComponentInChildren<LocalizeFontEvent>(true);
            if (localizeFontEvent != null)
            {
                var tmpUITextField = localizeFontEvent.GetType()
                    .GetField("_tmpUITexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(localizeFontEvent) as TextMeshProUGUI[];

                var tmpTextField = localizeFontEvent.GetType()
                    .GetField("_tmpTexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(localizeFontEvent) as TextMeshPro[];

                if (tmpUITextField != null)
                {
                    foreach (var tmpText in tmpUITextField)
                    {
                        if (tmpText != null && tmpText.font == null)
                        {
                            Debug.LogWarning($"[DialogBox] Missing font on TMP_UI_Text: {tmpText.name}. Applying fallback font.");
                            tmpText.font = fallbackFont;
                        }
                    }
                }

                if (tmpTextField != null)
                {
                    foreach (var tmpText in tmpTextField)
                    {
                        if (tmpText != null && tmpText.font == null)
                        {
                            Debug.LogWarning($"[DialogBox] Missing font on TMP_Text: {tmpText.name}. Applying fallback font.");
                            tmpText.font = fallbackFont;
                        }
                    }
                }
            }

            // ✅ 현재 Dialog 활성화
            gameObject.SetActive(true);

            if (appearInstantly)
            {
                transform.localScale = Vector3.one;
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(OpenAnimation());
            }
        }

        public virtual void Close()
        {
            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }

        public void Return()
        {
            previousDialog?.Open();
            Close();
        }

        public void Continue()
        {
            nextDialog?.Open();
            Close();
        }

        private void CloseAllOtherDialogs()
        {
            var allDialogs = FindObjectsOfType<DialogBox>(true); // 비활성화된 것도 포함
            foreach (var dialog in allDialogs)
            {
                if (dialog != this && dialog.gameObject.activeSelf)
                {
                    dialog.Close();
                }
            }
        }

        private IEnumerator OpenAnimation()
        {
            float t = 0;
            while (t < 1)
            {
                transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, EasingUtils.EaseOutBack(t));
                t += Time.unscaledDeltaTime * animationSpeed;
                yield return null;
            }
            transform.localScale = Vector3.one;
        }

        private IEnumerator CloseAnimation()
        {
            float t = 0;
            while (t < 1)
            {
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, EasingUtils.EaseOutQuart(t));
                t += Time.deltaTime * animationSpeed;
                yield return null;
            }
            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }
    }
}
