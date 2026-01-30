using System.Collections.Generic;
using System.Linq;
using HexTecGames.Basics;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [CreateAssetMenu(menuName = "HexTecGames/Editor/BuildHelper/Platform")]
    public class Platform : ScriptableObject
    {
        [InlineSOField] public bool include;

        [SerializeReference, SubclassSelector] public PlatformTarget buildTarget;
        [Tooltip("File ending of executable, e.g.: (Windows:.exe, Linux:.x86_64 or empty for OSX")]
        public string fileEnding;

        public List<Store> Stores
        {
            get
            {
                return stores;
            }
        }
        [InlineSO(true)]
        [InlineSOField]
        [SerializeField]
        private List<Store> stores;

        [SubclassSelector, SerializeReference] public List<SettingOverride> settingOverrides = new List<SettingOverride>();


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

        public void OnBeforeBuild(BuildData buildData)
        {
            foreach (var settingOverride in settingOverrides)
            {
                settingOverride.ApplyBeforeBuild(buildData, buildData.platform == this);
            }
            foreach (var store in stores)
            {
                store.OnBeforeBuild(buildData);
            }
        }
        public void OnAfterBuild(BuildData buildData, bool success)
        {
            foreach(var settingOverride in settingOverrides)
            {
                settingOverride.ApplyAfterBuild(buildData, success);
            }
            foreach (var store in stores)
            {
                store.OnAfterBuild(buildData, success);
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