using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ScriptGuidMigrator : EditorWindow
{
    private MonoScript oldScript;
    private MonoScript newScript;

    private string oldGuid = "";
    private string newGuid = "";

    private int foundCount;
    private int replacedCount;

    private bool hasScanned;
    private bool isReplacing;

    private string[] targetFiles = Array.Empty<string>();

    private Vector2 scrollPosition;

    // 可處理的 Unity 序列化檔案
    private static readonly string[] Extensions =
    {
        ".unity",
        ".prefab",
        ".asset",
        ".controller",
        ".anim",
        ".overrideController",
        ".mat",
        ".playable",
        ".mask"
    };

    [MenuItem("Tools/UPM/Script GUID Migrator")]
    private static void OpenWindow()
    {
        GetWindow<ScriptGuidMigrator>("Script GUID Migrator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(
            "UPM Script GUID Migrator",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "將 Assets 內引用舊腳本 GUID 的序列化資產，" +
            "轉換成引用 UPM Package 的腳本 GUID。\n" +
            "不會修改 Package 內的檔案。",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        oldScript = (MonoScript)EditorGUILayout.ObjectField("Old Script", oldScript, typeof(MonoScript), false);

        newScript = (MonoScript)EditorGUILayout.ObjectField(
            "UPM Script",
            newScript,
            typeof(MonoScript),
            false
        );

        // 自動取得 GUID
        oldGuid = GetScriptGuid(oldScript);
        newGuid = GetScriptGuid(newScript);

        // 顯示讀取到的 GUID
        EditorGUILayout.LabelField(
            "Old GUID",
            oldGuid
        );

        EditorGUILayout.LabelField(
            "UPM GUID",
            newGuid
        );

        EditorGUILayout.Space(10);

        using (new EditorGUI.DisabledScope(!IsGuidValid()))
        {
            if (GUILayout.Button("1. Scan References", GUILayout.Height(30)))
            {
                ScanReferences();
            }
        }

        if (hasScanned)
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                $"找到 {foundCount} 個包含舊 GUID 的檔案"
            );

            EditorGUILayout.LabelField(
                $"已替換 {replacedCount} 個檔案"
            );

            if (foundCount > 0 && !isReplacing)
            {
                EditorGUILayout.Space(5);

                EditorGUILayout.HelpBox(
                    "執行後將直接修改 Assets 內的序列化檔案。",
                    MessageType.Warning
                );

                if (GUILayout.Button(
                    "2. Replace GUID",
                    GUILayout.Height(35)))
                {
                    ReplaceReferences();
                }
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "掃描結果",
                EditorStyles.boldLabel
            );

            scrollPosition = EditorGUILayout.BeginScrollView(
                scrollPosition,
                GUILayout.Height(200)
            );

            foreach (string file in targetFiles)
            {
                EditorGUILayout.LabelField(
                    file.Replace(
                        Application.dataPath,
                        "Assets"
                    )
                );
            }

            EditorGUILayout.EndScrollView();
        }
    }
    private string GetScriptGuid(MonoScript script)
    {
        if (script == null)
            return "";

        string path = AssetDatabase.GetAssetPath(script);

        if (string.IsNullOrEmpty(path))
            return "";

        return AssetDatabase.AssetPathToGUID(path);
    }

    private bool IsGuidValid()
    {
        return IsValidGuid(oldGuid)
            && IsValidGuid(newGuid)
            && oldGuid != newGuid;
    }

    private static bool IsValidGuid(string guid)
    {
        return !string.IsNullOrWhiteSpace(guid)
            && guid.Length == 32
            && guid.All(Uri.IsHexDigit);
    }

    private void ScanReferences()
    {
        if (EditorApplication.isCompiling)
        {
            EditorUtility.DisplayDialog(
                "警告",
                "Unity 正在編譯，請稍後再執行。",
                "確定"
            );

            return;
        }

        targetFiles = Directory
            .GetFiles(
                Application.dataPath,
                "*.*",
                SearchOption.AllDirectories
            )
            .Where(file =>
                Extensions.Contains(
                    Path.GetExtension(file),
                    StringComparer.OrdinalIgnoreCase
                )
            )
            .Where(file =>
            {
                try
                {
                    return File.ReadAllText(file)
                        .IndexOf(
                            oldGuid,
                            StringComparison.OrdinalIgnoreCase
                        ) >= 0;
                }
                catch
                {
                    return false;
                }
            })
            .ToArray();

        foundCount = targetFiles.Length;
        replacedCount = 0;
        hasScanned = true;
        isReplacing = false;

        Debug.Log(
            $"[ScriptGuidMigrator] 找到 {foundCount} 個檔案"
        );
    }

    private void ReplaceReferences()
    {
        if (!IsGuidValid())
        {
            EditorUtility.DisplayDialog(
                "錯誤",
                "GUID 格式不正確。",
                "確定"
            );

            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "確認替換 GUID",
            $"即將替換 {foundCount} 個檔案。\n\n" +
            $"Old GUID:\n{oldGuid}\n\n" +
            $"UPM GUID:\n{newGuid}\n\n" +
            "請確認已經備份專案。",
            "開始替換",
            "取消"
        );

        if (!confirm)
            return;

        isReplacing = true;
        replacedCount = 0;

        try
        {
            for (int i = 0; i < targetFiles.Length; i++)
            {
                string file = targetFiles[i];

                if (EditorUtility.DisplayCancelableProgressBar(
                    "替換 GUID",
                    file,
                    (float)i / targetFiles.Length))
                {
                    break;
                }

                string content = File.ReadAllText(file);

                if (!content.Contains(oldGuid))
                    continue;

                content = content.Replace(
                    oldGuid,
                    newGuid
                );

                File.WriteAllText(
                    file,
                    content,
                    new UTF8Encoding(false)
                );

                replacedCount++;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[ScriptGuidMigrator] 替換失敗：{e}"
            );
        }
        finally
        {
            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh(
                ImportAssetOptions.ForceUpdate
            );

            isReplacing = false;
        }

        Debug.Log(
            $"[ScriptGuidMigrator] 完成，替換 {replacedCount} 個檔案"
        );

        EditorUtility.DisplayDialog(
            "完成",
            $"已替換 {replacedCount} 個檔案。\n\n" +
            "請檢查場景、Prefab 與 Inspector。",
            "確定"
        );

        ScanReferences();
    }
}