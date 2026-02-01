using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [CustomEditor(typeof(BuildSettings))]
    public class BuildSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BuildSettings buildSettings = (BuildSettings)target;
            int totalActiveBuilds = buildSettings.GetTotalActiveBuilds();
            GUILayout.Space(10);
            if (GUILayout.Button("Open Build Folder", GUILayout.ExpandWidth(true), GUILayout.Height(24)))
            {
                buildSettings.OpenBuildFolder();
            }

            if (totalActiveBuilds <= 0)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.Button("No Active Builds", GUILayout.ExpandWidth(true), GUILayout.Height(32));
                }
            }
            else
            {
                string buttonText = buildSettings.versionType == VersionType.Demo ? "Build Demo" : "Build";
                if (GUILayout.Button(buttonText, GUILayout.ExpandWidth(true), GUILayout.Height(32)))
                {
                    buildSettings.BuildAll();
                }
            }
                

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Version: {buildSettings.LastBuildVersion} -> {buildSettings.GetVersionString()}");
            GUILayout.Label($"Builds Active: {totalActiveBuilds}/{buildSettings.GetTotalBuilds()}");
            GUILayout.EndHorizontal();

            base.OnInspectorGUI();
        }
    }
}