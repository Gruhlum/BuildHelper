using UnityEditor;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class WindowsTarget : PlatformTarget
    {
        public override BuildTarget BuildTarget
        {
            get
            {
                return BuildTarget.StandaloneWindows64;
            }
        }

        public override string FileEnding
        {
            get
            {
                return ".exe";
            }
        }

        public override string Name
        {
            get
            {
                return "Windows";
            }
        }
    }
}