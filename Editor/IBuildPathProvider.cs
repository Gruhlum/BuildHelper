using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    public interface IBuildPathProvider
    {
        string ConfigFolder { get; }
        string ConfigPath { get; }
        string ResultPath { get; }
        string BuildsFolder { get; }
    }

}