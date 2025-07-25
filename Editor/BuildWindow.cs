using UnityEditor;

namespace HexTecGames.BuildHelper.Editor
{
    public class BuildWindow : EditorWindow
    {
        [MenuItem("Tools/Build")]
        public static void ShowWindow()
        {
            GetWindow(typeof(BuildWindow));
        }
        private void OnGUI()
        {
            if (BuildSettings.instance == null)
            {
                return;
            }
            UnityEditor.Editor m_MyScriptableObjectEditor = UnityEditor.Editor.CreateEditor(BuildSettings.instance);
            m_MyScriptableObjectEditor.OnInspectorGUI();
        }
    }
}