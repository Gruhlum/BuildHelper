using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class StoreSettings
    {
        public bool include = true;
        public string name;
        public bool createZip = true;      

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
    }
}