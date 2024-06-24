#nullable enable
using System;
using System.Collections.Generic;
using Milthm.BuildScript.BuildFlag;
using Milthm.BuildScript.Context;
using Milthm.BuildScript.Input;
using DRFV.Data.Model;
using Semver;
using Milthm.BuildScript.System;
using UnityEditor;
using UnityEngine;

namespace Milthm.BuildScript.Versioning
{
    public class VersionData
    {
        /// <summary>
        /// 对应的 Tag 的 semver 结构
        /// </summary>、
        public SemVersion Version { get; private set; }

        /// <summary>
        /// 安卓构建需要用到的 Bundle Version Code
        /// </summary>
        public string AndroidBundleVersionCode { get; private set; }

        /// <summary>
        /// 安卓构建需要用到的 Bundle Version Code
        /// </summary>
        public string AppleBuildNumber { get; private set; }

        /// <summary>
        /// 版本号 Tag 的 Commit 时间
        /// </summary>
        public DateTimeOffset GitTagCommitTime { get; private set; }

        /// <summary>
        /// 对应的 Tag 名字
        /// </summary>
        public string GitTag { get; private set; }

        /// <summary>
        /// 对应的 Tag 的 Commit ID
        /// </summary>
        public string GitTagCommitId { get; private set; }

        /// <summary>
        /// Unity 里用的版本号
        /// </summary>
        public string BundleVersion => $"{Version.Major}.{Version.Minor}.{Version.Patch}";

        public VersionData(
            SemVersion version, DateTimeOffset gitTagCommitTime,
            string gitTag, string gitTagCommitId,
            ////////////////////////////////////////////////////////////////
            string androidBundleVersionCode,
            string appleBuildNumber
        )
        {
            Version = version;
            GitTagCommitTime = gitTagCommitTime;
            GitTag = gitTag;
            GitTagCommitId = gitTagCommitId;

            AndroidBundleVersionCode = androidBundleVersionCode;
            AppleBuildNumber = appleBuildNumber;
        }

        public override string ToString()
        {
            return $@"Version: {Version}
GitTagCommitTime: {GitTagCommitTime}
GitTag: {GitTag}
GitTagCommitId: {GitTagCommitId}
BundleVersion: {BundleVersion}
AndroidBundleVersionCode: {AndroidBundleVersionCode}
AppleBuildNumber: {AppleBuildNumber}";
        }
    }

    public class CurrentVersionData : VersionData
    {
        public string CurrentCommitId { get; private set; }

        public bool WorkspaceIsDirty { get; private set; }

        public CurrentVersionData(
            SemVersion version, DateTimeOffset gitTagCommitTime,
            string gitTag, string gitTagCommitId,
            ////////////////////////////////////////////////////////////////
            string androidBundleVersionCode, string appleBuildNumber,
            ////////////////////////////////////////////////////////////////
            string currentCommitId, bool workspaceIsDirty
        ) :
            base(version, gitTagCommitTime, gitTag, gitTagCommitId, androidBundleVersionCode, appleBuildNumber)
        {
            CurrentCommitId = currentCommitId;
            WorkspaceIsDirty = workspaceIsDirty;
        }

        public void ValidateForRelease(bool checkTagError)
        {
            var err = new List<string>();
            
            if (CurrentCommitId != GitTagCommitId && checkTagError)
            {
                err.Add($"currentCommitId({CurrentCommitId}) != gitTagCommitId({GitTagCommitId}), " +
                        "please tag head before release");
            }

            if (WorkspaceIsDirty)
            {
                err.Add("current workspace is dirty, please commit before release");
            }

            if (err.Count > 0)
            {
                throw new ArgumentException(string.Join("\n", err));
            }
        }

        public void ValidateForRelease(BuildContext ctx)
        {
            ValidateForRelease(string.Equals(ctx.OptionsGet("NOTANOTE_RELEASE_CHECK_TAG", "true"), "true", StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            return $@"{base.ToString()}
================================
CurrentCommitId: {CurrentCommitId}
WorkspaceIsDirty: {WorkspaceIsDirty}";
        }

        public string GenerateGameShorTag()
        {
            var dirty = WorkspaceIsDirty;
            var versionTag = GitTag;

            return dirty ? $"{versionTag} dirty" : versionTag;
        }

        public string GenerateGameFullTag()
        {
            var versionTag = GitTag;
            var commit = CurrentCommitId[..8];
            var dirty = WorkspaceIsDirty;

            if (versionTag.Contains("+"))
            {
                versionTag += $"--{commit}";
            }
            else
            {
                versionTag += $"+--{commit}";
            }

            return dirty ? $"{versionTag}-dirty" : versionTag;
        }


        public VersionModel Export(BuildPlayerOptions? buildPlayerOptions = null)
        {
            var ret = new VersionModel();
            ret.FullTag = GenerateGameFullTag();
            ret.ShortTag = GenerateGameShorTag();
            ret.Version = Version.ToString();
            ret.StructuredVersion.Major = Version.Major;
            ret.StructuredVersion.Minor = Version.Minor;
            ret.StructuredVersion.Patch = Version.Patch;
            ret.StructuredVersion.Prerelease = Version.Prerelease;
            ret.StructuredVersion.Metadata = Version.Metadata;
            ret.BundleVersion = BundleVersion;
            ret.AndroidBundleVersionCode = AndroidBundleVersionCode;
            ret.AppleBuildNumber = AppleBuildNumber;
            ret.BuildSymbols = buildPlayerOptions != null
                ? Flags.GetReleasesSymbolsAndVerify(buildPlayerOptions.Value)
                : new List<string>();

#if !NOTANOTE_USING_CI && UNITY_EDITOR
            if (!BuildScriptEnvironment.CallFromBuildScript())
            {
                ret.GitTagCommitTime = GitTagCommitTime;
                ret.CurrentCommitId = CurrentCommitId;
                ret.WorkspaceIsDirty = WorkspaceIsDirty;
            }
            else
            {
                ret.GitTagCommitTime = default;
                ret.CurrentCommitId = default;
                ret.WorkspaceIsDirty = default;
            }
#else
#endif
            return ret;
        }
    }
}