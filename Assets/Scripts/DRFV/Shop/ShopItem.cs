using System.IO;
using DRFV.Data;
using DRFV.Global;
using DRFV.Global.Utilities;
using DRFV.Login;
using NVorbis;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Shop
{
    public class ShopItem : MonoBehaviour
    {
        public Image cover, coverBackground;
        public Text title, artist, bpm;
        private int id;
        private ShopManager shopManager;
        private SongInfo songInfo;
        private Sprite coverSpr;
        private AudioClip previewAc;
        public bool gotCover, gotPreview;

        private string coverCachPath;
        private string previewCachPath;

        public void Init(SongInfo songInfo, int id, ShopManager shopManager, bool refresh)
        {
            this.songInfo = songInfo;
            this.id = id;
            this.shopManager = shopManager;
            coverCachPath = shopManager.cachePath + "cover." + songInfo.keyword;
            previewCachPath = shopManager.cachePath + "preview." + songInfo.keyword;
            if (songInfo.hasCover)
            {
                GetCover(refresh);
                cover.transform.parent.gameObject.SetActive(true);
                title.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                title.text = "曲名：" + songInfo.title;
                artist.text = "作者：" + songInfo.artist;
                bpm.text = "BPM：" + songInfo.bpm;
                cover.transform.parent.gameObject.SetActive(false);
                title.transform.parent.gameObject.SetActive(true);
                Texture2D texture2D = Util.SpritePlaceholder.texture;
                WriteFile(coverCachPath, texture2D.EncodeToJPG());
                SetCover(Util.SpritePlaceholder);
            }
        }

        private void GetCover(bool refresh)
        {
            if (gotCover) return;
            if (!refresh && File.Exists(coverCachPath))
            {
                byte[] data = ReadFile(coverCachPath);
                SetCover(data);
            }
            else
            {
                HttpRequest.Instance.Get(AccountInfo.Instance.urlPrefix + "public/songs/" + songInfo.keyword +
                                         "/base." +
                                         songInfo.coverSuffix +
                                         "?token=" + AccountInfo.Instance.loginToken, GetCoverSuccess,
                    () => { GetCover(refresh); });
            }
        }

        private void GetCoverSuccess(byte[] data)
        {
            WriteFile(coverCachPath, data);
            SetCover(data);
        }

        public Sprite GetCover()
        {
            return coverSpr;
        }

        public AudioClip GetPreview()
        {
            return previewAc;
        }

        private void SetCover(Sprite sprite)
        {
            coverSpr = cover.sprite = sprite;
            if (sprite != null && sprite.texture.isReadable)
                coverBackground.color = Util.GetAvgColor(sprite);
            gotCover = true;
        }

        private void SetCover(byte[] data)
        {
            Texture2D texture2D = new Texture2D(0, 0);
            texture2D.LoadImage(data);
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height),
                Vector2.zero);
            SetCover(sprite);
        }

        public void OnClicked()
        {
            shopManager.UpdatePressedItem(id);
        }

        public void GetPreview(bool refresh)
        {
            if (gotPreview) return;
            if (refresh || !File.Exists(previewCachPath))
            {
                if (songInfo.hasPreview)
                {
                    HttpRequest.Instance.Get(
                        AccountInfo.Instance.urlPrefix + "public/songs/" + songInfo.keyword + "/preview." +
                        songInfo.previewSuffix +
                        "?token=" + AccountInfo.Instance.loginToken, OnGetPreviewSuccess,
                        () => { GetPreview(refresh); });
                }
                else
                {
                    HttpRequest.Instance.Get(
                        AccountInfo.Instance.urlPrefix + "public/songs/" + songInfo.keyword + "/base." +
                        songInfo.musicSuffix +
                        "?token=" + AccountInfo.Instance.loginToken, OnGetPreviewSuccess,
                        () => { GetPreview(refresh); });
                }
            }
            else
            {
                SetPreview(ReadFile(previewCachPath));
            }
        }

        private void OnGetPreviewSuccess(byte[] data)
        {
            WriteFile(previewCachPath, data);
            SetPreview(data);
        }

        private void SetPreview(byte[] data)
        {
            previewAc = GetAudioClip(data, songInfo.hasPreview ? songInfo.previewSuffix : songInfo.musicSuffix);
            gotPreview = true;
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

        byte[] ReadFile(string source)
        {
            if (!File.Exists(source)) return new byte[0];
            using FileStream fs = new FileStream(source, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data);
            return data;
        }

        private AudioClip GetAudioClip(byte[] data, string ext)
        {
            switch (ext)
            {
                case "ogg":
                    MemoryStream memoryStream = new MemoryStream(data);
                    VorbisReader vorbis = new VorbisReader(memoryStream, false);


                    int sampleCount = (int) (vorbis.SampleRate * vorbis.TotalTime.TotalSeconds * vorbis.Channels);
                    float[] f = new float[sampleCount];
                    vorbis.ReadSamples(f, 0, f.Length);
                    AudioClip audioClip =
                        AudioClip.Create("", sampleCount, vorbis.Channels, vorbis.SampleRate, false);
                    audioClip.SetData(f, 0);

                    return audioClip;
                case "wav":
                    WAV wav = new WAV(data);
                    AudioClip audioClip1;
                    if (wav.ChannelCount == 2)
                    {
                        audioClip1 = AudioClip.Create("", wav.SampleCount, 2, wav.Frequency, false);
                        audioClip1.SetData(wav.StereoChannel, 0);
                    }
                    else
                    {
                        audioClip1 = AudioClip.Create("", wav.SampleCount, 1, wav.Frequency, false);
                        audioClip1.SetData(wav.LeftChannel, 0);
                    }

                    return audioClip1;
                default:
                    return null;
            }
        }
    }
}