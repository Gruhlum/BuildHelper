using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace HexTecGames.BuildHelper.Editor
{
    public static class ExternalBuilder
    {
        public static void PerformBuild()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string configPath = GetArg(args, "-configPath");

            if (string.IsNullOrEmpty(configPath))
            {
                File.WriteAllText(BuildPaths.RESULT_PATH, "FAILURE");
                return;
            }

            var buildData = JsonUtility.FromJson<BuildData>(File.ReadAllText(configPath));

            Debug.Log("External build started...");
            Debug.Log(buildData.ToString());

            BuildPipeline.BuildPlayer(buildData.GenerateBuildPlayerOptions());

            File.WriteAllText(BuildPaths.RESULT_PATH, "SUCCESS");
            Debug.Log("External build finished");
        }

        private static string GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length; i++)
                if (args[i] == name && i + 1 < args.Length)
                    return args[i + 1];
            return null;
        }
    }

}