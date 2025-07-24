using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
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