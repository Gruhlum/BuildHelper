using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.Basics.Editor.BuildHelper
{
    [CustomEditor(typeof(BuildSettings))]
    public class BuildSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BuildSettings myTarget = (BuildSettings)target;

            GUILayout.Space(10);

            if (GUILayout.Button("Start Build", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                myTarget.BuildAll();
            }

            GUILayout.Space(10);

            base.OnInspectorGUI();
        }
    }
}