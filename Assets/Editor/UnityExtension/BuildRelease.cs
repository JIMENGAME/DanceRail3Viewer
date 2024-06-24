using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using Milthm.BuildScript;
using Milthm.BuildScript.Versioning;
using Milthm.UnityExtension.Model;
using UnityEngine;

namespace Milthm.UnityExtension
{
    public class BuildRelease
    {
        private static readonly string ProjectPath = Path.Join(Application.dataPath, "..");

        private static readonly string BuildPath = Path.Join(ProjectPath, "Build");
        private static readonly string SecretsPath = Path.Join(ProjectPath, "Secrets");

        private const string GameName = "Notanote";

        private const AndroidSdkVersions AndroidTargetSdk = AndroidSdkVersions.AndroidApiLevel29;

        private const bool CheckTag = true;

        private static readonly Dictionary<BuildTarget, string> BuildTargetsToName = new Dictionary<BuildTarget, string>()
            {
                { BuildTarget.StandaloneWindows64, "Win64"},
                { BuildTarget.StandaloneLinux64, "Linux64"},
                { BuildTarget.StandaloneOSX, "MacOS"},
                { BuildTarget.iOS, "iOS"},
                { BuildTarget.Android, "Android"},
            };

        private static string GenerateBuildPath(string prefix, bool checkTag)
        {
            var t = VersionGenerator.GetCurrentVersionInfo();
            Debug.Log(DateTime.UnixEpoch);
            t.ValidateForRelease(checkTag);
            var tag = t.GenerateGameShorTag();
            var folder = $"{prefix}-{tag}";
            var ret = Path.Join(BuildPath, folder);
            Debug.Log("GenerateBuildDirectory: " + ret);
            return ret;
        }

        private static AndroidKeystoreConfig GetAndroidKeystoreConfig()
        {
            var cfg = File.ReadAllText(Path.Join(SecretsPath, "android-keystore-config.yaml"), Encoding.UTF8);
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            return deserializer.Deserialize<AndroidKeystoreConfig>(cfg);
        }


        [MenuItem("构建工具/构建/构建 Windows 可执行程序", false, 0)]
        public static void BuildForWindows()
        {
            Builder.BuildProjectWithArgs(
                "customBuildName", GameName,
                "buildTarget", BuildTarget.StandaloneWindows64.ToString(),
                "customBuildPath", Path.Join(GenerateBuildPath(BuildTargetsToName[BuildTarget.StandaloneWindows64], CheckTag), $"{GameName}.exe"),
                "notanoteReleaseChannel", "universal",
                "notanoteReleaseCheckTag", CheckTag.ToString().ToLowerInvariant(),
                "quitAfterBuild", "false",
                "openBuildFolder", "true"
            );
        }
        
        [MenuItem("构建工具/构建/构建 Linux 应用", false, 1)]
        public static void BuildForLinux()
        {
            Builder.BuildProjectWithArgs(
                "customBuildName", GameName,
                "buildTarget", BuildTarget.StandaloneLinux64.ToString(),
                "customBuildPath", Path.Join(GenerateBuildPath(BuildTargetsToName[BuildTarget.StandaloneLinux64], CheckTag), $"{GameName}.app"),
                "notanoteReleaseChannel", "universal",
                "notanoteReleaseCheckTag", CheckTag.ToString().ToLowerInvariant(),
                "quitAfterBuild", "false",
                "openBuildFolder", "true"
            );
        }

        [MenuItem("构建工具/构建/构建 MacOS 应用", false, 2)]
        public static void BuildForMacOS()
        {
            Builder.BuildProjectWithArgs(
                "customBuildName", GameName,
                "buildTarget", BuildTarget.StandaloneOSX.ToString(),
                "customBuildPath", Path.Join(GenerateBuildPath(BuildTargetsToName[BuildTarget.StandaloneOSX], CheckTag), $"{GameName}.app"),
                "notanoteReleaseChannel", "universal",
                "notanoteReleaseCheckTag", CheckTag.ToString().ToLowerInvariant(),
                "quitAfterBuild", "false",
                "openBuildFolder", "true"
            );
        }

        [MenuItem("构建工具/构建/构建 Android 国区 APK", false, 3)]
        public static void BuildAndroidAPKCN()
        {
            var ks = GetAndroidKeystoreConfig();
            Builder.BuildProjectWithArgs(
                "customBuildName", GameName,
                "buildTarget", BuildTarget.Android.ToString(),
                "customBuildPath", Path.Join(GenerateBuildPath($"{BuildTargetsToName[BuildTarget.Android]}-CN-APK", CheckTag), $"{GameName}.apk"),
                "androidKeystoreName", ks.AndroidKeystoreName,
                "androidKeystorePass", ks.AndroidKeystorePass,
                "androidKeyaliasName", ks.AndroidKeyaliasName,
                "androidKeyaliasPass", ks.AndroidKeyaliasPass,
                "androidTargetSdkVersion", $"{(int)AndroidTargetSdk}",
                "androidExportType", "androidPackage",
                "androidUseAPKExpansionFiles", "false",
                "androidSymbolType", "none",
                "notanoteReleaseChannel", "cn",
                "notanoteReleaseCheckTag", CheckTag.ToString().ToLowerInvariant(),
                "quitAfterBuild", "false",
                "openBuildFolder", "true"
            );
        }

        [MenuItem("构建工具/构建/构建 Android 国际 APK", false, 4)]
        public static void BuildAndroidAPKUniversal()
        {
            var ks = GetAndroidKeystoreConfig();
            Builder.BuildProjectWithArgs(
                "customBuildName", GameName,
                "buildTarget", BuildTarget.Android.ToString(),
                "customBuildPath", Path.Join(GenerateBuildPath($"{BuildTargetsToName[BuildTarget.Android]}-Universal-APK", CheckTag), $"{GameName}.apk"),
                "androidKeystoreName", ks.AndroidKeystoreName,
                "androidKeystorePass", ks.AndroidKeystorePass,
                "androidKeyaliasName", ks.AndroidKeyaliasName,
                "androidKeyaliasPass", ks.AndroidKeyaliasPass,
                "androidTargetSdkVersion", $"{(int)AndroidTargetSdk}",
                "androidExportType", "androidPackage",
                "androidUseAPKExpansionFiles", "false",
                "androidSymbolType", "none",
                "notanoteReleaseChannel", "universal",
                "notanoteReleaseCheckTag", CheckTag.ToString().ToLowerInvariant(),
                "quitAfterBuild", "false",
                "openBuildFolder", "true"
            );
        }

        [MenuItem("构建工具/构建/构建 Android 国际 AAB", false, 5)]
        public static void BuildAndroidAABUniversal()
        {
            var ks = GetAndroidKeystoreConfig();
            Builder.BuildProjectWithArgs(
                "customBuildName", GameName,
                "buildTarget", BuildTarget.Android.ToString(),
                "customBuildPath", Path.Join(GenerateBuildPath("Android-Universal-AAB", CheckTag), $"{GameName}.aab"),
                "androidKeystoreName", ks.AndroidKeystoreName,
                "androidKeystorePass", ks.AndroidKeystorePass,
                "androidKeyaliasName", ks.AndroidKeyaliasName,
                "androidKeyaliasPass", ks.AndroidKeyaliasPass,
                "androidTargetSdkVersion", $"{(int)AndroidTargetSdk}",
                "androidExportType", "androidAppBundle",
                "androidUseAPKExpansionFiles", "true",
                "androidSymbolType", "none",
                "notanoteReleaseChannel", "universal",
                "notanoteReleaseCheckTag", CheckTag.ToString().ToLowerInvariant(),
                "quitAfterBuild", "false",
                "openBuildFolder", "true"
            );
        }
        
        [MenuItem("构建工具/构建/构建 iOS 应用", false, 6)]
        public static void BuildForIOS()
        {
            Builder.BuildProjectWithArgs(
                "customBuildName", GameName,
                "buildTarget", BuildTarget.iOS.ToString(),
                "customBuildPath", Path.Join(GenerateBuildPath(BuildTargetsToName[BuildTarget.iOS], CheckTag), $"{GameName}.app"),
                "notanoteReleaseChannel", "universal",
                "notanoteReleaseCheckTag", CheckTag.ToString().ToLowerInvariant(),
                "quitAfterBuild", "false",
                "openBuildFolder", "true"
            );
        }
    }
}