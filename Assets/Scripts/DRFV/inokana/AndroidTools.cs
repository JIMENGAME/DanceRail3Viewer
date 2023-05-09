#if UNITY_EDITOR && UNITY_ANDROID

using System;
using UnityEditor;
using UnityEngine;

//1111感谢inokana大哥送来的安卓密钥自动填写工具，这样我就不会忘了写密钥了

namespace DRFV.inokana
{
    [InitializeOnLoad]
    public class KeystoreMatcher
    {
        static KeystoreMatcher()
        {
            try
            {
                PlayerSettings.Android.keystorePass = "lucario";
                PlayerSettings.Android.keyaliasName = "dancerail3viewer";
                PlayerSettings.Android.keyaliasPass = "lucario";
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to match Android Keystore.");
            }
        }
    }
}

#endif