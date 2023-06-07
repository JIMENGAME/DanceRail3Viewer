using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DG.Tweening;
using DRFV.Global;
using DRFV.Login;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DRFV.Shop
{
    public class ShopManager : MonoBehaviour
    {
        private SongInfo[] songInfos;

        public GameObject shopList;

        public GameObject shopItemPrefab;

        public RectTransform mainPanel;

        private int nowSelectedIndex = -1;

        public Image cover, coverBackground;

        public AudioSource previewSource;

        private ShopItem[] shopItems = new ShopItem[0];

        public GameObject selectSongPanel, mask;

        public Text titleText, artistText, bpmText;

        public Button[] queueOrders;
        public Sprite unpressedSprite, pressedSprite;

        private int queue;

        private bool atSelect;

        private bool inited;

        public Text tiersText;

        public Button downloadButton, backButton;

        public Text progress;

        public string dataPath, cachePath;

        private bool mainRefresh;

        // Start is called before the first frame update
        void Start()
        {
            queue = PlayerPrefs.GetInt("ShopItemsQueue", 0); 
            AccountInfo.Instance.UpdateAccountPanel();
            if (AccountInfo.Instance == null || HttpRequest.Instance == null || StaticResources.Instance == null ||
                AccountInfo.Instance.acountStatus != AccountInfo.AcountStatus.LOGINED)
            {
                SceneManager.LoadScene("main");
                return;
            }

            dataPath = StaticResources.Instance.dataPath;
            cachePath = StaticResources.Instance.cachePath;
            if (AccountInfo.Instance.songlistGot)
            {
                songInfos = AccountInfo.Instance.SongInfos;
                RequeueShopItems(queue);
            }
            else GetSongList();

            MoveToSelect();
        }

        public void Back()
        {
            if (!atSelect) MoveToSelect();
            else if (inited) SceneManager.LoadScene("main");
        }

        private Vector2 selectPos = new(0f, 0f),
            itemPos = new(0, 1145f);

        private void MoveToSelect()
        {
            atSelect = true;
            mainPanel.DOAnchorPos(selectPos, 0.5f).SetEase(Ease.OutExpo);
        }

        private void MoveToItem()
        {
            atSelect = false;
            mainPanel.DOAnchorPos(itemPos, 0.5f).SetEase(Ease.OutExpo);
        }

        public void RefreshSonglist()
        {
            mainRefresh = true;
            GetSongList();
        }

        public void GetSongList()
        {
            UpdateSongPanelState(false);
            string url = AccountInfo.Instance.urlPrefix + "?type=get_song_list&token=" + AccountInfo.Instance.loginToken;
            HttpRequest.Instance.Get(url, OnGetSongListSuccess, () => { SceneManager.LoadScene("main"); });
        }

        private object queueLock = new();

        public void RequeueShopItems(int queue)
        {
            lock (queueLock)
            {
                this.queue = queue;
                PlayerPrefs.SetInt("ShopItemsQueue", queue);
                PlayerPrefs.Save();
                selectSongPanel.SetActive(false);
                Fuck();
                mask.SetActive(false);
            }
        }

        private void OnGetSongListSuccess(byte[] data)
        {
            JObject jObject = HttpRequest.Data2JObject(data);
            string status = jObject["status"].ToString();
            if (status.Equals("success"))
            {
                List<SongInfo> songInfos = new();
                JArray jArray = jObject["songs"].ToObject<JArray>();
                foreach (JObject jo in jArray)
                {
                    try
                    {
                        SongInfo songInfo = new SongInfo();
                        songInfo.keyword = jo["keyword"].ToString();
                        songInfo.title = jo["title"].ToString();
                        songInfo.artist = jo["artist"].ToString();
                        songInfo.bpm = jo["bpm"].ToString();
                        songInfo.musicSuffix = jo["music_suffix"].ToString();
                        songInfo.hasPreview = jo["has_preview"].ToObject<bool>();
                        songInfo.previewSuffix = jo["preview_suffix"].ToString();
                        songInfo.hasCover = jo["has_cover"].ToObject<bool>();
                        songInfo.coverSuffix = jo["cover_suffix"].ToString();
                        songInfo.tiers = jo["hards"].ToObject<List<int>>().ToArray();
                        songInfos.Add(songInfo);
                    }
                    catch (NullReferenceException)
                    {
                        Debug.LogError("Unknown Exception");
                    }
                }

                this.songInfos = songInfos.ToArray();
                mask.SetActive(false);
                Fuck();
            }
        }

        private void Fuck()
        {
            if (previewSource.isPlaying) previewSource.Stop();
            getMoreSongButton.SetActive(shopItems.Length < songInfos.Length);
            QueueSongInfos();
            GenerateShopItem(true);
        }

        private void QueueSongInfos()
        {
            for (var i = 0; i < queueOrders.Length; i++)
            {
                queueOrders[i].image.sprite = i == queue ? pressedSprite : unpressedSprite;
            }

            nowSelectedIndex = -1;
            List<SongInfo> songInfos = this.songInfos.ToList();
            if (queue != 0)
            {
                songInfos.Sort((a, b) =>
                {
                    string aaa, bbb;
                    switch (queue)
                    {
                        default:
                            aaa = a.keyword;
                            bbb = b.keyword;
                            break;
                        case 2:
                            aaa = a.title;
                            bbb = b.title;
                            break;
                        case 3:
                            aaa = a.artist;
                            bbb = b.artist;
                            break;
                    }
        
                    return aaa.Equals(bbb) ? 0 : System.String.Compare(aaa, bbb, StringComparison.Ordinal);
                });
            }
            AccountInfo.Instance.SongInfos = this.songInfos = songInfos.ToArray();
            if (AccountInfo.Instance.songlistGot) return;
            AccountInfo.Instance.songlistGot = true;
            inited = true;
        }

        private int id;
        private int loadPerTime = 15;

        private void GenerateShopItem(bool full)
        {
            int addRange = Math.Min(songInfos.Length - (full ? 0 : shopItems?.Length ?? 0), loadPerTime);
            if (full)
            {
                id = 0;
                shopList.transform.parent.gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                if (shopItems != null)
                {
                    foreach (ShopItem shopItem in shopItems)
                    {
                        Destroy(shopItem.gameObject);
                    }
                }

                shopList.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                shopList.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                shopItems = new ShopItem[addRange];
            }
            else
            {
                ShopItem[] tmp = new ShopItem[(shopItems?.Length ?? 0) + addRange];
                if (shopItems != null)
                {
                    for (int i = 0; i < shopItems.Length; i++)
                    {
                        tmp[i] = shopItems[i];
                    }
                }

                shopItems = tmp;
            }

            for (int i = 0; i < addRange; i++, id++)
            {
                SongInfo songInfo = songInfos[id];
                GameObject go = Instantiate(shopItemPrefab, shopList.transform);
                ShopItem shopItem = go.GetComponent<ShopItem>();
                shopItem.Init(songInfo, id, this, mainRefresh);
                shopItems[id] = shopItem;
            }

            shopList.transform.parent.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }

        public GameObject getMoreSongButton;

        public void GetMoreSong()
        {
            mask.SetActive(true);
            GenerateShopItem(false);
            getMoreSongButton.SetActive(shopItems.Length < songInfos.Length);
            mask.SetActive(false);
        }

        private object updateLock = new();

        public void UpdatePressedItem(int id)
        {
            lock (updateLock)
            {
                if (nowSelectedIndex == id)
                {
                    MoveToItem();
                    UpdateSongDetailPanel();
                }
                else
                {
                    nowSelectedIndex = id;

                    if (previewSource.isPlaying) previewSource.Stop();
                    shopItems[nowSelectedIndex].GetPreview(false);
                    StartCoroutine(UpdateSelectSongPanel());
                }
            }
        }

        private void UpdateSongDetailPanel()
        {
            forceDownload = false;
            tiersText.text = "Tiers：" + string.Join(", ", songInfos[nowSelectedIndex].tiers);
            progress.text = "";
        }

        private void UpdateSongPanelState(bool isOn)
        {
            selectSongPanel.SetActive(isOn);
            mask.SetActive(!isOn);
        }

        private IEnumerator UpdateSelectSongPanel()
        {
            UpdateSongPanelState(false);
            titleText.text = songInfos[nowSelectedIndex].title;
            artistText.text = songInfos[nowSelectedIndex].artist;
            bpmText.text = "BPM " + songInfos[nowSelectedIndex].bpm;
            yield return new WaitUntil(() => shopItems[nowSelectedIndex].gotPreview);
            previewSource.clip = shopItems[nowSelectedIndex].GetPreview();
            yield return new WaitUntil(() => shopItems[nowSelectedIndex].gotCover);
            cover.sprite = shopItems[nowSelectedIndex].GetCover();
            coverBackground.color = songInfos[nowSelectedIndex].hasCover
                ? Util.GetAvgColor(cover.sprite)
                : Util.SpritePlaceholderBGColor;
            previewSource.Play();
            UpdateSongPanelState(true);
        }

        // private void OnInitCoverSuccess(byte[] data)
        // {
        //     try
        //     {
        //         Texture2D texture2D = new Texture2D(0, 0);
        //         texture2D.LoadImage(data);
        //         covers[nowSelectedIndex] =
        //             Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        //         shopItems[nowSelectedIndex].SetCover(covers[nowSelectedIndex]);
        //     }
        //     catch (Exception)
        //     {
        //         // ignored
        //     }
        // }
        //
        // private void OnGetCoverSuccess(byte[] data)
        // {
        //     try
        //     {
        //         Texture2D texture2D = new Texture2D(0, 0);
        //         texture2D.LoadImage(data);
        //         covers[nowSelectedIndex] =
        //             Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        //         UpdateCover();
        //     }
        //     catch (Exception)
        //     {
        //         // ignored
        //     }
        // }

        #region 下载歌曲

        private bool forceDownload;

        public void Download()
        {
            SetDownloadingState(true);
            if (!forceDownload && Directory.Exists(dataPath + "/songs/" + songInfos[nowSelectedIndex].keyword))
            {
                progress.text = "警告：本地文件中已拥有该曲目，是否重新下载？（重新点击下载按钮）";
                forceDownload = true;
            }

            progress.text = "请求谱面信息...";

            HttpRequest.Instance.Get(
                AccountInfo.Instance.urlPrefix +
                $"?type=get_song_info&token={AccountInfo.Instance.loginToken}&song_keyword={songInfos[nowSelectedIndex].keyword}",
                OnDownloadSuccess, OnDownloadFailed);
        }

        private void OnDownloadSuccess(byte[] data)
        {
            string localFolder = dataPath + "/songs/";
            progress.text = "下载中...";
            string urlPrefix = AccountInfo.Instance.urlPrefix + "public/songs/";
            JObject result = HttpRequest.Data2JObject(data);
            if (!result.ContainsKey("status") || !result["status"].ToString().Equals("success") ||
                !result.ContainsKey("urls"))
            {
                progress.text = "错误：谱面信息未找到，请检查keyword是否正确";
                SetDownloadingState(false);
                return;
            }

            List<FileStruct> fileUrls = new List<FileStruct>();

            JArray urls = result["urls"].ToObject<JArray>();
            foreach (string path in urls)
            {
                fileUrls.Add(new FileStruct
                {
                    url = urlPrefix + songInfos[nowSelectedIndex].keyword + "/" + path,
                    path = localFolder + songInfos[nowSelectedIndex].keyword + "/" + path
                });
            }

            StartCoroutine(DownloadFiles(fileUrls.ToArray(), localFolder));
        }

        private void SetDownloadingState(bool state)
        {
            backButton.interactable = downloadButton.interactable = !state;
        }

        private IEnumerator DownloadFiles(FileStruct[] fileUrls, string localFolder)
        {
            foreach (var fileStruct in fileUrls)
            {
                FileStruct fileUrl = fileStruct;
                using UnityWebRequest unityWebRequest =
                    new UnityWebRequest(fileUrl.url + "?token=" + AccountInfo.Instance.loginToken, "GET")
                    {
                        downloadHandler = new DownloadHandlerBuffer()
                    };

                unityWebRequest.SetRequestHeader("User-Agent", HttpRequest.UserAgent);

                yield return unityWebRequest.SendWebRequest();
                if (unityWebRequest.result == UnityWebRequest.Result.Success)
                {
                    fileStruct.data = unityWebRequest.downloadHandler.data;
                }
                else
                {
                    progress.text = "网络错误";
                    SetDownloadingState(false);
                    yield break;
                }
            }

            if (Directory.Exists(localFolder + songInfos[nowSelectedIndex].keyword))
            {
                new DirectoryInfo(localFolder + songInfos[nowSelectedIndex].keyword).Delete(true);
            }

            if (!Directory.Exists(localFolder + songInfos[nowSelectedIndex].keyword))
            {
                Directory.CreateDirectory(localFolder + songInfos[nowSelectedIndex].keyword);
            }

            foreach (FileStruct file in fileUrls)
            {
                WriteFile(file.path, file.data);
            }

            string filePath = localFolder + songInfos[nowSelectedIndex].keyword + "/info.txt";

            StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8);
            string[] infos = new string[3];
            for (int i = 0; i < 3; i++)
            {
                infos[i] = streamReader.ReadLine();
            }

            streamReader.Close();

            string qwq = dataPath + "Songlist.json";
            JArray songlist = Util.ReadJson(qwq)["songs"].ToObject<JArray>();
            for (int i = 0; i < songlist.Count; i++)
            {
                JToken fk = songlist[i];
                JObject ds = fk.ToObject<JObject>();
                if (ds != null && ds.ContainsKey("keyword") &&
                    songInfos[nowSelectedIndex].keyword.Equals(ds["keyword"].ToString()))
                {
                    songlist.Remove(fk);
                }
            }

            JObject jObject = new JObject
            {
                {"keyword", songInfos[nowSelectedIndex].keyword}, {"name", Util.ToBase64(infos[0])},
                {"artist", Util.ToBase64(infos[1])}, {"bpm", infos[2]}
            };
            songlist.Add(jObject);
            JObject a = new JObject {{"songs", songlist}};
            StreamWriter streamWriter = new StreamWriter(qwq, false, Encoding.UTF8);
            streamWriter.Write(a.ToString());
            streamWriter.Close();

            progress.text = "完成！";
            SetDownloadingState(false);
        }

        private void OnDownloadFailed()
        {
            progress.text = "网络错误";
        }

        void WriteFile(string destination, byte[] data)
        {
            DirectoryInfo directoryInfo = new FileInfo(destination).Directory;
            if (directoryInfo is {Exists: false})
            {
                Directory.CreateDirectory(directoryInfo.FullName);
            }

            using FileStream fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write);
            fileStream.Write(data, 0, data.Length);
        }

        #endregion

        private class FileStruct
        {
            public string url;
            public string path;
            public byte[] data;
        }
    }


    public struct SongInfo
    {
        public string keyword;
        public string title;
        public string artist;
        public string bpm;
        public string musicSuffix;
        public bool hasPreview;
        public string previewSuffix;
        public bool hasCover;
        public string coverSuffix;
        public int[] tiers;
    }
}