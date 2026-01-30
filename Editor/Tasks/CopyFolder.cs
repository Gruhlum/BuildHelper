using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class CopyFolder : Task
    {
        [Tooltip("Used to copy the build folders to another location")]
        public List<FolderCopyLocation> folderLocations;

        private void CopyFolders(List<Store> stores, BuildSettings buildSettings)
        {
            if (stores == null)
            {
                return;
            }

            if (folderLocations == null || folderLocations.Count <= 0)
            {
                return;
            }

            foreach (Store store in stores)
            {
                CopyFolders(store, buildSettings);
            }
        }
        private void CopyFolders(Store store, BuildSettings buildSettings)
        {
            //foreach (FolderCopyLocation folderLocation in folderLocations)
            //{
            //    if (folderLocation.versionType != buildSettings.versionType)
            //    {
            //        continue;
            //    }
            //    //Platform platform = buildSettings.platforms.Find(x => x.platform.b == folderLocation.buildTarget);
            //    if (platform == null)
            //    {
            //        Debug.Log("no build found: " + platform.buildTarget.Name);
            //        continue;
            //    }
            //    string sourceLocation = Path.Combine(Application.dataPath, buildSettings.GetBuildFolderPath(platform, store));
            //    if (!Directory.Exists(sourceLocation))
            //    {
            //        Debug.Log("no files found for " + platform.buildTarget.Name);
            //        continue;
            //    }
            //    CopyFolders(sourceLocation, folderLocation.targetLocation);
            //}
        }
        private void CopyFolders(string source, string target)
        {
            string[] results = Directory.GetFiles(source);
            foreach (string result in results)
            {
                File.Copy(result, result.Replace(source, target), true);
            }
            string[] directories = Directory.GetDirectories(source);
            {
                foreach (string directory in directories)
                {
                    if (directory.Contains("DoNotShip"))
                    {
                        continue;
                    }
                    Directory.CreateDirectory(directory.Replace(source, target));
                    CopyFolders(directory, directory.Replace(source, target));
                }
            }
        }

        public override void Run(Platform platform, BuildSettings buildSettings)
        {
            CopyFolders(platform.Stores, buildSettings);
        }
    }
}