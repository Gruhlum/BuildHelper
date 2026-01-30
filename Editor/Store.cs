using System;
using System.Collections.Generic;
using HexTecGames.Basics;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [CreateAssetMenu(menuName = "HexTecGames/Editor/BuildHelper/Store")]
    public class Store : ScriptableObject
    {
        [InlineSOField] public bool include;
        [SerializeField] private bool createZip = default;

        [SubclassSelector, SerializeReference] public List<SettingOverride> settingOverrides = new List<SettingOverride>();
        [SubclassSelector, SerializeReference] public List<Task> tasks;

        public bool CreateZip
        {
            get
            {
                return this.createZip;
            }
            set
            {
                this.createZip = value;
            }
        }

        public override string ToString()
        {
            return name;
        }

        internal void OnBeforeBuild(BuildData buildData)
        {
            foreach (var settingOverride in settingOverrides)
            {
                settingOverride.ApplyBeforeBuild(buildData,  buildData.store == this);
            }
        }

        internal void OnAfterBuild(BuildData buildData, bool success)
        {
            if (success && createZip)
            {
                ZipHelper.CreateZipFile(buildData.storePath);
            }

            foreach (var settingOverride in settingOverrides)
            {
                settingOverride.ApplyAfterBuild(buildData, success);
            }
        }
    }
}