using System;
using System.Diagnostics;
using System.Linq;
using Semver;
using Milthm.BuildScript.System;
using Milthm.BuildScript.SystemExtension;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Milthm.BuildScript.Versioning
{
    public static class Git
    {
        const string application = @"git";
        
        public static string[] GetAllTags()
        {
            string version = Run("tag").Replace("\r", "");
            return version.Split("\n");
        }

        public static string[] GetAllTagsWithCommit(string commitId)
        {
            string version = Run($"tag --merged {commitId}").Replace("\r", "");
            return version.Split("\n");
        }

        public static string LatestTagWithCommit(string commitId)
        {
            // string commitId = Run("describe --tags --abbrev=0");
            return Run($"describe --tags {commitId}");
        }

        public static bool CurrentWorkspaceIsDirty()
        {
            var result = false;

            var message = Run("status --porcelain");
            Debug.Log("> git status --porcelain\n" + message);

            var flags = message.Replace("\r", "").Split("\n");
            foreach (var item in flags)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }

                if (WorkspaceDirtyJudgement.CheckDirtyAcceptable(item))
                {
                    Debug.Log($"Acceptable dirty item found: {item}");
                }
                else
                {
                    LogExtension.WrapLogErr($"Unacceptable dirty item found: {item}");
                    result = true;
                }
            }

            return result;
        }

        public static string CurrentCommitId()
        {
            string commitId = Run("rev-list --max-count=1 HEAD");
            return commitId;
        }

        public static DateTimeOffset GetCommitTimeByRefName(string refName)
        {
            string date = Run($"log -1 --format=%aI {refName}");
            return DateTimeOffset.Parse(date);
        }

        public static string GetCommitIdByRefName(string refName)
        {
            string commit = Run($"rev-list -n 1 {refName}");
            return commit;
        }

        /// <summary>
        /// Runs git binary with any given arguments and returns the output.
        /// </summary>
        static string Run(string arguments)
        {
            using (var process = new Process())
            {
                string workingDirectory = Application.dataPath;

                string output, errors;
                int exitCode = process.Run(application, arguments, workingDirectory, out output, out errors);
                if (exitCode != 0)
                {
                    throw new GitException(exitCode, errors);
                }

                return output;
            }
        }
    }
}
