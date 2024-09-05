using System.Collections;
using System.Collections.Generic;
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

            GUILayout.Space(10);
            if (GUILayout.Button("Open Build Folder", GUILayout.ExpandWidth(true), GUILayout.Height(24)))
            {
                buildSettings.OpenBuildFolder();
            }
            if (GUILayout.Button("Start Build", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                buildSettings.BuildAll();
            }

            GUILayout.Space(10);

            base.OnInspectorGUI();
        }
    }
}