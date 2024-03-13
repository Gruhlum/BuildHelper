using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.Basics.Editor.BuildHelper
{

    [System.Serializable]
    public class FolderCopyLocation
    {
        [Tooltip("Location to where the build will get copied to")]
        public string targetLocation;
        [Tooltip("Only copy when the BuildTarget matches")]
        public BuildTarget buildTarget;
        [Tooltip("Only copy when the VersionType matches")]
        public VersionType versionType;
    }
}