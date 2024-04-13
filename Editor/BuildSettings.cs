using HexTecGames.Basics;
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

namespace HexTecGames.Editor.BuildHelper
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

        private string lastPath;

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
            updateType = UpdateType.None;

            foreach (var platformSetting in platformSettings)
            {
                lastPath = GetLocationPath(platformSetting);
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
                    ApplyObjectFilters(platformSetting, storeSetting);
                    Build(platformSetting, storeSetting);
                }
                CopyFolders(platformSetting.storeSettings);
                RunExternalScript(platformSetting.storeSettings);
            }
            if (!string.IsNullOrEmpty(lastPath))
            {
                Process.Start(lastPath);
            }
        }

        private void CreateZipFile(string path)
        {
            string fileName = Path.GetFileName(path);
            string pathWithoutFileName = Path.GetDirectoryName(path);
            Debug.Log(fileName + " - " + pathWithoutFileName + " - " + path);
            ZipFile.CreateFromDirectory(path, Path.Combine(pathWithoutFileName, fileName + ".zip"));
        }
        private void Build(PlatformSettings platformSetting, StoreSettings storeSetting)
        {
            BuildReport report = BuildPlatform(platformSetting, storeSetting);
            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                if (storeSetting.isWebGL && storeSetting.createZip)
                {
                    CreateZipFile(summary.outputPath);
                }
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
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
            buildPlayerOptions.locationPathName = GetFullPath(platformSetting, storeSetting);
            buildPlayerOptions.target = platformSetting.buildTarget;
            buildPlayerOptions.options = options;
            Thread.Sleep(100);
            return BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        private void ApplyStoreSettings(StoreSettings targetSetting, List<StoreSettings> storeSettings)
        {
            if (storeSettings == null)
            {
                return;
            }
            if (targetSetting.isWebGL)
            {
                PlayerSettings.defaultWebScreenWidth = targetSetting.width;
                PlayerSettings.defaultScreenHeight = targetSetting.height;

                List<string> results = GetAllFolderPaths("Assets");
                string[] folderNames;
                foreach (var result in results)
                {
                    folderNames = result.Split(new char[] { '/', '\\' });
                    if (folderNames.Contains(targetSetting.webGLTemplate))
                    {
                        PlayerSettings.WebGL.template = "PROJECT:" + targetSetting.webGLTemplate;
                        return;
                    }
                }             
                PlayerSettings.WebGL.template = "APPLICATION:" + targetSetting.webGLTemplate;
            }
        }

        private List<string> GetAllFolderPaths(string startFolder)
        {
            List<string> folderNames = new List<string>();
            //Debug.Log(startFolder);
            var results = AssetDatabase.GetSubFolders(startFolder);
            folderNames.AddRange(results);
            foreach (var result in results)
            {
                folderNames.AddRange(GetAllFolderPaths(result));
            }
            return folderNames;
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
        private string GetFullPath(PlatformSettings platformSetting, StoreSettings storeSetting)
        {
            string fileName;
            if (version == VersionType.Demo)
            {
                fileName = $"{GetFileName(platformSetting)}_{storeSetting.name}_{VersionNumber.GetCurrentVersion()}_demo";
            }
            else fileName = $"{GetFileName(platformSetting)}_{storeSetting.name}_{VersionNumber.GetCurrentVersion()}";

            return Path.Combine(GetLocationPath(platformSetting), fileName);
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
                sceneNames.Add("Assets/Scenes/" + sceneOrder.scene.name + ".unity");
            }
            return sceneNames;
        }
    }
}