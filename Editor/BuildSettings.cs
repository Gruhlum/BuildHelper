using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HexTecGames.Basics.Editor.BuildHelper
{

    //[CreateAssetMenu(menuName = "HecTec/BuildSettings")]
    public class BuildSettings : ScriptableObject
    {
        [Tooltip("Scenes to be added to the Build")]
        public List<SceneAsset> scenes;

        public List<PlatformSettings> platformSettings;
        [Tooltip("Can be used to deactive specific gameObjects or to copy the builds into another folder")]
        public List<StoreSettings> storeSettings;

        public BuildOptions options;

        public UpdateType updateType;

        public VersionType version;


        private void OnValidate()
        {
            foreach (var platformSetting in platformSettings)
            {
                platformSetting.OnValidate();
            }
        }

        [ContextMenu("Build All")]
        public void BuildAll()
        {
            VersionData.IncreaseVersion(updateType);
            VersionData.CurrentVersionType = version;


            foreach (var platformSetting in platformSettings)
            {
                if (!platformSetting.include)
                {
                    Debug.Log($"Skipped {platformSetting.buildTarget} since it is not included");
                    continue;
                }
                foreach (var storeSetting in storeSettings)
                {
                    if (!storeSetting.include)
                    {
                        Debug.Log($"Skipped {storeSetting.name} since it is not included");
                        continue;
                    }
                    ApplyStoreSettings(storeSetting);
                    Build(platformSetting, storeSetting);
                }
            }
            CopyFolders(storeSettings);
            RunExternalScript();
        }
        private void Build(PlatformSettings platformSetting, StoreSettings storeSetting)
        {
            BuildReport report = BuildPlatform(platformSetting, storeSetting);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }    
        private void ApplyStoreSettings(StoreSettings targetSetting)
        {
            if (storeSettings == null)
            {
                return;
            }
            foreach (var storeSetting in storeSettings)
            {
                if (storeSetting.exclusiveObjects == null)
                {
                    continue;
                }
                foreach (var go in storeSetting.exclusiveObjects)
                {
                    if (storeSetting == targetSetting)
                    {
                        go.hideFlags = HideFlags.None;
                    }
                    else go.hideFlags = HideFlags.DontSaveInBuild;
                }
            }
        }
        private BuildReport BuildPlatform(PlatformSettings platformSettings, StoreSettings storeSettings)
        {
            if (platformSettings.buildTarget == BuildTarget.NoTarget)
            {
                Debug.LogError("No build target selected");
            }
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetSceneNames(storeSettings).ToArray();
            buildPlayerOptions.locationPathName = GetFullPath(platformSettings);
            buildPlayerOptions.target = platformSettings.buildTarget;
            buildPlayerOptions.options = options;

            return BuildPipeline.BuildPlayer(buildPlayerOptions);

        }

        private void RunExternalScript()
        {
            StoreSettings activeSetting = storeSettings.Find(x => x.include);
            if (activeSetting == null)
            {
                return;
            }
            if (!activeSetting.runExternalScript)
            {
                return;
            }
            if (string.IsNullOrEmpty(activeSetting.externalScript))
            {
                return;
            }
            Process.Start(activeSetting.externalScript);
        }
        private void CopyFolders(List<StoreSettings> storeSettings)
        {
            if (storeSettings == null)
            {
                return;
            }
            foreach (var setting in storeSettings)
            {
                if (setting.copyFolders != null)
                {
                    CopyFolders(setting);
                }
            }
        }
        public void CopyFolders(StoreSettings setting)
        {
            foreach (var copyFolder in setting.copyFolders)
            {
                if (copyFolder.versionType != version)
                {
                    continue;
                }
                PlatformSettings platformSetting = platformSettings.Find(x => x.buildTarget == copyFolder.buildTarget);
                if (platformSetting == null)
                {
                    Debug.Log("no build found: " + platformSetting.buildTarget);
                    continue;
                }
                string sourceLocation = Path.Combine(Directory.GetCurrentDirectory(), GetLocationPath(platformSetting));
                if (!Directory.Exists(sourceLocation))
                {
                    Debug.Log("no files found for " + platformSetting.buildTarget);
                    continue;
                }
                CopyFolders(sourceLocation, copyFolder.targetLocation);
            }
        }
        public void CopyFolders(string source, string target)
        {
            var results = Directory.GetFiles(source);
            foreach (var result in results)
            {
                File.Copy(result, result.Replace(source, target), true);
            }
            var directories = Directory.GetDirectories(source);
            {
                foreach (var directory in directories)
                {
                    if (directory.Contains("DoNotShip"))
                    {
                        continue;
                    }
                    Directory.CreateDirectory(directory.Replace(source, target));
                    CopyFolders(directory, directory.Replace(source, target));
                }
            }
        }
        private string GetFullPath(PlatformSettings setting)
        {
            return Path.Combine(GetLocationPath(setting), GetFileName(setting));
        }
        private string GetFileName(PlatformSettings setting)
        {
            return PlayerSettings.productName + setting.fileEnding;
        }

        private string GetLocationPath(PlatformSettings setting)
        {
            string path;
            path = Path.Combine("Builds", version == VersionType.Demo ? "DEMO" : "", setting.buildTarget.ToString());
            return path;

        }
        private List<string> GetSceneNames(StoreSettings setting)
        {
            List<string> sceneNames = new List<string>();
            sceneNames.AddRange(GetSceneNames(scenes));
            sceneNames.AddRange(GetSceneNames(setting.extraScenes));
            return sceneNames;
        }
        private List<string> GetSceneNames(List<SceneAsset> scenes)
        {
            List<string> sceneNames = new List<string>();
            if (scenes == null)
            {
                return sceneNames;
            }
            foreach (var scene in scenes)
            {
                sceneNames.Add("Assets/Scenes/" + scene.name + ".unity");
            }
            return sceneNames;
        }
    }
}