using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace Vampire
{
    [Serializable]
    public class UnityEventTMPFont : UnityEvent<TMP_FontAsset> {}
    
    [AddComponentMenu("Localization/Asset/Localize Font Event")]
    public class LocalizeFontEvent : LocalizedAssetEvent<TMP_FontAsset, LocalizedTmpFont, UnityEventTMPFont>
    {
        [SerializeField] private TextMeshProUGUI[] _tmpUITexts;
        [SerializeField] private TextMeshPro[] _tmpTexts;
        
        protected override void UpdateAsset(TMP_FontAsset font)
        {
            base.UpdateAsset(font);

            if (font == null)
            {
                Debug.LogWarning("[LocalizeFontEvent] Font asset is null. Skipping font update.");
                return;
            }

            // ✅ 자동 스캔: 등록 안 된 TextMeshProUGUI도 찾아서 폰트 적용
            var allTMPUIs = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTMPUIs)
            {
                if (tmp != null)
                {
                    tmp.font = font;
                }
            }

            // ✅ 기존 배열 처리
            if (_tmpUITexts != null && _tmpUITexts.Length > 0)
            {
                foreach (var tmp in _tmpUITexts)
                {
                    if (tmp != null)
                    {
                        tmp.font = font;
                    }
                }
            }

            if (_tmpTexts != null && _tmpTexts.Length > 0)
            {
                foreach (var tmp in _tmpTexts)
                {
                    if (tmp != null)
                    {
                        tmp.font = font;
                    }
                }
            }
        }


    }
}