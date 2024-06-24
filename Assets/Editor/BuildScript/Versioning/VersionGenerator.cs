using System;
using System.Linq;
using Milthm.BuildScript.System;
using Semver;

namespace Milthm.BuildScript.Versioning
{
    public static class VersionGenerator
    {
        public static CurrentVersionData GetCurrentVersionInfo()
        {
            var currentCommitId = Git.CurrentCommitId();
            var currentTag = Git.LatestTagWithCommit(currentCommitId);
            var currentVersion = SemVersion.Parse(currentTag, SemVersionStyles.AllowV);

            var allTags = Git.GetAllTagsWithCommit(currentCommitId)
                .Where(s =>
                {
                    try
                    {
                        var ver = SemVersion.Parse(s, SemVersionStyles.AllowV);
                        if (ver.Major != currentVersion.Major ||
                            ver.Minor != currentVersion.Minor)
                        {
                            return false;
                        }

                        return true;
                    }
                    catch (Exception e)
                    {
                        LogExtension.WrapLogErr(e);
                        return false;
                    }
                })
                .Select(s => (
                    Tag: s,
                    Version: SemVersion.Parse(s, SemVersionStyles.AllowV),
                    CommitTime: Git.GetCommitTimeByRefName(s)
                ))
                .OrderBy(s => s.Version);

            // AndroidBundleVersionCode
            // 50AABBCC
            // AA 为大版本号
            // BB 为小版本号
            // CC 为补丁版本
            var androidBundleVersionCode =
                $"50{TwoDigits(currentVersion.Major)}{TwoDigits(currentVersion.Minor)}{TwoDigits(currentVersion.Patch)}";

            // 你就说能不能用嘛
            var timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var appleBuildNumber = $"{timestamp}";

            var gitTagCommitTime = Git.GetCommitTimeByRefName(currentTag);
            var gitTag = currentTag;
            var gitTagCommitId = Git.GetCommitIdByRefName(currentTag);
            var workspaceDirty = Git.CurrentWorkspaceIsDirty();

            return new CurrentVersionData(
                currentVersion, gitTagCommitTime,
                gitTag, gitTagCommitId,
                ////////////////////////////////////////////////////////////////
                androidBundleVersionCode, appleBuildNumber,
                ////////////////////////////////////////////////////////////////
                currentCommitId, workspaceDirty
            );
        }

        private static string TwoDigits(int t)
        {
            if (t < 0 || t >= 100)
            {
                throw new ArgumentOutOfRangeException();
            }

            return t < 10 ? $"0{t}" : $"{t}";
        }
    }
}
