using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Vampire
{
    public class LocaleDropdown : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;

        IEnumerator Start()
        {
            // Wait for the localization system to initialize
            yield return LocalizationSettings.InitializationOperation;

            var allowedLocales = new HashSet<string> { "en", "kr" }; // 원하는 언어 코드만
            var options = new List<TMP_Dropdown.OptionData>();
            int selected = 0;
            int dropdownIndex = 0;

            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
            {
                var locale = LocalizationSettings.AvailableLocales.Locales[i];

                if (!allowedLocales.Contains(locale.Identifier.Code))
                    continue;

                if (LocalizationSettings.SelectedLocale == locale)
                    selected = dropdownIndex;

                options.Add(new TMP_Dropdown.OptionData(locale.LocaleName));
                dropdownIndex++;
            }

            dropdown.options = options;
            dropdown.value = selected;
            dropdown.onValueChanged.AddListener(LocaleSelectedFiltered);
        }

        static void LocaleSelected(int index)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        }
                
        void LocaleSelectedFiltered(int index)
        {
            var filteredLocales = LocalizationSettings.AvailableLocales.Locales
                .FindAll(l => l.Identifier.Code == "en" || l.Identifier.Code == "kr");

            if (index >= 0 && index < filteredLocales.Count)
                LocalizationSettings.SelectedLocale = filteredLocales[index];
        }
    }
}