using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    public class BuildWindow : EditorWindow
    {
        [SerializeField] private BuildSettings settings = default;

        [MenuItem("Tools/Build")]
        public static void ShowWindow()
        {
            GetWindow(typeof(BuildWindow));
        }
        private void OnGUI()
        {
            UnityEditor.Editor m_MyScriptableObjectEditor = UnityEditor.Editor.CreateEditor(settings);
            m_MyScriptableObjectEditor.OnInspectorGUI();
        }
    }
}