#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace Milthm.BuildScript.BuildFlag
{
    public class Flags
    {
        public const string NotanoteClosedBetaFlag = "CLOSED_BETA_VERSION";

        public const string NotanoteDebugFlag = "DEVELOPMENT_VERSION";

        public const string NotanoteUniversalChannelFlag = "NOTANOTE_INTERNATIONAL_VER";

        public const string NotanoteJudgementDebugFlag = "NOTANOTE_JUDGEMENT_DEBUG";

        // CI 用的

        public const string NotanoteBuildInCi = "NOTANOTE_USING_CI";

        public const string NotanoteUniversalChannelCiFlag = "NOTANOTE_CI_INTERNATIONAL_VER";

        private static Dictionary<string, string?> FilterMapping = new Dictionary<string, string>()
        {
            { NotanoteUniversalChannelCiFlag, NotanoteUniversalChannelFlag },
            { NotanoteJudgementDebugFlag, NotanoteJudgementDebugFlag },
            { NotanoteUniversalChannelFlag, null },
            { NotanoteBuildInCi, null },
            { NotanoteClosedBetaFlag, null },
            { NotanoteDebugFlag, null }
        };

        // Filter for releases
        private static IEnumerable<string> FilterForRelease(IEnumerable<string> symbols)
        {
            var ret = new List<string>();
            foreach (var item in symbols)
            {
                if (FilterMapping.TryGetValue(item, out var val))
                {
                    if (val != null)
                    {
                        ret.Add(val);
                    }
                }
                else if (item.StartsWith("notanote", StringComparison.OrdinalIgnoreCase))
                {
                    throw new AggregateException($"unknown notanote build symbol: {item}");
                }
            }

            return ret;
        }

        private static IEnumerable<string> GetReleasesSymbolsForTarget(NamedBuildTarget target)
        {
            return FilterForRelease(PlayerSettings.GetScriptingDefineSymbols(target).Split(";"));
        }

        public static List<string> GetReleasesSymbolsAndVerify(BuildPlayerOptions buildPlayerOptions)
        {
            var android = GetReleasesSymbolsForTarget(NamedBuildTarget.Android).ToList();
            var ios = GetReleasesSymbolsForTarget(NamedBuildTarget.iOS).ToList();
            var standalone = GetReleasesSymbolsForTarget(NamedBuildTarget.Standalone).ToList();

            if (!SymbolEquals(android, ios) || !SymbolEquals(android, standalone) || !SymbolEquals(android, standalone))
            {
                throw new Exception(
                    "build symbol verification error: the symbol of android ios standalone platform mismatched");
            }

            var ret = new List<string>();
            ret.AddRange(android);
            ret.AddRange(FilterForRelease(buildPlayerOptions.extraScriptingDefines));
            return ret;
        }

        private static bool SymbolEquals(List<string> a, List<string> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            if (a.Any(item => !b.Any(s => s == item)))
            {
                return false;
            }

            if (b.Any(item => !a.Any(s => s == item)))
            {
                return false;
            }

            return true;
        }
    }
}