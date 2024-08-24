using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class WebGLTarget : PlatformTarget
    {
        public string webGLTemplate = "Default";

        public override bool CreateZip
        {
            get
            {
                return createZip;
            }
        }
        public bool createZip;
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

        public override string GetZipFilePath(string outputPath)
        {
            return outputPath;
        }

        public override string GetLocationPath(string path, string fileName)
        {
            return path;
        }
        public override void ApplySettings()
        {
            List<string> results = GetAllFolderPaths("Assets");
            string[] folderNames;
            foreach (var result in results)
            {
                folderNames = result.Split(new char[] { '/', '\\' });
                if (folderNames.Contains(webGLTemplate))
                {
                    PlayerSettings.WebGL.template = "PROJECT:" + webGLTemplate;
                    return;
                }
            }
            PlayerSettings.WebGL.template = "APPLICATION:" + webGLTemplate;
        }

        private List<string> GetAllFolderPaths(string startFolder)
        {
            List<string> folderNames = new List<string>();
            var results = AssetDatabase.GetSubFolders(startFolder);
            folderNames.AddRange(results);
            foreach (var result in results)
            {
                folderNames.AddRange(GetAllFolderPaths(result));
            }
            return folderNames;
        }

        public override string GetFileName(PlatformSettings platformSetting, StoreSettings storeSetting, VersionType version)
        {
            string fileName;

            if (version == VersionType.Demo)
            {
                fileName = $"{PlayerSettings.productName}_{storeSetting.name}_{VersionNumber.GetCurrentVersion()}_demo{platformSetting.fileEnding}";
            }
            else fileName = $"{PlayerSettings.productName}_{storeSetting.name}_{VersionNumber.GetCurrentVersion()}{platformSetting.fileEnding}";

            return fileName;
        }
    }
}