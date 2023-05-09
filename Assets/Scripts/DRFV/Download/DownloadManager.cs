#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace DRFV.Download
{
    public class DownloadManager : MonoBehaviour
    {
        public string api = "http://www.hadoulucario.shop/DanceRail/DR3Test/";

        public string outputPath = "";

        private string keyword;

        [Range(1, 20)] private int tier;

        private bool isMusicDownlading, isChartDownloading;

        private bool isSniffing;

        // Start is called before the first frame update
        void Start()
        {
            api = api.Replace("\\", "/");
            outputPath = outputPath.Replace("\\", "/");
            if (!api.EndsWith("/")) api += "/";
            if (!outputPath.EndsWith("/")) outputPath += "/";
        }

        public void SetKeyword(string str)
        {
            keyword = str;
        }

        public void SetTier(string str)
        {
            try
            {
                tier = int.Parse(str);
            }
            catch (FormatException)
            {
            }
        }

        private IEnumerator StartDownloadMusic()
        {
            isMusicDownlading = true;
            UnityWebRequest uwr = UnityWebRequest.Get(api + keyword + ".ogg");
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.ConnectionError)
            {
                switch (uwr.responseCode)
                {
                    case 200:
                        if (!Directory.Exists(outputPath + "dr3fv/" + keyword))
                        {
                            Directory.CreateDirectory(outputPath + "dr3fv/" + keyword);
                        }

                        using (FileStream fileStream =
                               new FileStream(outputPath + keyword + ".ogg", FileMode.Create, FileAccess.Write))
                        {
                            fileStream.Write(uwr.downloadHandler.data);
                        }

                        using (FileStream fileStream =
                               new FileStream(outputPath + "dr3fv/" + keyword + "/base.ogg", FileMode.Create,
                                   FileAccess.Write))
                        {
                            fileStream.Write(uwr.downloadHandler.data);
                        }

                        using (StreamWriter streamWriter = new StreamWriter(outputPath + "dr3fv/" + keyword + "/info.txt",
                                   false, Encoding.UTF8))
                        {
                            streamWriter.WriteLine(keyword);
                            streamWriter.WriteLine("-");
                            streamWriter.WriteLine("???");
                        }
                        Debug.Log($"{keyword}.ogg下载成功");
                        break;
                    case 404:
                        Debug.LogError($"{keyword}.ogg不存在，可能是keyword有误或存在于游戏内");
                        break;
                    default:
                        Debug.LogError($"下载{keyword}.ogg时遇到未知未知错误");
                        break;
                }
            }
            else
            {
                Debug.LogError("下载音乐文件时遇到未知未知错误");
            }

            isMusicDownlading = false;
        }

        private IEnumerator StartDownloadChart()
        {
            isChartDownloading = true;
            UnityWebRequest uwr = UnityWebRequest.Get(api + $"{keyword}.{tier}.txt");
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.ConnectionError)
            {
                switch (uwr.responseCode)
                {
                    case 200:
                        if (!Directory.Exists(outputPath + "dr3fv/" + keyword))
                        {
                            Directory.CreateDirectory(outputPath + "dr3fv/" + keyword);
                        }

                        using (FileStream fileStream =
                               new FileStream(outputPath + "dr3fv/" + keyword + "/" + tier + ".txt", FileMode.Create,
                                   FileAccess.Write))
                        {
                            fileStream.Write(uwr.downloadHandler.data);
                        }

                        using (FileStream fileStream =
                               new FileStream(outputPath + $"{keyword}.{tier}.txt", FileMode.Create, FileAccess.Write))
                        {
                            fileStream.Write(uwr.downloadHandler.data);
                        }

                        Debug.Log($"{keyword}.{tier}.txt下载成功");
                        break;
                    case 404:
                        if (!isSniffing) Debug.LogError($"{keyword}.{tier}.txt不存在，可能是keyword或tier有误");
                        break;
                    default:
                        Debug.LogError($"下载{keyword}.{tier}.txt时遇到未知未知错误");
                        break;
                }
            }
            else
            {
                Debug.LogError("下载音乐文件时遇到未知未知错误");
            }

            isChartDownloading = false;
        }

        private IEnumerator StartChartsSniffing()
        {
            int tmp = tier;
            for (tier = 1; tier <= 20; tier++)
            {
                yield return StartDownloadChart();
            }

            tier = tmp;
            Debug.Log("嗅探完毕");
        }

        public void ChartsSniffing()
        {
            if (isChartDownloading) return;
            isSniffing = true;
            StartCoroutine(StartChartsSniffing());
        }

        public void DownloadChart()
        {
            if (isChartDownloading) return;
            isSniffing = false;
            StartCoroutine(StartDownloadChart());
        }

        public void DownloadMusic()
        {
            if (isMusicDownlading) return;
            StartCoroutine(StartDownloadMusic());
        }
    }
}

#endif