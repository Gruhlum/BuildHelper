using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
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
        public static string GetCurrentVersion()
        {
            return PlayerSettings.bundleVersion;
        }
        private static VersionNumber CreateVersionNumber()
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
        public static void SetVersionNumber(string versionNumber)
        {
            PlayerSettings.bundleVersion = versionNumber;
        }
        public static void SetVersionNumber(VersionNumber versionNumber)
        {
            PlayerSettings.bundleVersion = versionNumber.ToString();
        }
        public static string GetVersionNumber(UpdateType updateType)
        {
            VersionNumber number = CreateVersionNumber();
            number.ApplyIncrease(updateType);
            return number.ToString();
        }
        public static void IncreaseVersion(UpdateType updateType)
        {
            VersionNumber number = CreateVersionNumber();
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
                    Minor = 0;
                    break;
                case UpdateType.Major:
                    Major++;
                    Medium = 0;
                    Minor = 0;
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