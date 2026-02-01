using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public class SceneOverride : SettingOverride
    {
        public List<SceneOrder> scenes = new List<SceneOrder>();
        [SerializeField] private MergeMode mergeMode = default;

        private enum MergeMode { Add, Set }

        public override void ApplyBeforeBuild(BuildData buildData, bool isActive)
        {
            if (!isActive)
            {
                return;
            }
            if (mergeMode == MergeMode.Set)
            {
                buildData.scenes = new List<SceneOrder>(scenes);
            }
            else buildData.scenes = Merge(buildData.scenes, scenes);
        }
        public List<SceneOrder> Merge(List<SceneOrder> scenes1, List<SceneOrder> scenes2)
        {
            var map = new Dictionary<SceneAsset, SceneOrder>();

            // Add all from scenes1
            foreach (var s in scenes1)
            {
                if (s?.scene == null)
                    continue;

                map[s.scene] = new SceneOrder
                {
                    scene = s.scene,
                    order = s.order
                };
            }

            // Merge/override with scenes2
            foreach (var s in scenes2)
            {
                if (s?.scene == null)
                    continue;

                map[s.scene] = new SceneOrder
                {
                    scene = s.scene,
                    order = s.order
                };
            }

            // Convert to list
            var result = new List<SceneOrder>(map.Values);

            result.Sort((a, b) => a.order.CompareTo(b.order));

            return result;
        }
    }
}