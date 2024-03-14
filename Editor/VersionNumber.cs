using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.Editor.BuildHelper
{
    public class VersionNumber
    {
        private int Major;
        private int Medium;
        private int Minor;

        private VersionNumber(int major, int medium, int minor)
        {
            this.Major = major;
            this.Medium = medium;
            this.Minor = minor;
        }

        private static VersionNumber GetCurrentVersion()
        {
            string result = PlayerSettings.bundleVersion;
            int major = 0;
            int medium = 0;
            int minor = 0;

            var results = result.Split(".");

            if (results.Length == 3)
            {
                int.TryParse(results[0], out major);
                int.TryParse(results[1], out medium);
                int.TryParse(results[2], out minor);
            }
            return new VersionNumber(major, medium, minor);
        }
        public static void IncreaseVersion(UpdateType updateType)
        {
            VersionNumber number = GetCurrentVersion();
            number.ApplyIncrease(updateType);
            number.SaveToBuild();
        }
        private void ApplyIncrease(UpdateType updateType)
        {
            switch (updateType)
            {
                case UpdateType.None:
                    return;
                case UpdateType.Minor:
                    Minor++;
                    break;
                case UpdateType.Medium:
                    Medium++;
                    break;
                case UpdateType.Major:
                    Major++;
                    break;
                default:
                    break;
            }
        }
        public override string ToString()
        {
            return $"{Major}.{Medium}.{Minor}";
        }
        private void SaveToBuild()
        {
            PlayerSettings.bundleVersion = this.ToString();
        }
    }
}