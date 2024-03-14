using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.Editor.BuildHelper
{
	[System.Serializable]
	public class PlatformSettings
	{
        [Tooltip("Include this the next time we build")]
        public bool include = true;
        [Tooltip("File ending of executable, e.g.: (Windows:.exe, Linux:.x86_64 or empty for OSX")]
        public string fileEnding;
        public BuildTarget buildTarget;
        [Tooltip("Scenes to only be added for this Platform")]
        public List<SceneOrder> extraScenes;
        [Tooltip("Can be used to deactive specific gameObjects or to copy the builds into another folder")]
        public List<StoreSettings> storeSettings;

        private BuildTarget lastBuildTarget;

        public void OnValidate()
        {
            CheckFileEnding();
        }

        private void CheckFileEnding()
        {
            if (lastBuildTarget != buildTarget)
            {
                lastBuildTarget = buildTarget;
                if (buildTarget == BuildTarget.StandaloneWindows64 || buildTarget == BuildTarget.StandaloneWindows)
                {
                    fileEnding = ".exe";
                }
                else if (buildTarget == BuildTarget.StandaloneLinux64)
                {
                    fileEnding = ".x86_64";
                }
                else fileEnding = null;
            }
        }
	}
}