using System.IO;
using System.IO.Compression;
using HexTecGames.Basics;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public abstract class PlatformTarget
    {
        public virtual string FileEnding
        {
            get
            {
                return string.Empty;
            }
        }

        public abstract string Name
        {
            get;
        }
        public abstract BuildTarget BuildTarget
        {
            get;
        }

        public virtual string GetFilePath(string path, string fileName)
        {
            return Path.Combine(path, fileName);
        }

        public virtual string GetFileName(Platform platform, Store store, VersionType version)
        {
            string fileName;

            if (version == VersionType.Demo)
            {
                fileName = $"{PlayerSettings.productName}_demo{platform.fileEnding}";
            }
            else fileName = $"{PlayerSettings.productName}{platform.fileEnding}";

            return fileName;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}