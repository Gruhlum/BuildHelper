using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class WebGLTemplateOverride : WebGLOverride
    {
        public string templateOverride = "Default";

        public override void ApplyBeforeBuild(BuildData buildData, bool isActive)
        {
            if (!isActive)
            {
                return;
            }
            // Search for a folder matching the template name
            bool foundProjectTemplate = FolderExistsInProject(templateOverride);

            if (foundProjectTemplate)
            {
                PlayerSettings.WebGL.template = "PROJECT:" + templateOverride;
                return;
            }

            // If not found in project, fall back to application template
            PlayerSettings.WebGL.template = "APPLICATION:" + templateOverride;
        }

        private bool FolderExistsInProject(string folderName)
        {
            return FolderExistsRecursive("Assets", folderName);
        }

        private bool FolderExistsRecursive(string root, string targetName)
        {
            string[] subFolders = AssetDatabase.GetSubFolders(root);

            foreach (string folder in subFolders)
            {
                // Extract the last folder name
                string last = System.IO.Path.GetFileName(folder);

                if (last == targetName)
                {
                    return true;
                }

                // Recurse deeper
                if (FolderExistsRecursive(folder, targetName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}