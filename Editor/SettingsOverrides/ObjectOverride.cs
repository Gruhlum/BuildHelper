using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class ObjectOverride : SettingOverride
    {
        [Tooltip("Objects that will only be included/excluded")]
        public List<ObjectFilter> exclusiveObjects;

        public override void ApplyBeforeBuild(BuildData buildData, bool isActive)
        {
            foreach (var obj in exclusiveObjects)
            {
                bool include =
                    (obj.mode == ObjectFilter.Mode.Include && isActive) ||
                     (obj.mode == ObjectFilter.Mode.Exclude && !isActive);

                obj.item.hideFlags = include ? HideFlags.None : HideFlags.DontSaveInBuild;
            }
        }

        public override void ApplyAfterBuild(BuildData buildData, bool success)
        {
            ClearObjectFilters();
        }

        private void ClearObjectFilters()
        {
            foreach (ObjectFilter obj in exclusiveObjects)
            {
                obj.item.hideFlags = HideFlags.None;
            }
        }
    }
}