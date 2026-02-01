using NUnit.Framework;
using UnityEngine;
using System.IO;
using HexTecGames.BuildHelper.Editor;
using HexTecGames.Basics.Editor;
using System.Collections.Generic;
using UnityEditor;

namespace HexTecGames.BuildHelper.Tests.Editor
{
    public class BuildSettingsTests
    {
        private BuildSettings settings;
        private string tempConfigDir;

        [SetUp]
        public void Setup()
        {
            settings = ScriptableObject.CreateInstance<BuildSettings>();

            // Create a single temp root folder for this test run
            tempConfigDir = Path.Combine(Path.GetTempPath(), "BuildHelperTests_" + Path.GetRandomFileName());
            Directory.CreateDirectory(tempConfigDir);

            // Inject the test path provider
            settings.PathProvider = new TestBuildPathProvider(tempConfigDir);
        }


        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(tempConfigDir))
                Directory.Delete(tempConfigDir, true);

            Object.DestroyImmediate(settings);
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Version Save/Load
        // ─────────────────────────────────────────────────────────────────────────────

        [Test]
        public void SaveAndLoadLastBuildVersion_Works()
        {
            settings.LastBuildVersion = new VersionNumber(1, 2, 3);

            TestUtils.CallPrivate(settings, "SaveLastBuildVersion");

            settings.LastBuildVersion = new VersionNumber(0, 0, 0);

            TestUtils.CallPrivate(settings, "LoadLastBuildVersion");

            Assert.AreEqual("1.2.3", settings.LastBuildVersion.ToString());
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // BuildData Queue Creation
        // ─────────────────────────────────────────────────────────────────────────────

        [Test]
        public void CreateAllBuildDatas_CreatesCorrectQueue()
        {
            var p1 = ScriptableObject.CreateInstance<Platform>();
            p1.include = true;
            p1.platformTarget = new MockPlatformTarget("Windows");

            var steam = ScriptableObject.CreateInstance<MockStore>();
            steam.Init("Steam", true);

            var gog = ScriptableObject.CreateInstance<MockStore>();
            gog.Init("GOG", false);

            p1.Stores = new List<Store> { steam, gog };

            settings.Platforms.Add(p1);

            var queue = TestUtils.CallPrivate<Queue<BuildData>>(settings, "CreateAllBuildDatas");

            Assert.AreEqual(1, queue.Count);
        }


        // ─────────────────────────────────────────────────────────────────────────────
        // Path Generation
        // ─────────────────────────────────────────────────────────────────────────────

        [Test]
        public void GetApplicationPath_GeneratesCorrectFolder()
        {
            var platform = ScriptableObject.CreateInstance<Platform>();
            platform.platformTarget = new MockPlatformTarget("Windows");

            var store = ScriptableObject.CreateInstance<MockStore>();
            store.Init("Steam", true);

            string path = TestUtils.CallPrivate<string>(settings, "GetApplicationPath", platform, store);

            Assert.IsTrue(path.Contains("Builds"));
            Assert.IsTrue(path.Contains("Windows"));
            Assert.IsTrue(path.Contains("Steam"));
        }


        // ─────────────────────────────────────────────────────────────────────────────
        // Total Builds
        // ─────────────────────────────────────────────────────────────────────────────

        [Test]
        public void GetTotalBuilds_CountsAllStores()
        {
            var p1 = ScriptableObject.CreateInstance<Platform>();

            var steam = ScriptableObject.CreateInstance<MockStore>();
            steam.Init("Steam", true);

            var gog = ScriptableObject.CreateInstance<MockStore>();
            gog.Init("GOG", true);

            p1.Stores = new List<Store> { steam, gog };

            settings.Platforms.Add(p1);

            Assert.AreEqual(2, settings.GetTotalBuilds());
        }


        [Test]
        public void GetTotalActiveBuilds_CountsOnlyIncluded()
        {
            var p1 = ScriptableObject.CreateInstance<Platform>();
            p1.include = true;

            var steam = ScriptableObject.CreateInstance<MockStore>();
            steam.Init("Steam", true);

            var gog = ScriptableObject.CreateInstance<MockStore>();
            gog.Init("GOG", false);

            p1.Stores = new List<Store> { steam, gog };

            settings.Platforms.Add(p1);

            Assert.AreEqual(1, settings.GetTotalActiveBuilds());
        }


        // ─────────────────────────────────────────────────────────────────────────────
        // Directory Creation
        // ─────────────────────────────────────────────────────────────────────────────

        [Test]
        public void CreatePath_CreatesDirectory()
        {
            var platform = ScriptableObject.CreateInstance<Platform>();
            platform.platformTarget = new MockPlatformTarget("Windows");

            var store = ScriptableObject.CreateInstance<MockStore>();
            store.Init("Steam", true);

            string path = TestUtils.CallPrivate<string>(settings, "CreatePath", platform, store);

            Assert.IsTrue(Directory.Exists(path));
        }


        // ─────────────────────────────────────────────────────────────────────────────
        // BuildEnded Flow (Mocked)
        // ─────────────────────────────────────────────────────────────────────────────

        [Test]
        public void BuildEnded_UpdatesVersion_OnSuccess()
        {
            settings.LastBuildVersion = new VersionNumber(0, 0, 1);

            var p = ScriptableObject.CreateInstance<Platform>();
            p.include = true;
            p.platformTarget = new MockPlatformTarget("Windows");
            var steam = ScriptableObject.CreateInstance<MockStore>();
            steam.Init("Steam", true);
            p.Stores = new List<Store> { steam };
            settings.Platforms.Add(p);

            var queue = new Queue<BuildData>();
            var storePath = TestUtils.CallPrivate<string>(settings, "CreatePath", p, steam);
            queue.Enqueue(new BuildData(p, steam, VersionType.Release, BuildOptions.None, storePath, new List<SceneOrder>()));

            typeof(BuildSettings)
                .GetField("buildDatas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(settings, queue);

            TestUtils.CallPrivate(settings, "BuildEnded", "SUCCESS");

            Assert.AreEqual(VersionNumber.GetVersionNumber().ToString(), settings.LastBuildVersion.ToString());
        }

    }

}