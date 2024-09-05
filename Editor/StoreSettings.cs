using HexTecGames.Basics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class StoreSettings
    {
        public string name;
        public bool include = true;

        public bool IsWebGLStore
        {
            get
            {
                return isWebGLStore;
            }
            set
            {
                isWebGLStore = value;
                if (isWebGLStore == false)
                {
                    templateOverride = string.Empty;
                }
            }
        }
        private bool isWebGLStore;

        [DrawIf(nameof(isWebGLStore), true)] public string templateOverride;

        [Tooltip("Scenes that will only be added to this specific Build")]
        public List<SceneOrder> extraScenes;

        [Tooltip("Objects that will only be included/exluded for this Store")]
        public List<ObjectFilter> exclusiveObjects;

        [Tooltip("Used to copy the build folders to another location")]
        public List<FolderCopyLocation> copyFolders;

        [Tooltip("Location of the external script")]
        public string externalScript;
        [Tooltip("Should this script be run after Build is complete")]
        public bool runExternalScript;

       



        public override string ToString()
        {
            return name;
        }
    }
}