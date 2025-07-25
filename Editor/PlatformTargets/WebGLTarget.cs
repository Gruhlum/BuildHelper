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

        public override string GetLocationPath(string path, string fileName)
        {
            return path;
        }
        public override void ApplySettings(StoreSettings store)
        {
            List<string> results = GetAllFolderPaths("Assets");
            string[] folderNames;
            string targetTemplate;

            if (store.HasSettingsOverride(out TemplateOverride templateOverride))
            {
                targetTemplate = templateOverride.templateOverride;
            }
            else targetTemplate = webGLTemplate;

            foreach (string result in results)
            {
                folderNames = result.Split(new char[] { '/', '\\' });
                if (folderNames.Contains(targetTemplate))
                {
                    PlayerSettings.WebGL.template = "PROJECT:" + targetTemplate;
                    return;
                }
            }
            PlayerSettings.WebGL.template = "APPLICATION:" + targetTemplate;
        }

        private List<string> GetAllFolderPaths(string startFolder)
        {
            List<string> folderNames = new List<string>();
            string[] results = AssetDatabase.GetSubFolders(startFolder);
            folderNames.AddRange(results);
            foreach (string result in results)
            {
                folderNames.AddRange(GetAllFolderPaths(result));
            }
            return folderNames;
        }

        public override string GetFileName(PlatformSettings platformSetting, StoreSettings storeSetting, VersionType version)
        {
            string fileName;

            return null;

            //if (version == VersionType.Demo)
            //{
            //    fileName = $"{PlayerSettings.productName}_{storeSetting.name}_{VersionNumber.GetCurrentVersion()}_demo{platformSetting.fileEnding}";
            //}
            //else fileName = $"{PlayerSettings.productName}_{storeSetting.name}_{VersionNumber.GetCurrentVersion()}{platformSetting.fileEnding}";

            return fileName;
        }


    }
}