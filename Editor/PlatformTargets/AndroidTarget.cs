using UnityEditor;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class AndroidTarget : PlatformTarget
    {
        public override BuildTarget BuildTarget
        {
            get
            {
                return BuildTarget.Android;
            }
        }

        public override string Name
        {
            get
            {
                return "Android";
            }
        }
    }
}