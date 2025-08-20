using UnityEngine;

namespace Vampire
{
    public static class CharacterSave
    {
        const string Key = "last_character_asset_name";

        public static void SaveByAssetName(CharacterBlueprint bp)
        {
            if (!bp) return;
            var assetName = ((Object)bp).name; // 에셋 파일명
            PlayerPrefs.SetString(Key, assetName);
            PlayerPrefs.Save();
        }

        public static bool TryLoadAssetName(out string assetName)
        {
            if (PlayerPrefs.HasKey(Key))
            {
                assetName = PlayerPrefs.GetString(Key);
                return !string.IsNullOrEmpty(assetName);
            }
            assetName = null;
            return false;
        }

        public static void Clear() => PlayerPrefs.DeleteKey(Key);
    }
}
