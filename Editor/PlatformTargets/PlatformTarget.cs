using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

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

        public virtual bool CreateZip
        {
            get
            {
                return false;
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

        public virtual string GetZipFilePath(string outputPath)
        {
            return Directory.GetParent(outputPath).FullName;
        }

        public virtual string GetLocationPath(string path, string fileName)
        {
            return Path.Combine(path, fileName);
        }
        public virtual void ApplySettings()
        {

        }
        public virtual string GetFileName(PlatformSettings platformSetting, StoreSettings storeSetting, VersionType version)
        {
            string fileName;

            if (version == VersionType.Demo)
            {
                fileName = $"{PlayerSettings.productName}_demo{platformSetting.fileEnding}";
            }
            else fileName = $"{PlayerSettings.productName}{platformSetting.fileEnding}";

            return fileName;
        }
    }
}