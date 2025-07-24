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
        public bool createZip;

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
        public virtual void ApplySettings(StoreSettings store)
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
            if (buildSummary.result == BuildResult.Succeeded)
            {
                if (createZip)
                {
                    CreateZipFile(buildSummary.outputPath);
                }
            }
        }
        private string GetZipFilePath(string outputPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(outputPath);
            //string newFolderName = directoryInfo.Name.Replace("WebGL", Application.productName);
            return Path.Combine(directoryInfo.Parent.FullName, directoryInfo.Name);
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
        public override string ToString()
        {
            return Name;
        }
    }
}