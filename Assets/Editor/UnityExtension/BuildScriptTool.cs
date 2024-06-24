using System.Linq;
using Milthm.BuildScript.BuildFlag;
using Milthm.BuildScript.Versioning;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Milthm.UnityExtension
{
    public class BuildScriptTool
    {
        [MenuItem("构建工具/查看所有 Tag")]
        public static void GetAllTag()
        {
            Debug.Log(
                string.Join("\n", Git.GetAllTags().Select(s =>
                    {
                        var commitTime = Git.GetCommitTimeByRefName(s);
                        return (s, commitTime);
                    }).OrderBy(s => s.commitTime).Select(s => $"{s.s}: {s.commitTime.ToString()}")
                )
            );
        }

        [MenuItem("构建工具/查看当前版本信息")]
        public static void GetCurrentVersionInfo()
        {
            var info = VersionGenerator.GetCurrentVersionInfo();
            Debug.Log(info);
        }

        private static void AddScriptingDefineSymbols(NamedBuildTarget target, string flag)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbols(target).Split(";").ToHashSet();
            if (!symbols.Contains(flag)) symbols.Add(flag);
            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", symbols));
        }

        private static void AddScriptingDefineSymbols(string flag)
        {
            AddScriptingDefineSymbols(NamedBuildTarget.Android, flag);
            AddScriptingDefineSymbols(NamedBuildTarget.iOS, flag);
            AddScriptingDefineSymbols(NamedBuildTarget.Standalone, flag);
        }

        private static void RemoveScriptingDefineSymbols(NamedBuildTarget target, string flag)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbols(target).Split(";").ToHashSet();
            symbols.Remove(flag);
            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", symbols));
        }

        private static void RemoveScriptingDefineSymbols(string flag)
        {
            RemoveScriptingDefineSymbols(NamedBuildTarget.Android, flag);
            RemoveScriptingDefineSymbols(NamedBuildTarget.iOS, flag);
            RemoveScriptingDefineSymbols(NamedBuildTarget.Standalone, flag);
        }

        private static void PrintScriptingDefineSymbols()
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
            Debug.Log($"修改后的自定义宏：{symbols}");
        }


        // [MenuItem("构建工具/构建标记/频道/中国大陆")]
        // public static void SetScriptingDefineSymbolsToChinaMainland()
        // {
        //     RemoveScriptingDefineSymbols(Flags.MilthmUniversalChannelFlag);
        //     PrintScriptingDefineSymbols();
        // }
        //
        // [MenuItem("构建工具/构建标记/频道/国际版")]
        // public static void SetScriptingDefineSymbolsToInternational()
        // {
        //     AddScriptingDefineSymbols(Flags.MilthmUniversalChannelFlag);
        //     PrintScriptingDefineSymbols();
        // }
        //
        // [MenuItem("构建工具/构建标记/判定调试/开启")]
        // public static void SetScriptingDefineSymbolsEnableJudgementDebug()
        // {
        //     AddScriptingDefineSymbols(Flags.MilthmJudgementDebugFlag);
        //     PrintScriptingDefineSymbols();
        // }
        //
        // [MenuItem("构建工具/构建标记/判定调试/关闭")]
        // public static void SetScriptingDefineSymbolsDisableJudgementDebug()
        // {
        //     RemoveScriptingDefineSymbols(Flags.MilthmJudgementDebugFlag);
        //     PrintScriptingDefineSymbols();
        // }
        //
        // [MenuItem("构建工具/构建标记/版本/正式版")]
        // public static void SetScriptingDefineSymbolsReleaseVersion()
        // {
        //     RemoveScriptingDefineSymbols(Flags.NotanoteDebugFlag);
        //     RemoveScriptingDefineSymbols(Flags.NotanoteClosedBetaFlag);
        //     PrintScriptingDefineSymbols();
        // }
        //
        // [MenuItem("构建工具/构建标记/版本/内测版")]
        // public static void SetScriptingDefineSymbolsClosedBetaVersion()
        // {
        //     RemoveScriptingDefineSymbols(Flags.NotanoteDebugFlag);
        //     AddScriptingDefineSymbols(Flags.NotanoteClosedBetaFlag);
        //     PrintScriptingDefineSymbols();
        // }
        //
        // [MenuItem("构建工具/构建标记/版本/调试版 (开发组内部版) ")]
        // public static void SetScriptingDefineSymbolsDebugVersion()
        // {
        //     RemoveScriptingDefineSymbols(Flags.NotanoteClosedBetaFlag);
        //     AddScriptingDefineSymbols(Flags.NotanoteDebugFlag);
        //     PrintScriptingDefineSymbols();
        // }
    }
}
