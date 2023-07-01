using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DG.Tweening;
using DRFV.Data;
using DRFV.Enums;
using DRFV.Game;
using DRFV.Global;
using DRFV.Global.Managers;
using DRFV.inokana;
using DRFV.JsonData;
using DRFV.Language;
using DRFV.Login;
using DRFV.Pool;
using DRFV.Result;
using DRFV.Select.Components;
using DRFV.Setting;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DRFV.Select
{
    public class TheSelectManager : MonoSingleton<TheSelectManager>
    {
        private GlobalSettings currentSettings;
        public RectTransform mainObject;
        public SelectItem[] selectItems;
        public SongData songDataNow;
        public GameObject songItemPrefab;
        public GameObject content;
        public GameObject[] SongOrders;
        public Sprite imageButton, imageButtonPressed;
        public DiffButton[] diffButtons;

        public RectTransform infoBoxPanel;

        private List<SonglistItem> songlist;

        public GameObject diffContent;
        public GameObject diffButtonPrefab;

        public Toggle isAuto;
        public Toggle isMirror;
        public Toggle useSkillCheck;
        public Toggle isHard;

        [Header("Song Info Box")] public Image songCover;

        public Text songName;
        public Text songArtist;
        public Text songBpm;

        public GameObject ScoreDebugPanel;

        public Dropdown HPBarTypeDropdown, SongSpeedDropdown, JudgeTimeDropdown;

        private bool atSelect;

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            currentSettings = GlobalSettings.CurrentSettings;
            AccountInfo.Instance.UpdateAccountPanel();
            noteSpeedPrefix = "> " + LanguageManager.Instance.GetText("select.notespeed");
            isAuto.isOn = currentSettings.IsAuto;
            isMirror.isOn = currentSettings.IsMirror;
            useSkillCheck.isOn = currentSettings.SkillCheckMode;
            isHard.isOn = currentSettings.HardMode;
            noteSpeedSlider.value = currentSettings.NoteSpeed;
            UpdateNoteSpeed();
#if UNITY_EDITOR
            ScoreDebugPanel.SetActive(true);
#else
            ScoreDebugPanel.SetActive(false);
#endif
            HPBarTypeDropdown.value = currentSettings.HPBarType;
            SongSpeedDropdown.value = currentSettings.SongSpeed;
            int noteJudgeRangeCount = Util.GetNoteJudgeRangeCount();
            while (JudgeTimeDropdown.options.Count < noteJudgeRangeCount)
            {
                JudgeTimeDropdown.options.Add(new Dropdown.OptionData());
            }
            for (int i = 0; i < noteJudgeRangeCount; i++)
            {
                JudgeTimeDropdown.options[i].text = Util.GetNoteJudgeRange(i).displayName;
            }
            JudgeTimeDropdown.value = currentSettings.NoteJudgeRange;
            atSelect = true;
            mainObject.anchoredPosition = selectPos;
            infoBoxPanel.anchoredPosition = new Vector2(infoOffPosX, infoBoxPanel.anchoredPosition.y);
            ReadSonglist();
        }

        void ReadSonglist()
        {
            string filePath = StaticResources.Instance.dataPath + "Songlist.json";
            if (!File.Exists(filePath))
            {
                Debug.Log("No Songlist file founded, Creating Songlist.");
                Util.CreateSonglist();
                Debug.Log("Created Songlist.");
                return;
            }

            queue = currentSettings.SelectOrder;
            QueueBy(false);
        }

        private string FromBase64(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        private void RemakeSongItem(SELECT_ORDER orderBy, bool refreshCachePools)
        {
            if (refreshCachePools)
            {
                PoolManager.Instance.Clear(PoolManager.Instance.spritePool);
                PoolManager.Instance.Clear(PoolManager.Instance.audioClipPool);
            }

            string lastSelectedKeyword = PlayerPrefs.GetString("last_selected_song", "");
            content.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            if (previewSource.isPlaying) previewSource.Stop();
            songlist = Util.ReadSonglist().songs;
            switch (orderBy)
            {
                case SELECT_ORDER.KEYWORD:
                    songlist = new List<SonglistItem>(songlist.OrderBy(item => item.keyword));
                    break;
                case SELECT_ORDER.NAME:
                    songlist = new List<SonglistItem>(songlist.OrderBy(item =>
                        FromBase64(item.name)));
                    break;
                case SELECT_ORDER.ARTIST:
                    songlist = new List<SonglistItem>(songlist.OrderBy(item =>
                        FromBase64(item.artist)));
                    break;
            }

            List<SelectItem> qwq = new List<SelectItem>();

            int id = 0;
            int lastSelectedId = -1;
            foreach (var songlistItem in songlist)
            {
                SongData songData = new SongData
                {
                    keyword = songlistItem.keyword,
                    songName = songlistItem.name,
                    songArtist = songlistItem.artist,
                    bpm = songlistItem.bpm,
                    preview = songlistItem.preview
                };

                if (!Directory.Exists(StaticResources.Instance.dataPath + "songs/" +
                                      songData.keyword)) continue;
                songData.cover = null;
                songData.hards = GetExistTiers(songData.keyword);
                if (songData.keyword == lastSelectedKeyword) lastSelectedId = id;
                GameObject item = Instantiate(songItemPrefab, content.transform);
                SelectItem selectItem = item.GetComponent<SelectItem>();
                selectItem.Init(songData, id);
                qwq.Add(selectItem);
                id++;
            }

            selectItems = qwq.ToArray();

            GridLayoutGroup gridLayoutGroup = content.GetComponent<GridLayoutGroup>();
            content.GetComponent<RectTransform>().sizeDelta = new Vector2(gridLayoutGroup.cellSize.x,
                id * gridLayoutGroup.spacing.y + id * gridLayoutGroup.cellSize.y);

            if (lastSelectedId > 0)
            {
                selectItems[lastSelectedId].OnPressed();
                content.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(0, Math.Max(0, lastSelectedId * gridLayoutGroup.cellSize.y - 300));
            }

            songDataNow = null;
        }

        private Regex number = new("[^0-9]");

        private int[] GetExistTiers(string keyword)
        {
            string[] files = Directory.GetFiles(StaticResources.Instance.dataPath +
                                                "songs/" + keyword);
            List<int> tiers = new List<int>();
            foreach (string filePath in files)
            {
                if (filePath.EndsWith(".txt") && !number.IsMatch(Path.GetFileNameWithoutExtension(filePath)))
                {
                    try
                    {
                        tiers.Add(int.Parse(Path.GetFileNameWithoutExtension(filePath)));
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
            }

            tiers.Sort();

            return tiers.ToArray();
        }

        #region 画布移动

        private Vector2 editPos = new(2830f, 0f),
            createPos = new(380f, -1075f),
            deletePos = new(380f, 1150f),
            diffPos = new(-1000f, 0f),
            selectPos = new(380f, 0f),
            sequencePos = new(2830f, -1075f);

        private float infoOnPosX = 1870f,
            infoOffPosX = 1470f;

        public void MoveToEdit()
        {
            atSelect = false;
            mainObject.DOAnchorPos(editPos, 0.5f).SetEase(Ease.OutExpo);
        }

        public void MoveToCreate()
        {
            atSelect = false;
            mainObject.DOAnchorPos(createPos, 0.5f).SetEase(Ease.OutExpo);
        }

        public void MoveToDelete()
        {
            atSelect = false;
            mainObject.DOAnchorPos(deletePos, 0.5f).SetEase(Ease.OutExpo);
        }

        public void MoveToDiff()
        {
            atSelect = false;
            mainObject.DOAnchorPos(diffPos, 0.5f).SetEase(Ease.OutExpo);
            infoBoxPanel.DOAnchorPosX(infoOnPosX, 0.5f).SetEase(Ease.OutExpo);
        }

        public void MoveToSelect()
        {
            if (atSelect)
            {
                BackToMain();
                return;
            }

            atSelect = true;
            mainObject.DOAnchorPos(selectPos, 0.5f).SetEase(Ease.OutExpo);
            infoBoxPanel.DOAnchorPosX(infoOffPosX, 0.5f).SetEase(Ease.OutExpo);
        }

        public void MoveToSequence()
        {
            atSelect = false;
            mainObject.DOAnchorPos(sequencePos, 0.5f).SetEase(Ease.OutExpo);
        }

        #endregion

        // Update is called once per frame
        void Update()
        {
            if (FadeManager.Instance.isFading()) return;
            if (Input.GetKey(KeyCode.Escape))
            {
                BackToMain();
            }
        }

        private void BackToMain()
        {
            GlobalSettings.CurrentSettings = currentSettings;
            GlobalSettings.Save();
            SaveLastSelectedSong();
            FadeManager.Instance.Back();
        }

        public GameObject[] hideInNoDiffs;

        private void SetSongNow(SongData songData)
        {
            songDataNow = songData;
            RefreshSongNow();

            if (diffContent.transform.childCount > 0)
            {
                for (int i = 0; i < diffContent.transform.childCount; i++)
                {
                    Destroy(diffContent.transform.GetChild(i).gameObject);
                }
            }

            foreach (var t in hideInNoDiffs)
            {
                t.SetActive(songData.hards.Length > 0);
            }

            if (songData.hards.Length == 0) return;
            List<DiffButton> list = new List<DiffButton>();
            for (int i = 0; i < songData.hards.Length; i++)
            {
                GameObject go = Instantiate(diffButtonPrefab, diffContent.transform);
                DiffButton diffButton = go.GetComponent<DiffButton>();
                diffButton.Init(songData.hards[i], i);
                list.Add(diffButton);
            }

            diffButtons = list.ToArray();
            diffButtons[0].OnClicked();
        }

        private void SaveLastSelectedSong()
        {
            if (songDataNow == null) return;
            PlayerPrefs.SetString("last_selected_song", songDataNow.keyword);
            PlayerPrefs.Save();
        }

        private object pressLock = new();

        public AudioSource previewSource;

        public void UpdatePressedSong(int id, SongData songData)
        {
            lock (pressLock)
            {
                for (int i = 0; i < selectItems.Length; i++)
                {
                    if (i == id)
                    {
                        selectItems[i].pressed = true;
                        selectItems[i].press.color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
                        SetSongNow(songData);
                    }
                    else
                    {
                        selectItems[i].pressed = false;
                        selectItems[i].press.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    }
                }
            }
        }

        public void SetPreview(AudioClip audioClip)
        {
            if (previewSource.isPlaying) previewSource.Stop();
            previewSource.clip = audioClip;
            previewSource.Play();
        }

        public Slider noteSpeedSlider;
        public Text noteSpeedLabel;
        public string noteSpeedPrefix;

        public void UpdateNoteSpeed()
        {
            currentSettings.NoteSpeed = (int)noteSpeedSlider.value;
            noteSpeedLabel.text = noteSpeedPrefix + (currentSettings.NoteSpeed / 2.0f).ToString("f1") + "x";
        }

        private int? selectedDiff;
        private object diffLock = new();
        private int selectedDiffIdx;

        public void UpdateDiff(int idx)
        {
            lock (diffLock)
            {
                for (int i = 0; i < diffButtons.Length; i++)
                {
                    if (i == idx)
                    {
                        selectedDiff = diffButtons[i].tier;
                        selectedDiffIdx = i;
                        diffButtons[i].highlightAnimator.SetActive(true);
                    }
                    else
                    {
                        diffButtons[i].highlightAnimator.SetActive(false);
                    }
                }
            }
        }

        public void DeleteHighScore()
        {
            PlayerPrefs.DeleteKey(songDataNow.keyword + "." + selectedDiff);
            PlayerPrefs.Save();
            diffButtons[selectedDiffIdx].highScore.gameObject.SetActive(false);
        }
        //
        // private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        //
        // public static long CurrentTimeMillis()
        // {
        //     return (long) (DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        // }
        //
        // private AudioClip GetAudioClip(string path)
        // {
        //     using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        //     fs.Seek(0, SeekOrigin.Begin);
        //     byte[] data = new byte[fs.Length];
        //     fs.Read(data, 0, data.Length);
        //     if (path.EndsWith(".ogg"))
        //     {
        //         MemoryStream memoryStream = new MemoryStream(data);
        //         VorbisReader vorbis = new VorbisReader(memoryStream, false);
        //
        //
        //         int sampleCount = (int) (vorbis.SampleRate * vorbis.TotalTime.TotalSeconds * vorbis.Channels);
        //         float[] f = new float[sampleCount];
        //         vorbis.ReadSamples(f, 0, f.Length);
        //         AudioClip audioClip =
        //             AudioClip.Create("", sampleCount / vorbis.Channels, vorbis.Channels, vorbis.SampleRate, false);
        //         audioClip.SetData(f, 0);
        //
        //         return audioClip;
        //     }
        //
        //     if (path.EndsWith(".wav"))
        //     {
        //         WAV wav = new WAV(data);
        //         AudioClip audioClip;
        //         if (wav.ChannelCount == 2)
        //         {
        //             audioClip = AudioClip.Create("", wav.SampleCount, 2, wav.Frequency, false);
        //             audioClip.SetData(wav.StereoChannel, 0);
        //         }
        //         else
        //         {
        //             audioClip = AudioClip.Create("", wav.SampleCount, 1, wav.Frequency, false);
        //             audioClip.SetData(wav.LeftChannel, 0);
        //         }
        //
        //         return audioClip;
        //     }
        //
        //     return null;
        // }
        //
        // private AudioClip SaveAudioClipToCache(AudioClip audioClip, int id)
        // {
        //     previews[id] = audioClip;
        //     return audioClip;
        // }
        //
        // private AudioClip GetPreview(string keyword)
        // {
        //     string temp = _dataPath + "songs/" + keyword +
        //                   "/preview";
        //     string temp1 = _dataPath + "songs/" + keyword +
        //                    "/base";
        //     if (File.Exists(temp + ".ogg"))
        //     {
        //         return GetAudioClip(temp + ".ogg");
        //     }
        //
        //     if (File.Exists(temp + ".wav"))
        //     {
        //         return GetAudioClip(temp + ".wav");
        //     }
        //
        //     if (File.Exists(temp1 + ".ogg"))
        //     {
        //         return GetAudioClip(temp1 + ".ogg");
        //     }
        //
        //     if (File.Exists(temp1 + ".wav"))
        //     {
        //         return GetAudioClip(temp1 + ".wav");
        //     }
        //
        //     return null;
        // }
        
        private void RefreshSongNow()
        {
            songName.transform.parent.gameObject.SetActive(true);
            songCover.sprite = songDataNow.cover ? songDataNow.cover : Util.SpritePlaceholder;
            songName.text = songDataNow.songName;
            songArtist.text = songDataNow.songArtist;
            songBpm.text = "BPM " + songDataNow.bpm;
        }

        public SELECT_ORDER queue;

        private bool isQueueing = false;

        public void RefreshSongItem(bool move)
        {
            StartCoroutine(RefreshSongItemC(move));
        }

        private IEnumerator RefreshSongItemC(bool move)
        {
            if (move)
            {
                MoveToSelect();
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else
            {
                yield return null;
            }

            QueueBy(true);
        }

        public class SongData
        {
            [JsonProperty("keyword")] public string keyword;
            private string _songName;

            [JsonProperty("name")]
            public string songName
            {
                get => _songName;
                set => _songName = Util.FromBase64(value);
            }

            private string _songArtist;

            [JsonProperty("artist")]
            public string songArtist
            {
                get => _songArtist;
                set => _songArtist = Util.FromBase64(value);
            }

            [JsonProperty("bpm")] public string bpm;
            public int[] hards;

            public Sprite cover;

            [JsonProperty("preview")] public float preview;
        }

        public void QueueBy(int i)
        {
            queue = (SELECT_ORDER) i;
            currentSettings.SelectOrder = queue;
            QueueBy(false);
        }

        private void QueueBy(bool refreshCachePools)
        {
            if (isQueueing) return;
            StartCoroutine(QueueByC(queue, refreshCachePools));
        }

        private IEnumerator QueueByC(SELECT_ORDER i, bool refreshCachePools)
        {
            isQueueing = true;
            for (int j = 0; j < SongOrders.Length; j++)
            {
                if (j == (int) i) SongOrders[j].GetComponent<Image>().sprite = imageButtonPressed;
                else SongOrders[j].GetComponent<Image>().sprite = imageButton;
            }

            if (selectItems != null)
            {
                foreach (SelectItem selectItem in selectItems)
                {
                    Destroy(selectItem.gameObject);
                }
            }
            songName.transform.parent.gameObject.SetActive(false);

            yield return null;
            RemakeSongItem(i, refreshCachePools);

            isQueueing = false;
        }

        public void StartGame()
        {
            StartCoroutine(StartGameCoroutine());
        }

        public InputField debugScore;
        public void SetScore()
        {
            if (selectedDiff == null || debugScore.text.Trim().Equals("")) return;
            int songHard = (int)selectedDiff;
            if (!songDataNow.hards.Contains(songHard)) return;
            string md5 = Util.GetMd5OfChart(songDataNow.keyword, songHard);
            ResultData result = new ResultData
            {
                score = int.Parse(debugScore.text.Trim())
            };
            PlayerPrefs.SetString("SongScore_" + md5, JsonConvert.SerializeObject(result, Formatting.None));
            PlayerPrefs.Save();
            debugScore.text = "";
        }

        private IEnumerator StartGameCoroutine()
        {
            if (selectedDiff == null) yield break;
            int songHard = (int)selectedDiff;
            if (!songDataNow.hards.Contains(songHard)) yield break;

            CheckDataContainers.CleanSongDataContainer();
            CheckDataContainers.CleanResultDataContainer();
            GameObject go = new GameObject("SongDataContainer") { tag = "SongData" };
            SongDataContainer songDataContainer = go.AddComponent<SongDataContainer>();
            if (File.Exists(StaticResources.Instance.dataPath + "songs/" + songDataNow.keyword + "/" + songHard + ".ogg"))
            {
                using var uwr =
                    UnityWebRequestMultimedia.GetAudioClip(
                        "file://" + StaticResources.Instance.dataPath + "songs/" + songDataNow.keyword + "/" + songHard + ".ogg",
                        AudioType.OGGVORBIS);
                yield return uwr.SendWebRequest();
                if (uwr.isDone)
                {
                    songDataContainer.music = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }
            // else if (File.Exists(dataPath + "songs/" + songDataNow.keyword + "/" + songHard + ".wav"))
            // {
            //     using var uwr =
            //         UnityWebRequestMultimedia.GetAudioClip(
            //             "file://" + dataPath + "songs/" + songDataNow.keyword + "/" + songHard + ".wav",
            //             AudioType.WAV);
            //     yield return uwr.SendWebRequest();
            //     if (uwr.isDone)
            //     {
            //         songDataContainer.music = DownloadHandlerAudioClip.GetContent(uwr);
            //     }
            // }
            else if (File.Exists(StaticResources.Instance.dataPath + "songs/" + songDataNow.keyword + "/base.ogg"))
            {
                using var uwr =
                    UnityWebRequestMultimedia.GetAudioClip(
                        "file://" + StaticResources.Instance.dataPath + "songs/" + songDataNow.keyword + "/base.ogg",
                        AudioType.OGGVORBIS);
                yield return uwr.SendWebRequest();
                if (uwr.isDone)
                {
                    songDataContainer.music = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }
            // else if (File.Exists(dataPath + "songs/" + songDataNow.keyword + "/base.wav"))
            // {
            //     using var uwr =
            //         UnityWebRequestMultimedia.GetAudioClip(
            //             "file://" + dataPath + "songs/" + songDataNow.keyword + "/base.wav",
            //             AudioType.WAV);
            //     yield return uwr.SendWebRequest();
            //     if (uwr.isDone)
            //     {
            //         songDataContainer.music = DownloadHandlerAudioClip.GetContent(uwr);
            //     }
            // }
            else
            {
                NotificationBarManager.Instance.Show(LanguageManager.Instance
                    .GetText("select.error.music"));
                yield break;
            }

            if (!File.Exists(StaticResources.Instance.dataPath + "songs/" + songDataNow.keyword + "/" + songHard + ".txt"))
            {
                NotificationBarManager.Instance.Show(LanguageManager.Instance
                    .GetText("select.error.chart"));
                yield break;
            }

            songDataContainer.songData = songDataNow;
            songDataContainer.selectedDiff = songHard;
            songDataContainer.saveAudio = Input.GetKey(KeyCode.LeftShift) &&
                                          Application.platform is RuntimePlatform.WindowsEditor or RuntimePlatform
                                              .WindowsPlayer or RuntimePlatform.LinuxEditor or RuntimePlatform.LinuxPlayer;
            InputManager.TOUCH_MAX = 20;
            DontDestroyOnLoad(go);

            GlobalSettings.CurrentSettings = currentSettings;
            GlobalSettings.Save();
            SaveLastSelectedSong();
            FadeManager.Instance.LoadScene("game");
        }

        public void SetAuto(bool value)
        {
            currentSettings.IsAuto = value;
        }

        public void SetMirror(bool value)
        {
            currentSettings.IsMirror = value;
        }

        public void SetSkillCheck(bool value)
        {
            currentSettings.SkillCheckMode = value;
        }

        public void SetHard(bool value)
        {
            currentSettings.HardMode = value;
        }

        public void UpdateSongSpeed(int value)
        {
            currentSettings.SongSpeed = value;
        }

        public void UpdateHPBarType(int value)
        {
            currentSettings.HPBarType = value;
        }

        public void UpdateJudgeRange(int value)
        {
            currentSettings.NoteJudgeRange = value;
        }
    }
        
    public enum SELECT_ORDER
    {
        SONGLIST = 0,
        KEYWORD = 1,
        NAME = 2,
        ARTIST = 3
    }
}
