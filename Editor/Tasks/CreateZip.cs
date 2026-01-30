using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using HexTecGames.Basics;
using UnityEngine;
using UnityEngine.UI;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class ZipHelper
    {
        private static string GetZipFilePath(string outputPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(outputPath);
            //string newFolderName = directoryInfo.Name.Replace("WebGL", Application.productName);
            return Path.Combine(directoryInfo.Parent.FullName, directoryInfo.Name);
        }
        private static void CreateFilteredZip(string sourceDir, string destinationZip)
        {
            if (File.Exists(destinationZip))
            {
                File.Delete(destinationZip);
            }

            using (var archive = ZipFile.Open(destinationZip, ZipArchiveMode.Create))
            {
                foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
                {
                    // Skip any file whose path contains "DoNotShip"
                    if (file.Contains("DoNotShip"))
                    {
                        continue;
                    }

                    string relativePath = Path.GetRelativePath(sourceDir, file);
                    archive.CreateEntryFromFile(file, relativePath);
                }
            }
        }
        public static void CreateZipFile(string path)
        {
            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                path = fileInfo.DirectoryName;
            }

            string finalPath = GetZipFilePath(path) + ".zip";
            if (File.Exists(finalPath))
            {
                CreateOldZipPath(finalPath);
            }
            CreateFilteredZip(path, finalPath);
        }
        private static void CreateOldZipPath(string finalPath)
        {
            string oldName = Path.GetFileNameWithoutExtension(finalPath);
            string newName = $"{oldName}_old";
            string newPath = FileManager.GenerateUniqueFileName(finalPath.Replace(oldName, newName));
            File.Move(finalPath, newPath);
        }
    }
}