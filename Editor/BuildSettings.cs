using System;
using System.Collections;
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

    [CreateAssetMenu(fileName = nameof(BuildSettings), menuName = "HexTecGames/Editor/BuildHelper/BuildSettings")]
    public class BuildSettings : ScriptableObject
    {
        public string gameName;
        public VersionType versionType;
        public UpdateType updateType;
        public BuildOptions buildOptions;
        //External not working yet
        [HideInInspector] public BuilderType builderType;
        public enum BuilderType { @internal, external }
        [Tooltip("Scenes to be added to the Build")]
        public List<SceneOrder> scenes;

        [InlineSO(true)]
        private List<Platform> platforms = new List<Platform>();

        private List<string> fullBuildPaths = new List<string>();
        private VersionNumber lastBuildVersion = new VersionNumber(0, 0, 0);

        public static BuildSettings instance;

        private const string DATA_FILE_NAME = "Data.txt";
        public IBuildPathProvider PathProvider
        {
            get
            {
                return this.pathProvider;
            }
            set
            {
                this.pathProvider = value;
            }
        }
        [NonSerialized] private IBuildPathProvider pathProvider = new DefaultBuildPathProvider();


        private Queue<BuildData> buildDatas = default;
        private string oldProductName;
        private VersionNumber oldVersion;
        private event Action<string> OnBuildEnded;
        private static readonly string projectRoot = Directory.GetCurrentDirectory();

        public List<Platform> Platforms
        {
            get
            {
                return this.platforms;
            }
            private set
            {
                this.platforms = value;
            }
        }
        public VersionNumber LastBuildVersion
        {
            get
            {
                return this.lastBuildVersion;
            }
            set
            {
                this.lastBuildVersion = value;
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(gameName))
            {
                gameName = Application.productName;
            }
        }
        private void Awake()
        {
            instance = this;
        }
        private void OnEnable()
        {
            Debug.Log("OnEnable!");
            if (LastBuildVersion == null)
            {
                LoadLastBuildVersion();
            }
        }
        private Queue<BuildData> CreateAllBuildDatas()
        {
            Queue<BuildData> buildDatas = new Queue<BuildData>();
            foreach (var platform in Platforms)
            {
                if (!platform.include)
                {
                    continue;
                }
                foreach (var store in platform.Stores)
                {
                    if (store.include)
                    {
                        buildDatas.Enqueue(new BuildData(platform, store, versionType, buildOptions, CreatePath(platform, store), scenes));
                    }
                }
            }

            return buildDatas;
        }

        [ContextMenu("Build All")]
        public void BuildAll()
        {
            if (GetTotalActiveBuilds() <= 0)
            {
                Debug.Log("No active Builds!");
                return;
            }

            oldVersion = VersionNumber.GetVersionNumber();
            VersionNumber.SetBuildVersionNumber(oldVersion.GetIncreasedVersion(updateType));
            oldProductName = PlayerSettings.productName;
            PlayerSettings.productName = gameName;
            fullBuildPaths.Clear();

            buildDatas = CreateAllBuildDatas();

            if (buildDatas == null || buildDatas.Count <= 0)
            {
                Debug.Log("No BuildDatas!");
                return;
            }
            if (builderType == BuilderType.@internal)
            {
                BuildInternal();
            }
            else BuildExternal();
        }

        private void SaveLastBuildVersion()
        {
            FileManager.WriteToFile(PathProvider.ConfigFolder, DATA_FILE_NAME, LastBuildVersion.ToString());
        }
        private void LoadLastBuildVersion()
        {
            if (FileManager.FileExists(PathProvider.ConfigFolder, DATA_FILE_NAME))
            {
                var result = FileManager.ReadFile(PathProvider.ConfigFolder, DATA_FILE_NAME);
                LastBuildVersion = new VersionNumber(result[0]);
            }
        }
        private void AfterBuildsComplete(bool success)
        {
            PlayerSettings.productName = oldProductName;

            updateType = UpdateType.Minor;
            TryOpeningBuildFolder();
            buildDatas.Clear();
            fullBuildPaths.Clear();
            if (success)
            {
                LastBuildVersion = VersionNumber.GetVersionNumber();
            }
            SaveLastBuildVersion();
        }

        private void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void RunExternalBuild(BuildData buildData)
        {
            CreateDirectory(PathProvider.ConfigFolder);
            File.WriteAllText(PathProvider.ConfigFolder, JsonUtility.ToJson(buildData));

            File.Delete(PathProvider.ResultPath);

            string unityExe = EditorApplication.applicationPath;

            var args =
                "-batchmode " +
                "-quit " +
                $"-projectPath \"{projectRoot}\" " +
                $"-executeMethod HexTecGames.BuildHelper.Editor.ExternalBuilder.PerformBuild " +
                $"-configPath \"{PathProvider.ConfigPath}\"";

            Debug.Log("Starting External Builder");

            Process.Start(unityExe, args);
        }

        private void BuildEnded(string result)
        {
            EditorApplication.update += Check;
            OnBuildEnded -= BuildEnded;

            foreach (var platform in Platforms)
            {
                platform.OnAfterBuild(buildDatas.Peek(), result.Contains("SUCCESS"));
                Debug.Log("Successfully build: " + buildDatas.Peek().ToString());
            }
            buildDatas.Dequeue();

            if (buildDatas.Count <= 0)
            {
                AfterBuildsComplete(result.Contains("SUCCESS"));
            }
            else BuildExternal(buildDatas.Peek());
        }
        private void BuildInternal()
        {
            List<string> debugs = new List<string>();
            bool anySuccess = false;
            foreach (var buildData in buildDatas)
            {
                foreach (var platform in Platforms)
                {
                    platform.OnBeforeBuild(buildData);
                }

                var report = BuildPipeline.BuildPlayer(buildData.GenerateBuildPlayerOptions());
                debugs.Add($"Build: {buildData.platform} | {buildData.store} {report.summary.result}");
                bool success = report.summary.result == BuildResult.Succeeded;

                if (success)
                {
                    fullBuildPaths.Add(buildData.GetPlatformPath());
                    anySuccess = true;
                }
                foreach (var platform in Platforms)
                {
                    platform.OnAfterBuild(buildData, success);
                }
            }
            foreach (var debug in debugs)
            {
                Debug.Log(debug);
            }
            if (fullBuildPaths.Count <= 0)
            {
                VersionNumber.SetBuildVersionNumber(oldVersion);
            }
            AfterBuildsComplete(anySuccess);
        }
        private void BuildExternal()
        {
            BuildExternal(buildDatas.Peek());
        }
        private void BuildExternal(BuildData buildData)
        {
            foreach (var platform in Platforms)
            {
                platform.OnBeforeBuild(buildData);
            }
            EditorApplication.update += Check;
            OnBuildEnded += BuildEnded;
            RunExternalBuild(buildData);
        }
        private void Check()
        {
            if (File.Exists(PathProvider.ResultPath))
            {
                EditorApplication.update -= Check;
                string result = File.ReadAllText(PathProvider.ResultPath);
                OnBuildEnded?.Invoke(result);
            }
        }

        private string CreatePath(Platform activePlatform, Store activeStore)
        {
            string path = GetApplicationPath(activePlatform, activeStore);
            CreateDirectory(path);
            return path;
        }
        public void OpenBuildFolder()
        {
            CreateDirectory(PathProvider.BuildsFolder);
            Process.Start(PathProvider.BuildsFolder);
        }
        private void TryOpeningBuildFolder()
        {
            if (fullBuildPaths != null && fullBuildPaths.Count > 0)
            {
                Debug.Log("HERE: " + fullBuildPaths[0]);
                if (!ExplorerUtils.IsFolderOrParentOpen(fullBuildPaths[0]))
                {
                    Process.Start("explorer.exe", fullBuildPaths[0]);
                }
            }
        }
        private string GetApplicationPath(Platform platform, Store store)
        {
            //ProjectName/Builds/Platform/Platform_Store_0.0.0
            return Path.Combine(PathProvider.BuildsFolder, platform.platformTarget.Name, GetApplicationFolderName(platform, store));
        }

        private string GetApplicationFolderName(Platform platform, Store store)
        {
            string versionSuffix;
            if (versionType == VersionType.Demo)
            {
                versionSuffix = $"{GetVersionString()}_Demo";
            }
            else versionSuffix = GetVersionString();

            return $"{Application.productName}_{platform.platformTarget.Name}_{store.name}_{versionSuffix}";
        }

        public string GetVersionString()
        {
            return VersionNumber.GetVersionNumber(updateType).ToString();
        }
        public int GetTotalBuilds()
        {
            int count = 0;

            foreach (Platform platform in Platforms)
            {
                if (platform == null || platform.Stores == null)
                {
                    continue;
                }
                foreach (var store in platform.Stores)
                {
                    if (store != null)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        public int GetTotalActiveBuilds()
        {
            int count = 0;

            foreach (Platform platform in Platforms)
            {
                if (platform == null || platform.Stores == null)
                {
                    continue;
                }
                if (!platform.include)
                {
                    continue;
                }
                foreach (var store in platform.Stores)
                {
                    if (store == null)
                    {
                        continue;
                    }
                    if (store.include)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}