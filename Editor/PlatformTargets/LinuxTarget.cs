using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class LinuxTarget : PlatformTarget
    {
        public override string FileEnding
        {
            get
            {
                return ".x86_64";
            }
        }

        public override BuildTarget BuildTarget
        {
            get
            {
                return BuildTarget.StandaloneLinux64;
            }
        }

        public override string Name
        {
            get
            {
                return "Linux";
            }
        }
    }
}