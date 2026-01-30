using System.Collections;
using System.Collections.Generic;
using HexTecGames.Basics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HexTecGames.BuildHelper.Editor
{
    public abstract class WebGLOverride : SettingOverride
    {
        protected override bool IsValid(Platform platform)
        {
            return platform.buildTarget is WebGLTarget;
        }
        protected override string GetInvalidMessage()
        {
            return "Not a WebGL build!";
        }
    }
}