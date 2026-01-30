using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class RunScript : Task
    {
        [Tooltip("Location of external script")]
        [TextArea, SerializeField]
        private string scriptPath
            = "C:\\Users\\NAME\\Documents\\Projects\\steamworks_sdk_157\\sdk\\tools\\ContentBuilder\\scripts\\[NAME]_app_build.vdf";
        [TextArea, SerializeField] private string arguments = "+login [LOGIN] +run_app_build ..\\scripts\\[FILENAME_app_build].vdf";


        private void StartProcess(string fileName, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            Process.Start(psi);
        }

        private void RunExternalScript(List<Store> stores)
        {
            if (string.IsNullOrEmpty(scriptPath))
            {
                return;
            }
            StartProcess(scriptPath, arguments);
        }

        public override void Run(Platform platform, BuildSettings buildSettings)
        {
            //RunExternalScript(platform.Stores.Select(x => x.store).ToList());
        }
    }
}