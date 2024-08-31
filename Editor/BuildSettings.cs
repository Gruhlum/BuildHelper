using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HexTecGames.BuildHelper.Editor
{

    [CreateAssetMenu(fileName = "BuildSettings", menuName = "HexTecGames/Editor/BuildSettings")]
    public class BuildSettings : ScriptableObject
    {
        public VersionType version;
        public UpdateType updateType;
        public BuildOptions options;

        [Tooltip("Scenes to be added to the Build")]
        public List<SceneOrder> scenes;

        public List<PlatformSettings> platformSettings;

        private List<string> fullBuildPaths = new List<string>();

        public static BuildSettings instance;

        private void OnValidate()
        {
            foreach (var platformSetting in platformSettings)
            {
                platformSetting.OnValidate();
            }
        }
        private void Awake()
        {
            instance = this;
        }

        [ContextMenu("Build All")]
        public void BuildAll()
        {
            string oldVersionNumber = VersionNumber.GetCurrentVersion();
            VersionNumber.IncreaseVersion(updateType);
            updateType = UpdateType.None;
            fullBuildPaths.Clear();

            bool success = false;

            foreach (var platformSetting in platformSettings)
            {
                if (!platformSetting.include)
                {
                    Debug.Log($"Skipped {platformSetting.buildTarget.Name} since it is not included");
                    continue;
                }

                platformSetting.ApplySettings();

                foreach (var storeSetting in platformSetting.storeSettings)
                {
                    if (!storeSetting.include)
                    {
                        Debug.Log($"Skipped {storeSetting.name} since it is not included");
                        continue;
                    }

                    ApplyObjectFilters(platformSetting, storeSetting);
                    bool result = Build(platformSetting, storeSetting);
                    if (result)
                    {
                        success = true;
                    }
                }
                if (success)
                {
                    CopyFolders(platformSetting.storeSettings);
                    RunExternalScript(platformSetting.storeSettings);

                    if (fullBuildPaths != null && fullBuildPaths.Count > 0)
                    {
                        Process.Start(fullBuildPaths[0]);
                    }
                }
                else VersionNumber.SetVersionNumber(oldVersionNumber);

                ClearObjectFilters();
            }            
        }

        private bool Build(PlatformSettings platformSetting, StoreSettings storeSetting)
        {
            BuildReport report = BuildPlatform(platformSetting, storeSetting);
            BuildSummary summary = report.summary;
            platformSetting.OnBuildFinished(summary);

            if (summary.result == BuildResult.Succeeded)
            {
                return true;
            }
            else return false;
        }
        private BuildReport BuildPlatform(PlatformSettings platformSetting, StoreSettings storeSetting)
        {
            if (platformSetting.buildTarget == null)
            {
                Debug.LogError("No build target selected");
            }

            string path = GenerateFolders(platformSetting, storeSetting);
            fullBuildPaths.Add(path);
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetSceneNames(platformSetting, storeSetting).ToArray();
            string fileName = platformSetting.buildTarget.GetFileName(platformSetting, storeSetting, version);
            buildPlayerOptions.locationPathName = platformSetting.buildTarget.GetLocationPath(path, fileName);
            buildPlayerOptions.target = platformSetting.buildTarget.BuildTarget;
            buildPlayerOptions.options = options;

            Thread.Sleep(100);
            return BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        private void ClearObjectFilters()
        {
            foreach (var platform in platformSettings)
            {
                foreach (var obj in platform.exclusiveObjects)
                {
                    obj.item.hideFlags = HideFlags.None;
                }
                foreach (var store in platform.storeSettings)
                {
                    foreach (var obj in store.exclusiveObjects)
                    {
                        obj.item.hideFlags = HideFlags.None;
                    }
                }
            }
        }
        private void ApplyObjectFilters(PlatformSettings activePlatform, StoreSettings activeStore)
        {
            foreach (var platform in platformSettings)
            {
                foreach (var obj in platform.exclusiveObjects)
                {
                    if (obj.mode == ObjectFilter.Mode.Include && platform == activePlatform)
                    {
                        obj.item.hideFlags = HideFlags.None;
                    }
                    else if (obj.mode == ObjectFilter.Mode.Exclude && platform != activePlatform)
                    {
                        obj.item.hideFlags = HideFlags.None;
                    }
                    else
                    {
                        obj.item.hideFlags = HideFlags.DontSaveInBuild;
                        Debug.Log($"Exluded object: {obj.item.name} from: {activePlatform}, {activeStore}");
                    }
                }
                foreach (var store in platform.storeSettings)
                {
                    foreach (var obj in store.exclusiveObjects)
                    {
                        if (obj.mode == ObjectFilter.Mode.Include && store == activeStore)
                        {
                            obj.item.hideFlags = HideFlags.None;
                        }
                        else if (obj.mode == ObjectFilter.Mode.Exclude && store != activeStore)
                        {
                            obj.item.hideFlags = HideFlags.None;
                        }
                        else
                        {
                            obj.item.hideFlags = HideFlags.DontSaveInBuild;
                            Debug.Log($"Exluded object: {obj.item.name} from: {activePlatform}, {activeStore}");
                        }
                    }
                }
            }
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
        public void CopyFolders(StoreSettings storeSetting)
        {
            foreach (var copyFolder in storeSetting.copyFolders)
            {
                if (copyFolder.versionType != version)
                {
                    continue;
                }
                PlatformSettings platformSetting = platformSettings.Find(x => x.buildTarget.BuildTarget == copyFolder.buildTarget);
                if (platformSetting == null)
                {
                    Debug.Log("no build found: " + platformSetting.buildTarget.Name);
                    continue;
                }
                string sourceLocation = Path.Combine(Application.dataPath, GetBuildFolderPath(platformSetting, storeSetting));
                if (!Directory.Exists(sourceLocation))
                {
                    Debug.Log("no files found for " + platformSetting.buildTarget.Name);
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
        private string GenerateFolders(PlatformSettings platformSetting, StoreSettings storeSetting)
        {
            string lastPath = Directory.GetCurrentDirectory(); //..ProjectName
            lastPath = Path.Combine(lastPath, "Builds"); //ProjectName/Builds
            CreateDirectory(lastPath);
            lastPath = Path.Combine(lastPath, platformSetting.buildTarget.Name); //ProjectName/Builds/Platform
            CreateDirectory(lastPath);

            lastPath = GetBuildFolderPath(platformSetting, storeSetting); //ProjectName/Builds/Platform/Platform_Store_0.0.0
            CreateDirectory(lastPath);

            return lastPath;
        }
        private string GetBuildFolderPath(PlatformSettings platformSetting, StoreSettings storeSetting)
        {
            //../Assets/Builds/WindowsStandalone64/Windows_Steam_1.0.0/
            return Path.Combine(Directory.GetCurrentDirectory(), "Builds", platformSetting.buildTarget.Name,
                $"{platformSetting.buildTarget.Name}_{storeSetting.name}_{VersionNumber.GetCurrentVersion()}");
        }

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private List<string> GetSceneNames(PlatformSettings platformSetting, StoreSettings storeSetting)
        {
            List<SceneOrder> sceneOrders = new List<SceneOrder>();
            sceneOrders.AddRange(scenes);
            sceneOrders.AddRange(storeSetting.extraScenes);
            sceneOrders.AddRange(platformSetting.extraScenes);

            return GetSceneNames(sceneOrders);
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
                string path = AssetDatabase.GetAssetPath(sceneOrder.scene);
                sceneNames.Add(path);
            }
            return sceneNames;
        }
    }
}