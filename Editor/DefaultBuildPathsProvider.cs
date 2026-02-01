using System.Collections;
using System.Collections.Generic;
using HexTecGames.Basics;
using UnityEngine;
using UnityEngine.UI;

namespace HexTecGames.BuildHelper.Editor
{
    public class DefaultBuildPathProvider : IBuildPathProvider
    {
        public string ConfigFolder => BuildPaths.CONFIG_FOLDER_NAME;
        public string ConfigPath => BuildPaths.CONFIG_PATH;
        public string ResultPath => BuildPaths.RESULT_PATH;
        public string BuildsFolder => BuildPaths.BUILDS;
    }
}