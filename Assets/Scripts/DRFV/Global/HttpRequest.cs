using System.Collections;
using System.Collections.Generic;
using System.Text;
using DRFV.inokana;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace DRFV.Global
{
    public class HttpRequest : MonoSingleton<HttpRequest>
    {
    
        public const string JsonType = "application/json",
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36 Edg/108.0.1462.54";
    
        public void Post(string url, byte[] data = null, [CanBeNull] PostSuccessCallback success = null,
            [CanBeNull] PostFailedCallback failed = null, [CanBeNull] Dictionary<string, string> customHeader = null, [CanBeNull] int? timeout = null)
        {
            StartCoroutine(PostEnumerator(url, data, success, failed, customHeader, timeout));
        }

        public void Get(string url, [CanBeNull] GetSuccessCallback success = null,
            [CanBeNull] GetFailedCallback failed = null, [CanBeNull] Dictionary<string, string> customHeader = null, [CanBeNull] int? timeout = null)
        {
            StartCoroutine(GetEnumerator(url, success, failed, customHeader, timeout));
        }

        public delegate void PostSuccessCallback();

        public delegate void GetSuccessCallback(byte[] data);

        public delegate void PostFailedCallback();

        public delegate void GetFailedCallback();

        private IEnumerator PostEnumerator(string url, byte[] data, [CanBeNull] PostSuccessCallback success,
            [CanBeNull] PostFailedCallback failed, [CanBeNull] Dictionary<string, string> customHeader, [CanBeNull] int? timeout = null)
        {
            data ??= new byte[0];
            using UnityWebRequest unityWebRequest =
                new UnityWebRequest(url, "POST")
                {
                    uploadHandler =
                        new UploadHandlerRaw(data)
                        {
                            contentType = "application/json"
                        }
                };
        
            unityWebRequest.SetRequestHeader("Content-Type", JsonType);
            unityWebRequest.SetRequestHeader("User-Agent", UserAgent);
            if (customHeader != null)
            {
                foreach ((string key, string value) in customHeader)
                {
                    unityWebRequest.SetRequestHeader(key, value);
                }
            }
            if (timeout != null)
            {
                unityWebRequest.timeout = (int) timeout;
            }

            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result == UnityWebRequest.Result.Success) success?.Invoke();
            else failed?.Invoke();
        }

        private IEnumerator GetEnumerator(string url, [CanBeNull] GetSuccessCallback success,
            [CanBeNull] GetFailedCallback failed, [CanBeNull] Dictionary<string, string> customHeader, [CanBeNull] int? timeout = null)
        {
            using UnityWebRequest unityWebRequest =
                new UnityWebRequest(url, "GET")
                {
                    downloadHandler = new DownloadHandlerBuffer()
                };

            unityWebRequest.SetRequestHeader("User-Agent", UserAgent);
            unityWebRequest.SetRequestHeader("Accept", JsonType);
            if (customHeader != null)
            {
                foreach ((string key, string value) in customHeader)
                {
                    unityWebRequest.SetRequestHeader(key, value);
                }
            }

            if (timeout != null)
            {
                unityWebRequest.timeout = (int) timeout;
            }
        
            yield return unityWebRequest.SendWebRequest();
            if (unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                success?.Invoke(unityWebRequest.downloadHandler.data);
            }
            else failed?.Invoke();
        }

        public static JObject Data2JObject(byte[] data)
        {
            string result = Encoding.UTF8.GetString(data);
            string bom = Encoding.UTF8.GetString(new byte[] {239, 187, 191});
            if (result.StartsWith(bom))
            {
                result = result.Substring(bom.Length);
            }

            return JObject.Parse(result);
        }
    }
}