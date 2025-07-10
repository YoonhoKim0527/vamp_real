#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public class TMPFontReplacerAll : EditorWindow
{
    TMP_FontAsset newFont;

    [MenuItem("Tools/Replace All TMP Fonts (Scene + Prefabs)")]
    public static void ShowWindow()
    {
        GetWindow<TMPFontReplacerAll>("TMP Font Replacer (Global)");
    }

    void OnGUI()
    {
        GUILayout.Label("TMP 폰트 전체 교체기", EditorStyles.boldLabel);
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("새 TMP 폰트", newFont, typeof(TMP_FontAsset), false);

        if (newFont == null)
        {
            EditorGUILayout.HelpBox("먼저 적용할 TMP_FontAsset을 선택하세요.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("씬 + 모든 프리팹 TMP 폰트 교체"))
        {
            int changedCount = 0;

            // 1. 현재 씬의 TMP 텍스트 교체
            var uiTexts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var tmp in uiTexts)
            {
                Undo.RecordObject(tmp, "Change TMP Font");
                tmp.font = newFont;
                EditorUtility.SetDirty(tmp);
                changedCount++;
            }

            var texts = FindObjectsOfType<TextMeshPro>(true);
            foreach (var tmp in texts)
            {
                Undo.RecordObject(tmp, "Change TMP Font");
                tmp.font = newFont;
                EditorUtility.SetDirty(tmp);
                changedCount++;
            }

            // 2. 프로젝트 전체 프리팹 탐색
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                bool modified = false;

                var allTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var tmp in allTexts)
                {
                    if (tmp.font != newFont)
                    {
                        Undo.RecordObject(tmp, "Change TMP Font in Prefab");
                        tmp.font = newFont;
                        EditorUtility.SetDirty(tmp);
                        modified = true;
                        changedCount++;
                    }
                }

                var allTexts3D = prefab.GetComponentsInChildren<TextMeshPro>(true);
                foreach (var tmp in allTexts3D)
                {
                    if (tmp.font != newFont)
                    {
                        Undo.RecordObject(tmp, "Change TMP Font in Prefab");
                        tmp.font = newFont;
                        EditorUtility.SetDirty(tmp);
                        modified = true;
                        changedCount++;
                    }
                }

                if (modified)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"✅ TMP 폰트를 전역으로 교체 완료! 총 {changedCount}개 텍스트 객체 수정됨.");
        }
    }
}
#endif
