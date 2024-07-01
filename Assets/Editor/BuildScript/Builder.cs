using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Milthm.BuildScript.Context;
using Milthm.BuildScript.Input;
using Milthm.BuildScript.Reporting;
using Milthm.BuildScript.System;
using Milthm.BuildScript.Versioning;
using DRFV.Global.Utilities;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEditor.VersionControl;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Milthm.BuildScript
{
    public static class Builder
    {
        [UsedImplicitly]
        public static void BuildProject()
        {
            var options = ArgumentsParser.GetValidatedOptions();
            BuildProjectWithArgs(options);
        }

        public static void BuildProjectWithArgs(params string[] options)
        {
            if (options.Length % 2 != 0)
            {
                throw new ArgumentException("options length must be a multiple of 2");
            }

            var pending = new Dictionary<string, string>();
            for (var i = 0; i < options.Length / 2; i++)
            {
                pending[options[i * 2]] = options[i * 2 + 1];
            }

            BuildProjectWithArgs(pending);
        }


        public static void BuildProjectWithArgs(IEnumerable<KeyValuePair<string, string>> options)
        {
            BuildProjectWithArgs(options.ToDictionary(s => s.Key, s => s.Value));
        }

        public static void BuildProjectWithArgs(Dictionary<string, string> options)
        {
            // Gather values from args
            var ctx = new BuildContext(options);
            BuildProjectWithCtx(ctx);
        }

        public static void BuildProjectWithCtx(BuildContext ctx)
        {
            // Gather values from project
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(s => s.path)
                .ToArray();
            Debug.Log($"Build Scenes: {string.Join(",", scenes)}");

            // Get all buildOptions from options
            BuildOptions buildOptions = BuildOptions.None;
            foreach (string buildOptionString in Enum.GetNames(typeof(BuildOptions)))
            {
                if (ctx.OptionsContains(buildOptionString))
                {
                    BuildOptions buildOptionEnum = (BuildOptions)Enum.Parse(typeof(BuildOptions), buildOptionString);
                    buildOptions |= buildOptionEnum;
                }
            }

#if UNITY_2021_2_OR_NEWER
            // Determine subtarget
            StandaloneBuildSubtarget buildSubtarget;
            if (!ctx.OptionsTryGet("standaloneBuildSubtarget", out var subtargetValue) ||
                !Enum.TryParse(subtargetValue, out buildSubtarget))
            {
                buildSubtarget = default;
            }
#endif

            string buildPath = ctx.OptionsGet("customBuildPath");
            // Define BuildPlayer Options
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = (BuildTarget)Enum.Parse(typeof(BuildTarget), ctx.OptionsGet("buildTarget")),
                options = buildOptions,
#if UNITY_2021_2_OR_NEWER
                subtarget = (int)buildSubtarget
#endif
            };

            // Apply Android settings
            if (buildPlayerOptions.target == BuildTarget.Android)
            {
                // VersionApplicator.SetAndroidVersionCode(options["androidVersionCode"]);
                AndroidSettings.Apply(ctx);
            }

            // Execute default AddressableAsset content build, if the package is installed.
            // Version defines would be the best solution here, but Unity 2018 doesn't support that,
            // so we fall back to using reflection instead.
            var addressableAssetSettingsType = Type.GetType(
                "UnityEditor.AddressableAssets.Settings.AddressableAssetSettings,Unity.Addressables.Editor");
            if (addressableAssetSettingsType != null)
            {
                // ReSharper disable once PossibleNullReferenceException, used from try-catch
                try
                {
                    addressableAssetSettingsType
                        .GetMethod("CleanPlayerContent", BindingFlags.Static | BindingFlags.Public)
                        .Invoke(null, new object[] { null });
                    addressableAssetSettingsType.GetMethod("BuildPlayerContent", new Type[0])
                        .Invoke(null, new object[0]);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to run default addressables build:\n{e}");
                }
            }

            // Notanote BuildFlag
            BuildFlag.BuildFlag.SetFlag(ctx, ref buildPlayerOptions);

            // Set version for this build
            VersionApplicator.SetVersion(ctx, buildPlayerOptions);
            // VersionApplicator.SetVersion(options["buildVersion"]);

            // Perform build
            BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            // Summary
            BuildSummary summary = buildReport.summary;
            StdOutReporter.ReportSummary(summary);
            
            // Edit iOS info.plist
            if (summary.result == BuildResult.Succeeded && buildPlayerOptions.target is BuildTarget.iOS or BuildTarget.StandaloneOSX)
            {
                PlistDocument plist = new PlistDocument();
                string plistPath = Path.Join(buildPath, "info.plist");
                plist.ReadFromFile(plistPath);
                PlistElementDict infoDict = plist.root;
                infoDict.SetBoolean("UIFileSharingEnabled", true); // 对第三方软件（例：iTunes）开放读写文件权限
                infoDict.SetBoolean("UISupportsDocumentBrowser", true); // 对iOS系统APP“文件”开放读写文件权限
                plist.WriteToFile(plistPath);
            }
            
            // Open build folder
            bool openBuildPath = false;
            if (ctx.OptionsTryGet("openBuildFolder", out var t))
            {
                if (bool.TryParse(t, out var b))
                {
                    openBuildPath = b;
                }
                else
                {
                    Debug.LogWarning("Failed to parse openBuildFolder option, set to default value False");
                }
            }
            
            if (openBuildPath && summary.result == BuildResult.Succeeded)
            {
                Process process = new Process();
                
#if UNITY_EDITOR_WIN
                process.StartInfo.FileName = "cmd";
#else
                process.StartInfo.FileName = "bash";
#endif
                process.StartInfo.RedirectStandardError = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                
#if UNITY_EDITOR_WIN
                process.StandardInput.WriteLine($"explorer \"{Path.GetDirectoryName(buildPath) ?? ""}\"");
#else
                process.StandardInput.WriteLine($"open \"{Path.GetDirectoryName(buildPath) ?? ""}\"");
#endif
                process.StandardInput.WriteLine("exit");
                process.WaitForExit();
            }

            // Result
            bool quitAfterBuild = true;
            if (ctx.OptionsTryGet("quitAfterBuild", out var u))
            {
                if (bool.TryParse(u, out var b))
                {
                    quitAfterBuild = b;
                }
                else
                {
                    Debug.LogWarning("Failed to parse quitAfterBuild option, set to default value True");
                }
            }

            if (quitAfterBuild)
            {
                StdOutReporter.ExitWithResult(summary.result);
            }
        }
    }
}