using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class PlatformSettings
    {
        [Tooltip("Include this the next time we build")]
        public bool include = true;
        [SerializeReference, SubclassSelector] public PlatformTarget buildTarget;
        [Tooltip("File ending of executable, e.g.: (Windows:.exe, Linux:.x86_64 or empty for OSX")]
        public string fileEnding;
        [Tooltip("Scenes to only be added for this Platform")]
        public List<SceneOrder> extraScenes;
        [Tooltip("Objects that will only be included/exluded for this Platform")]
        public List<ObjectFilter> exclusiveObjects;
        [Tooltip("Can be used to deactive specific gameObjects or to copy the builds into another folder")]
        public List<StoreSettings> storeSettings;

        public void OnValidate()
        {
            CheckFileEnding();
        }

        private void CheckFileEnding()
        {
            if (buildTarget == null)
            {
                fileEnding = string.Empty;
            }
            else fileEnding = buildTarget.FileEnding;
        }

        public void ApplySettings(StoreSettings store)
        {
            if (buildTarget != null)
            {
                buildTarget.ApplySettings(store);
            }
        }

        public void OnBuildFinished(BuildSummary summary)
        {
            if (buildTarget != null)
            {
                buildTarget.OnBuildFinished(summary);
            }
        }
        public override string ToString()
        {
            if (buildTarget == null)
            {
                return "No Build Target";
            }
            else return buildTarget.Name;
        }
    }
}