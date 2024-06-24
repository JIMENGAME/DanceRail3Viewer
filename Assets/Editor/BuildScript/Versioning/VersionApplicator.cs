using System;
using System.IO;
using Milthm.BuildScript.Context;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Milthm.BuildScript.Versioning
{
    public class VersionApplicator
    {
        public static void SetVersion(BuildContext ctx, BuildPlayerOptions buildPlayerOptions)
        {
            var info = VersionGenerator.GetCurrentVersionInfo();
            Debug.Log("Current Version:\n" + info);
            info.ValidateForRelease(ctx);

            {
                if (!int.TryParse(info.AndroidBundleVersionCode, out int androidBundleVersionCode))
                {
                    throw new ArgumentException("bundleVersionCode not validate. versionInfo:" + info);
                }

                if (androidBundleVersionCode < 0)
                {
                    throw new ArgumentException("bundleVersionCode not validate. versionInfo:" + info);
                }

                PlayerSettings.Android.bundleVersionCode = androidBundleVersionCode;
            }

            PlayerSettings.bundleVersion = info.BundleVersion;
            PlayerSettings.macOS.buildNumber = info.AppleBuildNumber;
            PlayerSettings.iOS.buildNumber = info.AppleBuildNumber;

            WriteVersionInfo(info, buildPlayerOptions);
        }

        private static void WriteVersionInfo(CurrentVersionData info, BuildPlayerOptions buildPlayerOptions)
        {
            File.WriteAllText("Assets/Resources/Texts/BuildInfo.json",
                JsonConvert.SerializeObject(info.Export(buildPlayerOptions))
            );
        }
    }
}
