
/*******
 * OutputLogLoaderEditor.cs
 * 
 * Editor拡張。Editorフォルダの下に配置すること
 * 
 * OutputlogLoader.csをアタッチした際のInspectorに表示される項目を拡張している
 * 
 * FBXExporterの有無を確認し、存在するOutputLogをプルダウンに表示する
 * "LoadLogToExportAnim"のボタンを押すとOutputLog内の処理が実行される
 * 
 * *****/
#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.EventSystems;

namespace HUMR
{
    [CustomEditor(typeof(OutputLogLoader))]
    public class OutputLogLoaderEditor : Editor
    {
        string path;
        bool foldout;
        string[] logFilePaths;
        string[] logFileNames;

        static Regex filenameRegex = new Regex(@"^output_log_|\.txt$");

        void CollectLogFilePaths()
        {
            logFilePaths = Directory.GetFiles(path, "*.txt")
                .OrderBy(file => File.GetLastWriteTime(Path.Combine(path, file)))
                .Reverse()
                .ToArray();
            logFileNames = logFilePaths
                .Select(p => filenameRegex.Replace(Path.GetFileName(p), ""))
                .ToArray();
        }

        public override void OnInspectorGUI()
        {

            //元のInspector部分を表示
            base.OnInspectorGUI();

            //targetを変換して対象を取得
            OutputLogLoader targetScript = target as OutputLogLoader;

            EditorGUI.BeginChangeCheck();

            var newFoldout = EditorGUILayout.Foldout(foldout, "Advanced : CustomOutputLogPath");
            if (foldout != newFoldout) logFilePaths = null;
            if (foldout)
            {
                EditorGUI.indentLevel++;
                path = EditorGUILayout.TextField("OutputLogPath", path);
                EditorGUI.indentLevel--;
            }
            else
            {
                path = System.Environment.GetEnvironmentVariable("USERPROFILE");
                path += @"\AppData\LocalLow\VRChat\VRChat";
            }
            if (logFilePaths == null) CollectLogFilePaths();
            foldout = newFoldout;

            // ラベルの作成
            string label = "LoadOutputLog";
            // 初期値として表示する項目のインデックス番号
            int selectedIndex = Array.IndexOf(logFilePaths, targetScript.LogFilePath);
            if (selectedIndex < 0 && 0 < logFilePaths.Length) selectedIndex = 0; 
            // プルダウンメニューの作成
            int index = logFilePaths.Length > 0 ? EditorGUILayout.Popup(label, selectedIndex, logFileNames)
                : -1;

            if (EditorGUI.EndChangeCheck())
            {// 操作を Undo に登録
             // インデックス番号を登録
                targetScript.LogFilePath = logFilePaths[index];
                logFilePaths = null;
            }

            GUILayout.Space(15);

            //PrivateMethodを実行する用のボタン
            if (GUILayout.Button("LoadLogToExportAnim"))
            {
                ExecuteEvents.Execute<OutputLogLoaderinterface>(
                target: targetScript.gameObject,
                eventData: null,
                functor: (recieveTarget, y) => recieveTarget.LoadLogToExportAnim());
            }

        }
    }
}
 #endif
