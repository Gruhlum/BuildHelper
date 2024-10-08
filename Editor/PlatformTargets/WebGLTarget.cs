using HexTecGames.Basics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class WebGLTarget : PlatformTarget
    {
        public string webGLTemplate = "Default";

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

        private string GetZipFilePath(string outputPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(outputPath);
            string newFolderName = directoryInfo.Name.Replace("WebGL", Application.productName);
            return Path.Combine(directoryInfo.Parent.FullName, newFolderName);
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

            foreach (var result in results)
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

        public override void OnBuildFinished(BuildSummary buildSummary)
        {
            if (buildSummary.result == BuildResult.Succeeded)
            {
                CreateZipFile(buildSummary.outputPath);
            }
        }
        private void CreateZipFile(string path)
        {
            string finalPath = GetZipFilePath(path) + ".zip";
            if (File.Exists(finalPath))
            {
                CreateOldZipPath(finalPath);
            }
            ZipFile.CreateFromDirectory(path, finalPath);
        }
        private void CreateOldZipPath(string finalPath)
        {
            string oldName = Path.GetFileNameWithoutExtension(finalPath);
            string newName = $"{oldName}_old";
            string newPath = FileManager.GenerateUniqueFileName(finalPath.Replace(oldName, newName));
            File.Move(finalPath, newPath);
        }
    }
}