using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using HexTecGames.Basics;
using HexTecGames.Basics.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HexTecGames.BuildHelper.Editor
{

    [CreateAssetMenu(fileName = "BuildSettings", menuName = "HexTecGames/Editor/BuildSettings")]
    public class BuildSettings : ScriptableObject
    {
        public string gameName;
        public VersionType version;
        public UpdateType updateType;
        public BuildOptions options;

        [Tooltip("Scenes to be added to the Build")]
        public List<SceneOrder> scenes;

        public List<PlatformSettings> platformSettings;

        private List<string> fullBuildPaths = new List<string>();

        public static BuildSettings instance;
        private const string BUILD_FOLDER_NAME = "Builds";

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(gameName))
            {
                gameName = Application.productName;
            }
            foreach (PlatformSettings platformSetting in platformSettings)
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
            var oldVersionNumber = VersionNumber.GetVersionNumber();
            string oldProductName = PlayerSettings.productName;
            PlayerSettings.productName = gameName;
            //VersionNumber.IncreaseVersion(updateType);
            updateType = UpdateType.None;
            fullBuildPaths.Clear();

            bool success = false;

            foreach (PlatformSettings platformSetting in platformSettings)
            {
                if (!platformSetting.include)
                {
                    Debug.Log($"Skipped {platformSetting.buildTarget.Name} since it is not included");
                    continue;
                }
                foreach (StoreSettings storeSetting in platformSetting.storeSettings)
                {
                    if (!storeSetting.include)
                    {
                        Debug.Log($"Skipped {storeSetting.name} since it is not included");
                        continue;
                    }

                    platformSetting.ApplySettings(storeSetting);

                    ApplyObjectFilters(platformSetting, storeSetting);

                    bool result = Build(platformSetting, storeSetting);
                    if (result)
                    {
                        success = true;
                    }
                    ClearObjectFilters();
                }
                if (success)
                {
                    CopyFolders(platformSetting.storeSettings);
                    RunExternalScript(platformSetting.storeSettings);
                }
                else Debug.Log("Failed Build: " + platformSetting.ToString());
            }
            if (success)
            {
                if (fullBuildPaths != null && fullBuildPaths.Count > 0)
                {
                    Process.Start(fullBuildPaths[0]);
                }
            }
            else VersionNumber.SetBuildVersionNumber(oldVersionNumber);

            PlayerSettings.productName = oldProductName;

            Debug.Log("Build Success: " + success);
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
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetSceneNames(platformSetting, storeSetting).ToArray()
            };
            string fileName = platformSetting.buildTarget.GetFileName(platformSetting, storeSetting, version);
            buildPlayerOptions.locationPathName = platformSetting.buildTarget.GetLocationPath(path, fileName);
            buildPlayerOptions.target = platformSetting.buildTarget.BuildTarget;
            buildPlayerOptions.options = options;

            Thread.Sleep(100);
            return BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
        public void OpenBuildFolder()
        {
            TryCreateDirectory(GetBuildFolderPath());
            Process.Start(GetBuildFolderPath());
        }
        private void ClearObjectFilters()
        {
            foreach (PlatformSettings platform in platformSettings)
            {
                foreach (ObjectFilter obj in platform.exclusiveObjects)
                {
                    obj.item.hideFlags = HideFlags.None;
                }
                foreach (StoreSettings store in platform.storeSettings)
                {
                    foreach (ObjectFilter obj in store.exclusiveObjects)
                    {
                        obj.item.hideFlags = HideFlags.None;
                    }
                }
            }
        }
        private void ApplyObjectFilters(PlatformSettings activePlatform, StoreSettings activeStore)
        {
            ApplyOtherFilterSettings(activePlatform, activeStore);
            ApplyCurrentObjectFilters(activePlatform, activeStore);
        }
        private void ApplyCurrentObjectFilters(PlatformSettings platform, StoreSettings store)
        {
            foreach (ObjectFilter obj in platform.exclusiveObjects)
            {
                if (obj.mode == ObjectFilter.Mode.Include)
                {
                    obj.item.hideFlags = HideFlags.None;
                }
                else if (obj.mode == ObjectFilter.Mode.Exclude)
                {
                    obj.item.hideFlags = HideFlags.DontSaveInBuild;
                }
            }
            foreach (ObjectFilter obj in store.exclusiveObjects)
            {
                if (obj.mode == ObjectFilter.Mode.Include)
                {
                    obj.item.hideFlags = HideFlags.None;
                }
                else if (obj.mode == ObjectFilter.Mode.Exclude)
                {
                    obj.item.hideFlags = HideFlags.DontSaveInBuild;
                }
            }
        }
        private void ApplyOtherFilterSettings(PlatformSettings activePlatform, StoreSettings activeStore)
        {
            foreach (PlatformSettings platform in platformSettings)
            {
                if (platform == activePlatform)
                {
                    continue;
                }
                foreach (ObjectFilter obj in platform.exclusiveObjects)
                {
                    if (obj.mode == ObjectFilter.Mode.Include)
                    {
                        obj.item.hideFlags = HideFlags.DontSaveInBuild;
                    }
                    else if (obj.mode == ObjectFilter.Mode.Exclude)
                    {
                        obj.item.hideFlags = HideFlags.DontSaveInBuild;
                    }
                }
                foreach (StoreSettings store in platform.storeSettings)
                {
                    if (store == activeStore)
                    {
                        return;
                    }
                    foreach (ObjectFilter obj in store.exclusiveObjects)
                    {
                        if (obj.mode == ObjectFilter.Mode.Include)
                        {
                            obj.item.hideFlags = HideFlags.DontSaveInBuild;
                        }
                        else if (obj.mode == ObjectFilter.Mode.Exclude)
                        {
                            obj.item.hideFlags = HideFlags.None;
                        }
                    }
                }
            }
        }

        private void RunScript(string fileName, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                //WorkingDirectory = @"C:\Users\Patrick\Documents\Projects\steamworks_sdk_157\sdk\tools\ContentBuilder",
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            Process.Start(psi);
            //using (Process proc = Process.Start(psi))
            //{
            //    string output = proc.StandardOutput.ReadToEnd();
            //    string error = proc.StandardError.ReadToEnd();
            //    proc.WaitForExit();

            //    Debug.Log("Output: " + output);
            //    Debug.Log("Error: " + error);
            //}
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
            if (string.IsNullOrEmpty(activeSetting.scriptPath))
            {
                return;
            }
            RunScript(activeSetting.scriptPath, activeSetting.arguments);
        }
        private void CopyFolders(List<StoreSettings> storeSettings)
        {
            if (storeSettings == null)
            {
                return;
            }
            foreach (StoreSettings setting in storeSettings)
            {
                if (setting.copyFolders != null)
                {
                    CopyFolders(setting);
                }
            }
        }
        public void CopyFolders(StoreSettings storeSetting)
        {
            foreach (FolderCopyLocation copyFolder in storeSetting.copyFolders)
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
            string[] results = Directory.GetFiles(source);
            foreach (string result in results)
            {
                File.Copy(result, result.Replace(source, target), true);
            }
            string[] directories = Directory.GetDirectories(source);
            {
                foreach (string directory in directories)
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
            TryCreateDirectory(lastPath);
            lastPath = Path.Combine(lastPath, platformSetting.buildTarget.Name); //ProjectName/Builds/Platform
            TryCreateDirectory(lastPath);

            lastPath = GetBuildFolderPath(platformSetting, storeSetting); //ProjectName/Builds/Platform/Platform_Store_0.0.0
            TryCreateDirectory(lastPath);

            return lastPath;
        }
        public string GetBuildFolderPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), BUILD_FOLDER_NAME);
        }
        private string GetBuildFolderPath(PlatformSettings platformSetting, StoreSettings storeSetting)
        {
            //../Assets/Builds/WindowsStandalone64/Windows_Steam_1.0.0/

            return Path.Combine(GetBuildFolderPath(), platformSetting.buildTarget.Name,
            $"{Application.productName}_{platformSetting.buildTarget.Name}_{storeSetting.name}_{GetVersionString()}");
        }
        public string GetVersionString()
        {
            return VersionNumber.GetVersionNumber(updateType).ToString();
        }
        public int GetTotalBuilds()
        {
            int count = 0;
            foreach (PlatformSettings platform in platformSettings)
            {
                foreach (StoreSettings store in platform.storeSettings)
                {
                    count++;
                }
            }
            return count;
        }
        public int GetTotalActiveBuilds()
        {
            int count = 0;

            if (platformSettings == null)
            {
                return 0;
            }

            foreach (PlatformSettings platform in platformSettings)
            {
                if (platform.storeSettings == null)
                {
                    continue;
                }
                if (!platform.include)
                {
                    continue;
                }
                foreach (StoreSettings store in platform.storeSettings)
                {
                    if (store.include)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        public static void TryCreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Path is null!");
                return;
            }
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
            foreach (SceneOrder sceneOrder in sceneOrders)
            {
                string path = AssetDatabase.GetAssetPath(sceneOrder.scene);
                sceneNames.Add(path);
            }
            return sceneNames;
        }
    }
}