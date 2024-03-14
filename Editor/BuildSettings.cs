using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HexTecGames.Editor.BuildHelper
{

    [CreateAssetMenu(fileName = "BuildSettings", menuName = "HexTecGames/Editor/BuildSettings")]
    public class BuildSettings : ScriptableObject
    {
        [Tooltip("Scenes to be added to the Build")]
        public List<SceneOrder> scenes;

        public List<PlatformSettings> platformSettings;      

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
            VersionNumber.IncreaseVersion(updateType);

            foreach (var platformSetting in platformSettings)
            {
                if (!platformSetting.include)
                {
                    Debug.Log($"Skipped {platformSetting.buildTarget} since it is not included");
                    continue;
                }
                foreach (var storeSetting in platformSetting.storeSettings)
                {
                    if (!storeSetting.include)
                    {
                        Debug.Log($"Skipped {storeSetting.name} since it is not included");
                        continue;
                    }
                    ApplyStoreSettings(storeSetting, platformSetting.storeSettings);
                    Build(platformSetting, storeSetting);
                }
                CopyFolders(platformSetting.storeSettings);
                RunExternalScript(platformSetting.storeSettings);
            }           
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
        private void ApplyStoreSettings(StoreSettings targetSetting, List<StoreSettings> storeSettings)
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
        private BuildReport BuildPlatform(PlatformSettings platformSetting, StoreSettings storeSetting)
        {
            if (platformSetting.buildTarget == BuildTarget.NoTarget)
            {
                Debug.LogError("No build target selected");
            }
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetSceneNames(platformSetting, storeSetting).ToArray();
            buildPlayerOptions.locationPathName = GetFullPath(platformSetting);
            buildPlayerOptions.target = platformSetting.buildTarget;
            buildPlayerOptions.options = options;

            return BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        private void RunExternalScript(List<StoreSettings> storeSettings)
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
        private List<string> GetSceneNames(PlatformSettings platformSetting, StoreSettings storeSetting)
        {
            List<string> sceneNames = new List<string>();
            sceneNames.AddRange(GetSceneNames(scenes));
            sceneNames.AddRange(GetSceneNames(storeSetting.extraScenes));
            sceneNames.AddRange(GetSceneNames(platformSetting.extraScenes));
            return sceneNames;
        }
        private List<string> GetSceneNames(List<SceneOrder> sceneOrders)
        {
            List<string> sceneNames = new List<string>();
            if (sceneOrders == null)
            {
                return sceneNames;
            }
            sceneOrders = sceneOrders.OrderBy(x => x.order).ToList();
            foreach (var sceneOrder in sceneOrders)
            {
                sceneNames.Add("Assets/Scenes/" + sceneOrder.scene.name + ".unity");
            }
            return sceneNames;
        }
    }
}