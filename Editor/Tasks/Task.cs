using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public abstract class Task
    {
        public abstract void Run(Platform platform, BuildSettings buildSettings);
    }
}