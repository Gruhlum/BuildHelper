using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    public class BuildPaths
    {
        public static readonly string ROOT = Directory.GetCurrentDirectory();
        public static readonly string BUILDS = Path.Combine(ROOT, BUILD_FOLDER_NAME);
        public static readonly string RESULT_PATH = Path.Combine(CONFIG_FOLDER_NAME, "ExternalBuildResult.json");
        public static readonly string CONFIG_PATH = Path.Combine(CONFIG_FOLDER_NAME, "ExternalBuildConfig.json");

        public static readonly string BUILD_FOLDER_NAME = "Builds";
        public static readonly string CONFIG_FOLDER_NAME = "BuildHelper";
    }
}