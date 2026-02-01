using System.Collections;
using System.Collections.Generic;
using HexTecGames.BuildHelper.Editor;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Tests.Editor
{
    public class MockPlatform : Platform
    {
    }

    public class MockStore : Store
    {
        public void Init(string name, bool include)
        {
            this.name = name;
            this.include = include;
            this.settingOverrides = new List<SettingOverride>();
        }
    }

    public class MockPlatformTarget : PlatformTarget
    {
        private readonly string name;
        private readonly BuildTarget target;

        public MockPlatformTarget(string name, BuildTarget target = BuildTarget.StandaloneWindows64)
        {
            this.name = name;
            this.target = target;
        }

        public override string Name => name;

        public override BuildTarget BuildTarget => target;

        public override string FileEnding => ".exe";
    }

}