using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class WebGLTarget : PlatformTarget
    {
        public string webGLTemplate = "Default";

        public int width = 900;
        public int height = 600;

        public override BuildTarget BuildTarget
        {
            get
            {
                return BuildTarget.WebGL;
            }
        }

        public override string Name
        {
            get
            {
                return "WebGL";
            }
        }

        public override string GetFilePath(string path, string fileName)
        {
            return path;
        }

        public override string GetFileName(Platform platform, Store store, VersionType version)
        {
            return null;
        }
    }
}