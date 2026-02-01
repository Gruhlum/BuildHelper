using System.Collections;
using System.Collections.Generic;
using System.IO;
using HexTecGames.Basics;
using HexTecGames.BuildHelper.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace HexTecGames.BuildHelper.Tests.Editor
{
    public class TestBuildPathProvider : IBuildPathProvider
    {
        private readonly string root;

        public TestBuildPathProvider(string root)
        {
            this.root = root;
        }

        public string ConfigFolder => Path.Combine(root, "Config");
        public string ConfigPath => Path.Combine(ConfigFolder, "ExternalBuildConfig.json");
        public string ResultPath => Path.Combine(ConfigFolder, "ExternalBuildResult.txt");
        public string BuildsFolder => Path.Combine(root, "Builds");
    }
}