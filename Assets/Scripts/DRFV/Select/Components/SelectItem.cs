using System;
using System.Collections;
using System.IO;
using DRFV.Global;
using DRFV.Pool;
using Unimage;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DRFV.Select.Components
{
    public class SelectItem : MonoBehaviour
    {
        public int _id;
        public Image songCover;
        public Text songName;
        public Text songArtist;
        public Image press;
        public bool pressed;
        private TheSelectManager.SongData _songData;
        private AudioClip preview;

        public void Init(TheSelectManager.SongData songData, int id)
        {
            _id = id;
            _songData = songData;
            songName.text = songData.songName;
            songArtist.text = songData.songArtist;
            ReadCoverByIO();
            press.color = new Color(1.0f, 1.0f, 1.0f, 0f);
        }

        // private IEnumerator ReadCover()
        // {
        //     string filePath = StaticResources.Instance.dataPath + "songs/" + songKeyword +
        //                       "/base";
        //     if (File.Exists(filePath + ".jpg")) filePath += ".jpg";
        //     else if (File.Exists(filePath + ".png")) filePath += ".png";
        //     else
        //     {
        //         _songData.cover = null;
        //         yield return new WaitForSecondsRealtime(0.1f);
        //         songCover.sprite = _placeholder;
        //         yield break;
        //     }
        //
        //     UnityWebRequest unityWebRequest = UnityWebRequestTexture.GetTexture("file://" + filePath);
        //     yield return unityWebRequest.SendWebRequest();
        //     if (unityWebRequest.isDone)
        //     {
        //         Texture2D texture2D = DownloadHandlerTexture.GetContent(unityWebRequest);
        //         _songData.cover = songCover.sprite = Sprite.Create(texture2D, new Rect(0,0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        //     }
        // }

        private IEnumerator GetPreviewOld()
        {
            if (preview == null)
            {
                string temp = StaticResources.Instance.dataPath + "songs/" + _songData.keyword +
                              "/preview";
                string temp1 = StaticResources.Instance.dataPath + "songs/" + _songData.keyword +
                               "/base";
                if (File.Exists(temp + ".ogg"))
                {
                    yield return ReadAudio(temp + ".ogg", AudioType.OGGVORBIS, true);
                }
                else if (File.Exists(temp + ".wav"))
                {
                    yield return ReadAudio(temp + ".wav", AudioType.WAV, true);
                }
                else if (File.Exists(temp1 + ".ogg"))
                {
                    yield return ReadAudio(temp1 + ".ogg", AudioType.OGGVORBIS, false);
                    CutPreview();
                }
                else if (File.Exists(temp1 + ".wav"))
                {
                    yield return ReadAudio(temp1 + ".wav", AudioType.WAV, false);
                    CutPreview();
                }
            }

            IEnumerator ReadAudio(string path, AudioType audioType, bool saveToCache)
            {
                string md5 = "";
                if (saveToCache)
                {
                    md5 = Util.GetMD5(path);
                    preview = PoolManager.Instance.Get(PoolManager.Instance.audioClipPool, md5);
                }

                if (!saveToCache || preview == null)
                {
                    using var uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + path, audioType);
                    yield return uwr.SendWebRequest();
                    if (uwr.isDone)
                    {
                        preview = DownloadHandlerAudioClip.GetContent(uwr);
                        if (saveToCache)
                        {
                            PoolManager.Instance.Push(PoolManager.Instance.audioClipPool, md5, preview);
                        }
                    }
                }
            }
        }

        private void CutPreview()
        {
            if (_songData.preview < 0 || preview == null) return;
            float[] data1 = new float[preview.samples * preview.channels];
            preview.GetData(data1, (int)(_songData.preview * preview.frequency));
            int length = preview.frequency * preview.channels * 20;
            float[] data2 = new float[length];
            for (int index = 0; index < length; ++index)
            {
                if (index <= preview.frequency * preview.channels)
                {
                    data2[index] = (float)(data1[index] * (index * 1f / (preview.frequency * preview.channels)) * 0.7);
                }
                else if (index >= preview.frequency * preview.channels * 19)
                {
                    data2[index] = (float)(data1[index] *
                                           ((length - index) * 1f / (preview.frequency * preview.channels)) * 0.7);
                }
                else
                {
                    data2[index] = data1[index] * 0.7f;
                }
            }

            AudioClip clip = AudioClip.Create("", length / 2, preview.channels, preview.frequency, false);
            clip.SetData(data2, 0);
            SavWav.Save(StaticResources.Instance.dataPath + "songs/" + _songData.keyword + "/preview.wav", clip);
            PoolManager.Instance.Push(PoolManager.Instance.audioClipPool,
                Util.GetMD5(StaticResources.Instance.dataPath + "songs/" + _songData.keyword + "/preview.wav"), clip);
            preview = clip;
        }

        private void ReadCoverByIO()
        {
            string filePath = StaticResources.Instance.dataPath + "songs/" + _songData.keyword +
                              "/base";
            bool hasCover = false;
            foreach (string suffix in Util.ImageSuffixes)
            {
                if (!File.Exists(filePath + suffix)) continue;
                filePath = string.Concat(filePath, suffix);
                hasCover = true;
                break;
            }
            if (!hasCover)
            {
                _songData.cover = null;
                songCover.sprite = Util.SpritePlaceholder;
                return;
            }

            _songData.cover = songCover.sprite = LoadTextureByIO(filePath);
        }

        private Sprite LoadTextureByIO(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);
            byte[] bytes = new byte[fs.Length];
            try
            {
                if (fs.Read(bytes) != bytes.Length) throw new ArgumentException();
            }
            catch (IOException e)
            {
                Debug.Log(e);
            }
            catch (ArgumentException e)
            {
                Debug.Log(e);
            }

            fs.Close();
            
            string md5 = Util.GetMD5(bytes);
            Sprite coverInPool = PoolManager.Instance.Get(PoolManager.Instance.spritePool, md5);
            if (coverInPool != null) return coverInPool;

            Sprite sprite = Util.ByteArrayToSprite(bytes, _songData.keyword);
            PoolManager.Instance.Push(PoolManager.Instance.spritePool, md5, sprite);
            return sprite;
        }

        public void OnPressed()
        {
            if (processing) return;
            StartCoroutine(OnPressedC());
        }

        private bool processing = false;

        private IEnumerator OnPressedC()
        {
            processing = true;
            if (pressed)
            {
                TheSelectManager.Instance.MoveToDiff();
                processing = false;
                yield break;
            }

            if (preview == null) yield return GetPreviewOld();

            TheSelectManager.Instance.UpdatePressedSong(_id, _songData);
            TheSelectManager.Instance.SetPreview(preview);
            processing = false;
        }
    }
}