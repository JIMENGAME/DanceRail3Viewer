using System;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using Milthm.BuildScript.Context;

namespace Milthm.BuildScript.Input
{
    public static class AndroidSettings
    {
        public static void Apply(BuildContext ctx)
        {
#if UNITY_2019_1_OR_NEWER
            if (ctx.OptionsTryGet("androidKeystoreName", out string keystoreName) &&
                !string.IsNullOrEmpty(keystoreName))
            {
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = keystoreName;
            }
#endif
            // Can't use out variable declaration as Unity 2018 doesn't support it
            string keystorePass;
            if (ctx.OptionsTryGet("androidKeystorePass", out keystorePass) && !string.IsNullOrEmpty(keystorePass))
                PlayerSettings.Android.keystorePass = keystorePass;

            string keyaliasName;
            if (ctx.OptionsTryGet("androidKeyaliasName", out keyaliasName) && !string.IsNullOrEmpty(keyaliasName))
                PlayerSettings.Android.keyaliasName = keyaliasName;

            string keyaliasPass;
            if (ctx.OptionsTryGet("androidKeyaliasPass", out keyaliasPass) && !string.IsNullOrEmpty(keyaliasPass))
                PlayerSettings.Android.keyaliasPass = keyaliasPass;

            string androidTargetSdkVersion;
            if (ctx.OptionsTryGet("androidTargetSdkVersion", out androidTargetSdkVersion) &&
                !string.IsNullOrEmpty(androidTargetSdkVersion))
            {
                var targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
                try
                {
                    targetSdkVersion =
                        (AndroidSdkVersions)Enum.Parse(typeof(AndroidSdkVersions), androidTargetSdkVersion);
                }
                catch
                {
                    UnityEngine.Debug.Log("Failed to parse androidTargetSdkVersion! Fallback to AndroidApiLevelAuto");
                }

                PlayerSettings.Android.targetSdkVersion = targetSdkVersion;
            }

            string androidExportType;
            if (ctx.OptionsTryGet("androidExportType", out androidExportType) &&
                !string.IsNullOrEmpty(androidExportType))
            {
                // Only exists in 2018.3 and above
                PropertyInfo buildAppBundle = typeof(EditorUserBuildSettings)
                    .GetProperty("buildAppBundle", BindingFlags.Public | BindingFlags.Static);
                switch (androidExportType)
                {
                    case "androidStudioProject":
                        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
                        if (buildAppBundle != null)
                            buildAppBundle.SetValue(null, false);
                        break;
                    case "androidAppBundle":
                        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                        if (buildAppBundle != null)
                            buildAppBundle.SetValue(null, true);
                        break;
                    case "androidPackage":
                        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                        if (buildAppBundle != null)
                            buildAppBundle.SetValue(null, false);
                        break;
                }
            }

            string androidUseAPKExpansionFiles;
            if (ctx.OptionsTryGet("androidUseAPKExpansionFiles", out androidUseAPKExpansionFiles) &&
                !string.IsNullOrEmpty(androidUseAPKExpansionFiles))
            {
                PlayerSettings.Android.useAPKExpansionFiles = string.Equals(androidUseAPKExpansionFiles, "true",
                    StringComparison.OrdinalIgnoreCase);
            }

            string symbolType;
            if (ctx.OptionsTryGet("androidSymbolType", out symbolType) && !string.IsNullOrEmpty(symbolType))
            {
#if UNITY_2021_1_OR_NEWER
                switch (symbolType)
                {
                    case "public":
                        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
                        break;
                    case "debugging":
                        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;
                        break;
                    case "none":
                        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Disabled;
                        break;
                }
#elif UNITY_2019_2_OR_NEWER
                switch (symbolType)
                {
                    case "public":
                    case "debugging":
                        EditorUserBuildSettings.androidCreateSymbolsZip = true;
                        break;
                    case "none":
                        EditorUserBuildSettings.androidCreateSymbolsZip = false;
                        break;
                }
#endif
            }
        }
    }
}