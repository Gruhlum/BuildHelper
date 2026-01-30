using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class BuildData
    {
        public Platform platform;
        public Store store;
        public VersionType versionType;
        public List<SceneOrder> scenes = new List<SceneOrder>();

        public PlatformTarget platformTarget;

        /// <summary>
        /// //ProjectName/Builds/Platform/Platform_Store_0.0.0
        /// </summary>
        public string storePath;
        public string fileName;
        /// <summary>
        /// //ProjectName/Builds/Platform/Platform_Store_0.0.0/
        /// </summary>
        public string filePath;
        public BuildOptions buildOptions;

        public BuildData(Platform platform, Store store, VersionType versionType, BuildOptions buildOptions, string storePath, List<SceneOrder> scenes)
        {
            this.platform = platform;
            this.store = store;
            this.versionType = versionType;
            this.scenes = scenes;
            this.storePath = storePath;
            this.buildOptions = buildOptions;
            platformTarget = platform.buildTarget;
            fileName = platform.buildTarget.GetFileName(platform, store, versionType);
            filePath = platformTarget.GetFilePath(storePath, fileName);
        }

        public BuildPlayerOptions GenerateBuildPlayerOptions()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetSceneNames(scenes).ToArray()
            };
            buildPlayerOptions.locationPathName = filePath;
            buildPlayerOptions.target = platformTarget.BuildTarget;
            buildPlayerOptions.options = buildOptions;
            if (versionType == VersionType.Demo)
            {
                buildPlayerOptions.extraScriptingDefines = new string[] { "DEMO" };
            }
            return buildPlayerOptions;
        }

        public string GetPlatformPath()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(storePath);
            var result = directoryInfo.Parent.FullName;
            Debug.Log(storePath + " - " + result);
            return result;
        }
        private List<string> GetSceneNames(List<SceneOrder> sceneOrders)
        {
            List<string> sceneNames = new List<string>();
            if (sceneOrders == null)
            {
                return sceneNames;
            }
            sceneOrders = sceneOrders.OrderBy(x => x.order).ToList();
            foreach (SceneOrder sceneOrder in sceneOrders)
            {
                string path = AssetDatabase.GetAssetPath(sceneOrder.scene);
                sceneNames.Add(path);
            }
            return sceneNames;
        }

        public override string ToString()
        {
            return $"{platform.name} {store.name}, Scenes: {string.Join(", ", scenes.Select(x => x.scene.name).ToList())}";
        }
    }
}