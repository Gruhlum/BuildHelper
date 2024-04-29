using HexTecGames.Basics;
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
        public BuildTarget buildTarget;       
        [Tooltip("File ending of executable, e.g.: (Windows:.exe, Linux:.x86_64 or empty for OSX")]
        public string fileEnding;
        [Tooltip("Scenes to only be added for this Platform")]
        public List<SceneOrder> extraScenes;
        [Tooltip("Objects that will only be included/exluded for this Platform")]
        public List<ObjectFilter> exclusiveObjects;
        [Tooltip("Can be used to deactive specific gameObjects or to copy the builds into another folder")]
        public List<StoreSettings> storeSettings;

        private BuildTarget lastBuildTarget;

        public void OnValidate()
        {
            CheckFileEnding();
            foreach (var storeSetting in storeSettings)
            {
                if (buildTarget == BuildTarget.WebGL)
                {
                    storeSetting.isWebGL = true;
                }
                else storeSetting.isWebGL = false;
            }
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
                else if (buildTarget == BuildTarget.StandaloneOSX)
                {
                    fileEnding = ".app";
                }
                else fileEnding = null;
            }
        }
	}
}