using HexTecGames.Basics;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.Editor.BuildHelper
{
    [System.Serializable]
    public class StoreSettings
    {
        public bool include = true;
        public string name;
        public bool createZip = true;
        [HideInInspector] public bool isWebGL;
        [DrawIf(nameof(isWebGL), true)] public string webGLTemplate = "Default";
        //[DrawIf(nameof(isWebGL), true)] public int width = 900;
        //[DrawIf(nameof(isWebGL), true)] public int height = 600;

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