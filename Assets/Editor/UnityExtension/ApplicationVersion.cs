using System;
using DRFV.Data.Model;
using UnityEngine;
using Newtonsoft.Json;

#if !NOTANOTE_USING_CI && UNITY_EDITOR
using Milthm.BuildScript.Versioning;
#endif

namespace Milthm.Util
{
    public static class ApplicationVersion
    {
        // 开发模式下（不再 CI 环境且在 Unity Editor 环境）时通过 git 指令获得版本信息
        // 否则通过 TextAsset 获得版本信息
#if !NOTANOTE_USING_CI && UNITY_EDITOR
        private static readonly Lazy<VersionModel> GitGetCurrentVersionInfo = new Lazy<VersionModel>(() =>
        {
            var info = VersionGenerator.GetCurrentVersionInfo();
            Debug.Log(info);
            return info.Export();
        });
#else
        private static readonly Lazy<VersionModel> GitGetCurrentVersionInfo = new Lazy<VersionModel>(() =>
        {
            try
            {
                var buildInfo = Resources.Load<TextAsset>("BuildInfo");
                return JsonConvert.DeserializeObject<VersionModel>(buildInfo.text);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("版本信息未能正确设置。");
                Debug.LogWarning(ex);
                return new VersionModel();
            }
        });
#endif
        public static VersionModel VersionInfo => GitGetCurrentVersionInfo.Value;

        public static string Version => VersionInfo.FullTag;

        public static string MainVersion => VersionInfo.ShortTag;
    }
}
