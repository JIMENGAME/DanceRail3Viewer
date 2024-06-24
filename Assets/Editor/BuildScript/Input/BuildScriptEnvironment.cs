using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Milthm.BuildScript.Input
{
    public class BuildScriptEnvironment
    {
        public static bool CallFromBuildScript()
        {
            var stackTrace = new StackTrace();
            var frames = stackTrace.GetFrames();
            if (frames == null)
            {
                return false;
            }

            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                if (method.DeclaringType == typeof(Milthm.BuildScript.Builder))
                {
                    Debug.Log("call from build script");
                    return true;
                }
            }

            Debug.Log("not call from build script");
            return false;
        }
    }
}
