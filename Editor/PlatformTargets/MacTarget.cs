using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class MacTarget : PlatformTarget
    {
        public override BuildTarget BuildTarget
        {
            get
            {
                return BuildTarget.StandaloneOSX;
            }
        }

        public override string FileEnding
        {
            get
            {
                return ".app";
            }
        }

        public override string Name
        {
            get
            {
                return "OSX";
            }
        }
    }
}