using HexTecGames.Basics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class StoreSettings
    {
        public string name;
        public bool include = true;

        [SubclassSelector, SerializeReference] public List<SettingsOverride> settingsOverrides = new List<SettingsOverride>();

        [Tooltip("Scenes that will only be added to this specific Build")]
        public List<SceneOrder> extraScenes;

        [Tooltip("Objects that will only be included/exluded for this Store")]
        public List<ObjectFilter> exclusiveObjects;

        [Tooltip("Used to copy the build folders to another location")]
        public List<FolderCopyLocation> copyFolders;

        [Tooltip("Location of the external script")]
        public string externalScript;
        [Tooltip("Should this script be run after Build is complete")]
        public bool runExternalScript;


        public bool HasSettingsOverride<T>(out T t) where T : SettingsOverride
        {
            if (settingsOverrides == null || settingsOverrides.Count == 0)
            {
                t = null;
                return false;
            }
            T settingsOverride = settingsOverrides.Find(x => x is T) as T;
            t = settingsOverride;
            return true;
        }
        public override string ToString()
        {
            return name;
        }
    }
}