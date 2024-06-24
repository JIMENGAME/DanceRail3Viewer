using System;
using System.Linq;
using Milthm.BuildScript.Context;
using Milthm.BuildScript.System;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Milthm.BuildScript.BuildFlag
{
    public class BuildFlag
    {
        public static void SetFlag(BuildContext ctx, ref BuildPlayerOptions buildPlayerOptions)
        {
            var channel = ctx.OptionsGet("NOTANOTE_RELEASE_CHANNEL");
            switch (channel)
            {
                case "cn":
                    SetCnFlag(ref buildPlayerOptions);
                    break;
                case "universal":
                    SetUniversalFlag(ref buildPlayerOptions);
                    break;
                default:
                    throw new ArgumentException($"Unknown Notanote Release Channel: {channel}");
            }

            Debug.Log($"Current Release Channel: {channel}");
        }

        private static void SetCnFlag(ref BuildPlayerOptions buildPlayerOptions)
        {
            buildPlayerOptions.extraScriptingDefines = (buildPlayerOptions.extraScriptingDefines ?? new string[] { })
                .Append(Flags.NotanoteBuildInCi)
                .ToArray();
        }

        private static void SetUniversalFlag(ref BuildPlayerOptions buildPlayerOptions)
        {
            buildPlayerOptions.extraScriptingDefines = (buildPlayerOptions.extraScriptingDefines ?? new string[] { })
                .Append(Flags.NotanoteBuildInCi)
                .Append(Flags.NotanoteUniversalChannelCiFlag)
                .ToArray();
        }
    }
}
