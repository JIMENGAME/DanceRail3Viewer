using System.Collections.Generic;

namespace Milthm.BuildScript.Versioning
{
    internal class WorkspaceDirtyJudgement
    {
        // 这里填入git status --porcelain返回结果
        private static List<string> TolerantDirty = new List<string>
        {
            " M Assets/Resources/GENERAL/BuildInfo.json",
            " M Packages/manifest.json",
            " M Packages/packages-lock.json",
            " M ProjectSettings/ProjectSettings.asset",
            " M ProjectSettings/ProjectVersion.txt",
            " M ProjectSettings/ProjectSettings.asset",
            "?? Assets/TapTap/",
            "?? .gitconfig",
            " D \"Assets/UB/Install LWRP Fog.unitypackage.meta\"",
        };

        internal static bool CheckDirtyAcceptable(string item)
        {
            if (TolerantDirty.Contains(item))
            {
                return true;
            }

            if (item.StartsWith(" D Assets/Resources/PV/"))
            {
                return true;
            }

            var path = item.Substring(3);
            //
            // if (path.StartsWith("scripts/"))
            // {
            //     return true;
            // }

            if (path.StartsWith("Assets/DEBUG"))
            {
                return true;
            }
            if (path is "Assets/AddressableAssetsData/link.xml" or "Assets/AddressableAssetsData/link.xml.meta")
            {
                return true;
            }

            if (path.StartsWith("Assets/Editor"))
            {
                return true;
            }

            if (path.StartsWith("Assets/TapTap"))
            {
                return true;
            }

            return false;
        }
    }
}
