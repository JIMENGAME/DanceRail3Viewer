using System;
using System.IO;
using System.Text;
using DRFV.inokana;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.WSA;
using Application = UnityEngine.Application;

namespace DRFV.Global
{
    public class StaticResources : MonoSingleton<StaticResources>
    {
        public string dataPath;
        public string cachePath;

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
            dataPath = GetDataPath();
            cachePath = GetCachePath();
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            if (!Directory.Exists(dataPath + "songs"))
            {
                Directory.CreateDirectory(dataPath + "songs");
            }

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_WIN
            File.SetAttributes(cachePath, FileAttributes.Hidden);
#endif
            }
        }

        private static string GetDataPath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    return Application.dataPath + "/../";
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.IPhonePlayer:
                    return Application.persistentDataPath + "/";
                case RuntimePlatform.Android:
                    return "/storage/emulated/0/DR3Viewer/";
                default:
                    Application.Quit();
                    throw new ArgumentException("Unsupported System");
            }
        }

        private static string GetCachePath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    return Application.dataPath + "/../cache/";
                case RuntimePlatform.IPhonePlayer:
                    return Application.temporaryCachePath + "/";
                case RuntimePlatform.Android:
                    return "/storage/emulated/0/DR3Viewer/.cache/";
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    Debug.Log(Application.temporaryCachePath);
                    return Application.temporaryCachePath + "/";
                default:
                    Application.Quit();
                    throw new ArgumentException("Unsupported System");
            }
        }

        public static string BoolArrayToString(bool[] arr)
        {
            StringBuilder sb = new StringBuilder();
            foreach (bool b in arr)
            {
                sb.Append(b ? "T" : "F");
            }

            return sb.ToString();
        }

        public static bool[] StringToBoolArray(string str)
        {
            bool[] result = new bool[str.Length];
            for (int i = 0; i < result.Length; i++)
            {
                string q = str.Substring(i, 1);
                result[i] = q == "T";
                if (!result[i] && q != "F") Debug.LogWarning("警告：不合法的参数");
            }

            return result;
        }
    }
}