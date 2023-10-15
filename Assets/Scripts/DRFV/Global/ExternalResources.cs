using System;
using System.IO;
using System.Text;
using DRFV.Global.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace DRFV.Global
{
    public class ExternalResources
    {
        private static readonly string[] TextFileSuffixes = {".txt", ".json", ".drb" };

        public static string GetVideoClipPath(string path)
        {
            return "file://" + ParsePath(path) + ".mp4";
        }
        
        public static AudioClip LoadAudioClip(string path)
        {
            path = ParsePath(path);
            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + path + ".ogg", AudioType.OGGVORBIS);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                
            }

            if (uwr.result != UnityWebRequest.Result.Success) return null;
            return DownloadHandlerAudioClip.GetContent(uwr);
        }

        public static Sprite LoadSprite(string path)
        {
            path = ParsePath(path);
            bool exist = false;
            foreach (string imageSuffix in Util.ImageSuffixes)
            {
                if (!File.Exists(path + imageSuffix)) continue;
                path = String.Concat(path, imageSuffix);
                exist = true;
                break;
            }

            if (!exist) return null;
            return Util.ByteArrayToSprite(File.ReadAllBytes(path), 0, 0);
        }

        public static TextAsset LoadText(string path)
        {
            path = ParsePath(path);
            bool exist = false;
            foreach (string textFileSuffix in TextFileSuffixes)
            {
                if (!File.Exists(path + textFileSuffix)) continue;
                path = String.Concat(path, textFileSuffix);
                exist = true;
                break;
            }

            if (!exist) return null;
            return new TextAsset(File.ReadAllText(path, Encoding.UTF8));
        }

        private static string ParsePath(string ori)
        {
            string path = ori.Replace("\\", "/");
            if (path.StartsWith("/")) path = path.Substring(1);
            path = string.Concat(StaticResources.Instance.dataPath, "resources/", path);
            return path;
        }
    }
}