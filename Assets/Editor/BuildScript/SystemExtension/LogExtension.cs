using System;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Milthm.BuildScript.System
{
    /// <summary>
    /// 避免在非 CI 中把 CI 卡掉了
    /// </summary>
    public static class LogExtension
    {
        public static void WrapLogErr(string s)
        {
            if (Application.dataPath.StartsWith("/github/workspace"))
            {
                Debug.LogError(s);
            }
            else
            {
                Debug.Log(s);
            }
        }

        public static void WrapLogErr(Exception s)
        {
            if (Application.dataPath.StartsWith("/github/workspace"))
            {
                Debug.LogError(s);
            }
            else
            {
                Debug.Log(s);
            }
        }
    }
}
