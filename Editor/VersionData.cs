using HexTecGames.Basics.Editor;
using UnityEditor;

namespace HexTecGames.BuildHelper.Editor
{
    public static class VersionData
    {
        public static string CurrentVersion
        {
            get
            {
                return $"{MajorVersion}.{MediumVersion}.{MinorVersion}";
            }
        }

        public static int MinorVersion
        {
            get
            {
                return minorVersion;
            }
            private set
            {
                minorVersion = value;
            }
        }
        private static int minorVersion = 1;
        public static int MediumVersion
        {
            get
            {
                return mediumVersion;
            }
            private set
            {
                mediumVersion = value;
            }
        }
        private static int mediumVersion = 0;
        public static int MajorVersion
        {
            get
            {
                return majorVersion;
            }
            private set
            {
                majorVersion = value;
            }
        }
        private static int majorVersion = 0;

        public static VersionType CurrentVersionType;

        public static void IncreaseVersion(UpdateType updateType)
        {
            switch (updateType)
            {
                case UpdateType.None:
                    return;
                case UpdateType.Minor:
                    MinorVersion++;
                    break;
                case UpdateType.Medium:
                    MediumVersion++;
                    break;
                case UpdateType.Major:
                    MajorVersion++;
                    break;
                default:
                    break;
            }
            UpdatePlayerSettings();
        }
        public static void SetVersion(int major, int medium, int minor)
        {
            MajorVersion = major;
            MediumVersion = medium;
            MinorVersion = minor;
            UpdatePlayerSettings();
        }
        private static void UpdatePlayerSettings()
        {
#if UNITY_EDITOR
            PlayerSettings.bundleVersion = CurrentVersion;
#endif
        }
    }
}