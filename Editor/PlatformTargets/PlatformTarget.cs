using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
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

        public abstract string Name
        {
            get;
        }
        public abstract BuildTarget BuildTarget
        {
            get;
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

        public virtual void OnBuildFinished(BuildSummary buildSummary)
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}