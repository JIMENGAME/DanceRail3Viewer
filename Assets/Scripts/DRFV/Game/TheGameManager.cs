using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DG.Tweening;
using DRFV.Dankai;
using DRFV.Data;
using DRFV.Enums;
using DRFV.Game.HPBars;
using DRFV.Game.SceneControl;
using DRFV.Game.Side;
using DRFV.Global;
using DRFV.Global.Managers;
using DRFV.Global.Utilities;
using DRFV.inokana;
using DRFV.Result;
using DRFV.Select;
using DRFV.Setting;
using DRFV.Story;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace DRFV.Game
{
    public class TheGameManager : MonoBehaviour
    {
        private byte[] HadouTestAPI =
        {
            104, 116, 116, 112, 58, 47, 47, 119, 119, 119, 46, 104, 97, 100, 111, 117, 108, 117, 99, 97, 114, 105, 111,
            46, 115, 104, 111, 112, 47, 68, 97, 110, 99, 101, 82, 97, 105, 108, 47, 68, 82, 51, 84, 101, 115, 116, 47
        };

        private byte[] HadouTestUA =
        {
            68, 97, 110, 99, 101, 82, 97, 105, 108, 51, 47, 49, 46, 54, 55, 32, 67, 70, 78, 101, 116, 119, 111, 114,
            107, 47, 49, 51, 51, 53, 46, 48, 46, 51, 32, 68, 97, 114, 119, 105, 110, 47, 50, 49, 46, 54, 46, 48
        };

        public GameSide gameSide = GameSide.DARK;
        public float[] comboWhite, comboRed, comboBlue;
        public Color[] comboColorLight;
        public Color[] comboColorDark;
        public SideChanger[] SideChangers;
        public static Random Random1;
        [SerializeField] public string SongKeyword = "bluemelody";
        [SerializeField] public int SongHard = 6;
        [SerializeField] string SongTitle = "Blue Melody", SongArtist = "波導Lucario";


        [SerializeField] public int NoteSpeed = 12;
        [SerializeField] public float RealNoteSpeed = 12;
        [SerializeField] float NoteOffset; //[SerializeField, Range(-200, 200)]
        [SerializeField] public bool GameAuto = true, GameMirror;

        [SerializeField, Range(0, 10)]
        public int GameEffectGaterLevel = 8, GameEffectParamEQLevel = 8, GameEffectTap = 6;

        [SerializeField] GameComboDisplay playerGameComboDisplay = GameComboDisplay.COMBO;

        [SerializeField] public float PJms = 30.0f, PFms = 60.0f, GDms = 100.0f;

        public GameObject BeforeGameBackground;

        private GameObject dankaiDataGameObject;
        public SongDataContainer songDataContainer;
        public DankaiDataContainer dankaiDataContainer;

        public GameObject autoPlayHint;
        public Text ProgramInfoTitle;

        public InputField inputJumpTo;

        public GameObject pauseUI;

        public GameSubJudgeDisplay gameSubJudgeDisplay;

        public TheLyricManager theLyricManager;

        public bool FCAPIndicator;

        public GameObject AegleseekerObject;

        public List<float> AccMSList = new();

        private bool videoPrepared;

        public float Score;
        public float LiLunZhi;
        public float MaxScore;
        public float Accuracy;
        public int Combo;
        public int MaxCombo;
        public int PerfectJ;
        public int Perfect;
        public int good;
        public int miss;
        public int fast;
        public int slow;
        private int PassedNotes;
        private int JudgedNotes => PerfectJ + Perfect + good + miss;

        private AudioClip originalBGM;
        private bool saveAudio;
        public bool DebugMode;
        public bool IsDankai;
        public bool ShowNoteId;

        public ProgressManager progressManager;

        private bool hasVideo;

        public PostProcess postProcess;

        //維持オブジェクト
        [SerializeField] // [SerializeField, HideInInspector]
        GameObject NotePrefab, HanteiPrefab;

        [SerializeField] // [SerializeField, HideInInspector]
        InputManager inputManager;

        float ReadyTime = 1990f;

        [SerializeField] // [SerializeField, HideInInspector]
        public BGMManager bgmManager;

        private AudioClip _acGameover;

        [SerializeField] // [SerializeField, HideInInspector] 
        public HPManager hpManager;

        [SerializeField] public SceneControlManager sceneControlManager;

        [SerializeField] // [SerializeField, HideInInspector] 
        MeshDrawer meshDrawer;

        [SerializeField] // [SerializeField, HideInInspector] 
        Sprite[] SpriteNoteLight, SpriteNotesDark;

        private Sprite[] SpriteNotes => gameSide switch
        {
            GameSide.DARK => SpriteNotesDark,
            GameSide.LIGHT => SpriteNoteLight,
            GameSide.COLORLESS => SpriteNoteLight,
            _ => SpriteNotesDark
        };

        [SerializeField] // [SerializeField, HideInInspector] 
        Sprite[] SpriteArrorLight, SpriteArrorDark;

        [SerializeField] // [SerializeField, HideInInspector] 
        GameObject[] prefabEffect;

        [SerializeField] // [SerializeField, HideInInspector] 
        GameObject notesUp, notesDown;

        [SerializeField] // [SerializeField, HideInInspector] 
        public Text textSongTitle, textSongArtist, textDif;

        [SerializeField] // [SerializeField, HideInInspector] 
        public Image sprSongImage;

        [SerializeField] // [SerializeField, HideInInspector] 
        Slider sliderMusicTime;

        [SerializeField] // [SerializeField, HideInInspector] 
        Text textScore, textMaxcombo, textPerfect, textPerfect2, textGood, textMiss;

        [SerializeField] // [SerializeField, HideInInspector] 
        GameObject[] objCombo;

        [SerializeField] // [SerializeField, HideInInspector] 
        Image[] imgJudgeBeam;

        [SerializeField] // [SerializeField, HideInInspector] 
        AnimationCurve EQCurve;

        List<float>
            EQList = new(),
            HPList = new(),
            LPList = new(),
            SteroList = new();

        float AudioMixerFreq = 1.0f,
            AudioMixerCenter = 1.0f,
            AudioMixerHiPass = 20.0f,
            AudioMixerLoPass = 10000.0f;

        List<float> AngList = new();

        float[] HgtList = new float[1000];

        // [HideInInspector] 
        public AnimationCurve HeightCurve;
        public AnimationCurve PositionCurve;

        [SerializeField] // [SerializeField, HideInInspector] 
        GameObject TheCamera;

        private AudioMixer audioMixer;

        [SerializeField] // [SerializeField, HideInInspector] 
        Image HPMask;

        public Transform AnimationGradesParent;

        [SerializeField] // [SerializeField, HideInInspector] 
        GameObject[] AnimationGrades;

        public float SkillDamage = 1.0f;


        // [HideInInspector]
        public AnimationCurve SCCurve;

        // [HideInInspector]
        public double Distance;
        int CurrentSCn;
        float CurrentSC = 1.0f;

        // [HideInInspector] 
        public bool isPause = true;
        public GameObject jumpTo;

        delegate void AudioClipReceiver(AudioClip result);

        public DRBFile drbfile;
        public int noteTotal;

        List<bool> isCreated = new List<bool>();
        int makenotestartReal, makenotestartFake;

        private float volumeScale = 0.5f;
        private Color? EndBgColor;

        public EtherStrike etherStrike;

        public bool storyMode;
        private string storyUnlock = "";

        public bool isComputerInStory;

        public BarType barType = BarType.DEFAULT;

        public bool hasCustomMover, hasCustomHeight;
        private float endTime;

        public bool hadouTest;

        private bool hasTextBeforeStart;

        public Text lyricText;

        private string customTierIdentifier = "Tier";
        private Color? customTierColor = null;
        private bool ratingPlus;

        private List<float> msDetailsList = new List<float>();

        public bool enableTestifyAnomaly;

        private float lastNoteTime;

        private SCORE_TYPE _scoreType;

        private void Awake()
        {
            if (!objCombo[0]) return;
            _topBarValue = objCombo[0].GetComponent<Text>();
            _topBarValueRed = objCombo[1].GetComponent<Text>();
            _topBarValueBlue = objCombo[2].GetComponent<Text>();
            _topBarLabel = objCombo[3].GetComponent<Text>();
        }

        void Start()
        {
            //メモリ解放
            Resources.UnloadUnusedAssets();
            audioMixer = StaticResources.Instance.audioMixer;
            VideoPlayer.targetTexture.Release();
            BeforeGameBackground.SetActive(true);

            SCUp = SCUpImage.DOFade(0, 0);
            SCDown = SCDownImage.DOFade(0, 0);

            speedLabelText = "更改谱面速度：";

            progressManager.Init(() =>
            {
                NotificationBarManager.Instance.Show("计时器精准度可能有问题，请与inokana取得联系");
                QuitDirectly();
            }, () =>
            {
                NotificationBarManager.Instance.Show("DspBuffer数值过小");
                QuitDirectly();
            }, GetPitch);
            VideoPlayer.errorReceived += (_, _) =>
            {
                background.SetActive(true);
                VideoPlayer.transform.parent.gameObject.SetActive(false);
                videoPrepared = true;
            };
            VideoPlayer.prepareCompleted += _ => videoPrepared = true;
            StartCoroutine(ApplySettings());
        }

        private float GetPitch()
        {
            return bgmManager.Pitch;
        }

        private IEnumerator ApplySettings()
        {
            dankaiDataGameObject = GameObject.FindWithTag("DankaiData");
            GameObject songDataObject = GameObject.FindWithTag("SongData");
            IsDankai = dankaiDataGameObject != null;
#if UNITY_EDITOR
            DebugMode = !IsDankai && songDataObject == null;
#endif
            currentSettings = GlobalSettings.CurrentSettings;
            if (DebugMode)
            {
                originalBGM = Resources.Load<AudioClip>("DEBUG/" + SongKeyword);
                if (sprSongImage) sprSongImage.sprite = Resources.Load<Sprite>("DEBUG/" + SongKeyword);
                VideoPlayer.playbackSpeed = bgmManager.Pitch;
                if (SongKeyword.Equals("hello") && SongHard == 13)
                {
                    isHard = true;
                }

                hasVideo = !String.IsNullOrEmpty(VideoPlayer.url) || VideoPlayer.clip != null;
            }
            else if (IsDankai || songDataObject != null)
            {
                if (IsDankai)
                {
                    dankaiDataContainer = dankaiDataGameObject.GetComponent<DankaiDataContainer>();
                    songDataContainer = dankaiDataContainer.songs[dankaiDataContainer.nowId];
                }
                else songDataContainer = songDataObject.GetComponent<SongDataContainer>();

                SongKeyword = songDataContainer.songData.keyword;
                SongHard = songDataContainer.selectedDiff;
                SongTitle = songDataContainer.songData.songName;
                SongArtist = songDataContainer.songData.songArtist;
                originalBGM = songDataContainer.music;
                NoteSpeed = currentSettings.NoteSpeed;
                NoteOffset = currentSettings.Offset;
                GameAuto = currentSettings.IsAuto;
                GameMirror = currentSettings.IsMirror;
                isHard = currentSettings.HardMode;
                skillcheck = currentSettings.SkillCheckMode;
                _scoreType = currentSettings.ScoreType;
                GameEffectParamEQLevel = currentSettings.GameEffectParamEQLevel;
                GameEffectGaterLevel = currentSettings.GameEffectGaterLevel;
                GameEffectTap = currentSettings.GameEffectTap;
                playerGameComboDisplay = currentSettings.ComboDisp;
                if (_scoreType != SCORE_TYPE.ARCAEA &&
                    playerGameComboDisplay == GameComboDisplay.LAGRANGE)
                    playerGameComboDisplay = GameComboDisplay.COMBO;
                gameSubJudgeDisplay = currentSettings.SmallJudgeDisp;
                FCAPIndicator = currentSettings.FCAPIndicator;
                saveAudio = songDataContainer.saveAudio;
                float songSpeed = Util.TransformSongSpeed(currentSettings.SongSpeed);
                VideoPlayer.playbackSpeed = bgmManager.Pitch = songSpeed;
                barType = (BarType)currentSettings.HPBarType;
                gameSide = (GameSide)currentSettings.GameSide;
                NoteJudgeRange aaa = GameUtil.GetNoteJudgeRange(currentSettings.NoteJudgeRange);
                RealNoteSpeed = currentSettings.enableSCFix ? NoteSpeed / songSpeed : NoteSpeed;
                PJms = aaa.PJ * songSpeed;
                PFms = aaa.P * songSpeed;
                GDms = aaa.G * songSpeed;

                if (SongKeyword.Equals("hello") && SongHard == 13)
                {
                    isHard = true;
                }

                if (sprSongImage)
                    sprSongImage.sprite =
                        songDataContainer.songData.cover ? songDataContainer.songData.cover : Util.SpritePlaceholder;
                if (barType == BarType.HARD)
                    currentSettings.HardMode = true;
                else if (barType == BarType.EASY)
                    currentSettings.HardMode = false;
                if (IsDankai)
                {
                    hasCustomMover = false;
                    hasCustomHeight = false;
                    isHard = true;
                    barType = BarType.DANKAI;
                    _scoreType = SCORE_TYPE.ORIGINAL;
                    if (playerGameComboDisplay == GameComboDisplay.LAGRANGE)
                        playerGameComboDisplay = GameComboDisplay.COMBO;
#if !UNITY_EDITOR
                    GameAuto = false;
#endif
                    GameMirror = false;
                    skillcheck = true;
                    // VideoPlayer.playbackSpeed = bgmManager.Pitch = 1.0f;
                    // enableJudgeRangeFix = false;
                    // RealNoteSpeed = NoteSpeed;
                }

                if (songDataContainer.GetContainerType() == SongDataContainerType.STORY)
                {
                    StoryChallengeContainer storyChallengeContainer = (StoryChallengeContainer)songDataContainer;
                    storyUnlock = storyChallengeContainer.unlock;
                    isComputerInStory = storyChallengeContainer.isComputer;
                    storyMode = true;
                    hasCustomMover = storyChallengeContainer.hasCustomMover;
                    hasCustomHeight = storyChallengeContainer.hasCustomHeight;
                    hasTextBeforeStart = storyChallengeContainer.hasTextBeforeStart;
                    customTierIdentifier = storyChallengeContainer.tierIdentifier;
                    customTierColor = storyChallengeContainer.customTierColor;
                    ratingPlus = storyChallengeContainer.ratingPlus;
                    GameAuto |= isComputerInStory;
                    RealNoteSpeed = NoteSpeed = currentSettings.NoteSpeed;
                    VideoPlayer.playbackSpeed = bgmManager.Pitch = storyChallengeContainer.songSpeed;
                    aaa = GameUtil.GetNoteJudgeRange(storyChallengeContainer.NoteJudgeRange);
                    PJms = aaa.PJ * storyChallengeContainer.songSpeed;
                    PFms = aaa.P * storyChallengeContainer.songSpeed;
                    GDms = aaa.G * storyChallengeContainer.songSpeed;

                    barType = BarType.DEFAULT;
                    GameMirror = false;
                    isHard = false;
                    skillcheck = false;
                    gameSide = storyChallengeContainer.gameSide;
                }

                if (songDataContainer.GetContainerType() == SongDataContainerType.HADOU_TEST)
                {
                    hadouTest = true;
                    if (sprSongImage) sprSongImage.sprite = null;
                    using UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(
                        Encoding.UTF8.GetString(HadouTestAPI) + $"{SongKeyword}.ogg", AudioType.OGGVORBIS);
                    uwr.SetRequestHeader("User-Agent", Encoding.UTF8.GetString(HadouTestUA));
                    yield return uwr.SendWebRequest();
                    if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                        uwr.result == UnityWebRequest.Result.ProtocolError)
                    {
                        NotificationBarManager.Instance.Show("错误: " + uwr.responseCode + " : " + uwr.error + "\n");
                        FadeManager.Instance.Back();
                    }
                    else if (uwr.isDone)
                    {
                        originalBGM = ((DownloadHandlerAudioClip)uwr.downloadHandler).audioClip;
                    }

                    NoteJudgeRange aaaa = GameUtil.GetNoteJudgeRange(-1);
                    PJms = aaaa.PJ;
                    PFms = aaaa.P;
                    GDms = aaaa.G;
                }
            }
            else
            {
                NotificationBarManager.Instance.Show("你怎么进来的");
                QuitGame();
                yield break;
            }

            if (originalBGM.channels != 2) originalBGM = originalBGM.MonoToStereo();

            foreach (var sideChanger in SideChangers)
            {
                sideChanger.SetSide(gameSide);
            }

            comboWhite = new float[3];
            comboRed = new float[3];
            comboBlue = new float[3];
            Color[] targetColors = gameSide switch
            {
                GameSide.LIGHT => comboColorLight,
                GameSide.DARK => comboColorDark,
                GameSide.COLORLESS => comboColorLight,
                _ => comboColorDark
            };
            comboWhite[0] = targetColors[0].r;
            comboWhite[1] = targetColors[0].g;
            comboWhite[2] = targetColors[0].b;
            comboRed[0] = targetColors[1].r;
            comboRed[1] = targetColors[1].g;
            comboRed[2] = targetColors[1].b;
            comboBlue[0] = targetColors[2].r;
            comboBlue[1] = targetColors[2].g;
            comboBlue[2] = targetColors[2].b;

            StartCoroutine(StartA());
        }

        IEnumerator StartA()
        {
            ReadSettings();
            AnimationGrades = new GameObject[AnimationGradesParent.childCount];
            for (int i = 0; i < AnimationGradesParent.childCount; i++)
            {
                AnimationGrades[i] = AnimationGradesParent.GetChild(i).gameObject;
            }

            // Auto提示
            if (GameAuto)
            {
                // int hintType = PlayerPrefs.GetInt("AutoplayHint", 0);
                if (GlobalSettings.CurrentSettings.AutoplayHint == 0)
                {
                    autoPlayHint.SetActive(true);
                }
                else
                {
                    ProgramInfoTitle.text = "DR3 AUTOPLAY";
                }
            }

            hpManager.manager = this;
            hpManager.Init(SongKeyword == "hellowinner" && SongHard == 13 && !hadouTest
                ? new HpBarHelloWinner()
                : barType switch
                {
                    BarType.DEFAULT => new HPBarDefault(),
                    BarType.EASY => new HPBarEasy(),
                    BarType.HARD => new HPBarHard(hpManager),
                    BarType.DANKAI => new HPBarDefault(dankaiDataContainer.hpMax, dankaiDataContainer.hpNow),
                    _ => new HPBarDefault()
                });

            //曲データ表示
            if (textSongTitle)
            {
                textSongTitle.text = SongTitle;
                // if (textSongTitle.preferredWidth > 320.0f)
                // {
                //     textSongTitle.rectTransform.localScale = new Vector2(320.0f / textSongTitle.preferredWidth, 1.0f);
                // }
            }

            if (textSongArtist)
            {
                textSongArtist.text = SongArtist;
                // if (textSongArtist.preferredWidth > 320.0f)
                // {
                //     textSongArtist.rectTransform.localScale = new Vector2(320.0f / textSongArtist.preferredWidth, 1.0f);
                // }
            }

            if (textDif)
            {
                textDif.text = customTierIdentifier + " " + (SongHard == 0 ? "?" : SongHard) + (ratingPlus ? "+" : "");
                textDif.color = customTierColor ?? Util.GetTierColor(SongHard);
                textDif.gameObject.GetComponent<Outline>().enabled = SongHard is > 20 or < 0;
            }


            if (!DebugMode && !storyMode && !hadouTest && !IsDankai) hasVideo = ReadVideo();
            background.SetActive(!hasVideo);
            VideoPlayer.transform.parent.gameObject.SetActive(hasVideo);
            if (hasVideo)
            {
                VideoPlayer.Prepare();
                yield return new WaitUntil(() => videoPrepared);
                RectTransform rectTransform = VideoPlayer.GetComponent<RectTransform>();
                int width = VideoPlayer.targetTexture.width = (int)VideoPlayer.width;
                int height = VideoPlayer.targetTexture.height = (int)VideoPlayer.height;
                rectTransform.sizeDelta = Screen.width * 1f / Screen.height < width * 1f / height
                    ? new Vector2(Screen.width, height * 1f * Screen.width / width)
                    : new Vector2(width * 1f * Screen.height / height, Screen.height);
                VideoPlayer.time = VideoPlayer.frame = 0;
            }

            string s;
            if (DebugMode)
            {
                s = Resources.Load<TextAsset>("DEBUG/" + SongKeyword + "." + SongHard).text.Trim().Replace("\r", "")
                    .Replace("\t", "");
            }
            else if (hadouTest)
            {
                using UnityWebRequest uwr = UnityWebRequest.Get(Encoding.UTF8.GetString(HadouTestAPI) +
                                                                $"{SongKeyword}.{SongHard}.txt");
                uwr.downloadHandler = new DownloadHandlerBuffer();
                uwr.SetRequestHeader("User-Agent", Encoding.UTF8.GetString(HadouTestUA));
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    NotificationBarManager.Instance.Show("谱找不到");
                    FadeManager.Instance.Back();
                    yield break;
                }

                s = Encoding.UTF8.GetString(uwr.downloadHandler.data).Replace("\r", "").Replace("\t", "");
            }
            else if (storyMode)
            {
                s = ExternalResources.LoadText("STORY/SONGS/" + SongKeyword + "." + SongHard).text.Trim()
                    .Replace("\r", "")
                    .Replace("\t", "");
            }
            else if (IsDankai)
            {
                s = Resources.Load<TextAsset>("DANKAI/SONGS/" + SongKeyword + "." + SongHard).text.Trim()
                    .Replace("\r", "")
                    .Replace("\t", "");
            }
            else
            {
                string path = StaticResources.Instance.dataPath + "songs/" + SongKeyword +
                              "/" + SongHard +
                              ".txt";
                using StreamReader streamReader = new StreamReader(path, Encoding.UTF8);
                string line;
                List<string> list = new List<string>();
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Trim() == "") continue;
                    list.Add(line.Trim().Replace("\r", "").Replace("\t", ""));
                }

                s = String.Join("\n", list.ToArray());
            }

            try
            {
                drbfile = DRBFile.Parse(s);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                NotificationBarManager.Instance.Show($"错误：读取谱面时出错");
                GlobalSettings.CurrentSettings = currentSettings;
                GlobalSettings.Save();
                FadeManager.Instance.Back();
                yield break;
            }

            md5 = drbfile.GetMD5();
            drbfile.notes.Sort((a, b) => Mathf.RoundToInt(a.time * 10000.0f - b.time * 10000.0f));
            drbfile.fakeNotes.Sort((a, b) => Mathf.RoundToInt(a.time * 10000.0f - b.time * 10000.0f));

            if (skillcheck)
            {
                drbfile.scs.Clear();
                DRBFile.SCS sc = new DRBFile.SCS
                {
                    sc = 1,
                    sci = 0
                };
                drbfile.scs.Add(sc);
            }

            foreach (NoteData noteData in drbfile.notes)
            {
                if (skillcheck)
                {
                    noteData.nsc = NoteData.NoteSC.GetCommonNSC();
                    noteData.mode = NoteAppearMode.None;
                }

                if (GameMirror)
                {
                    noteData.pos = 16 - noteData.pos - noteData.width;
                    if (noteData.kind == NoteKind.FLICK_LEFT ||
                        noteData.kind == NoteKind.FLICK_RIGHT)
                    {
                        noteData.kind = noteData.kind == NoteKind.FLICK_LEFT
                            ? NoteKind.FLICK_RIGHT
                            : NoteKind.FLICK_LEFT;
                    }
                }
            }

            foreach (NoteData noteData in drbfile.fakeNotes)
            {
                if (skillcheck)
                {
                    noteData.nsc = NoteData.NoteSC.GetCommonNSC();
                    noteData.mode = NoteAppearMode.None;
                }

                if (GameMirror)
                {
                    noteData.pos = 16 - noteData.pos - noteData.width;
                    if (noteData.kind == NoteKind.FLICK_LEFT ||
                        noteData.kind == NoteKind.FLICK_RIGHT)
                    {
                        noteData.kind = noteData.kind == NoteKind.FLICK_LEFT
                            ? NoteKind.FLICK_RIGHT
                            : NoteKind.FLICK_LEFT;
                    }
                }
            }

            // 特判
            if (SongKeyword.Equals("hello") && SongHard == 13 && !hadouTest)
            {
                drbfile.notes.Clear();
                drbfile.fakeNotes.Clear();
                drbfile.noteWeightCount = 0;
                GenerateHelloWinnerNotes();
            }

            drbfile.GenerateAttributesOnPlay(SongHard);

            // if (A.Instance) A.Instance.Init(this);

            //SC
            SCCurve = drbfile.SCCurve;
            noteTotal = drbfile.notes.Count;

            //SCORE初期化
            SCORE_INIT();

            if (DebugMode && SongKeyword.Equals("aegleseeker"))
            {
                RegisterAegleseekerAnomaly();
            }


            if (sceneControlManager && !hadouTest)
            {
                yield return sceneControlManager.Init();
            }

            if (theLyricManager && !hadouTest)
            {
                yield return theLyricManager.Init();
            }

            if (etherStrike && !hadouTest)
            {
                etherStrike.Init(this);
            }

            if ((storyMode && SongKeyword == "testify" || enableTestifyAnomaly) && postProcess)
            {
                postProcess.Init(progressManager, GenerateTestifyAnomaly());
            }

            _acGameover = Resources.Load<AudioClip>("SE/gameover");
            yield return ProcessMusic();
            StartB();
        }

        private IEnumerator SaveNoteFX()
        {
            SavWav.Save(StaticResources.Instance.dataPath + "songs/" + SongKeyword +
                        "/" + SongHard +
                        ".wav", bgmManager.NoteFxClip);
            yield break;
        }

        private void StartB()
        {
            if (saveAudio && !DebugMode)
            {
                StartCoroutine(SaveNoteFX());
            }

            SetTimeToEnd(drbfile.LastNoteTime);


            //复杂整理03：按节点计算front和back的判定范围

            for (int i = 0; i < drbfile.notes.Count + drbfile.fakeNotes.Count; i++)
            {
                isCreated.Add(false);
            }


            bool hasMover;
            string path = "";
            //平移曲线
            if (!DebugMode && ((!storyMode && File.Exists(path =
                    StaticResources.Instance.dataPath + "songs/" + SongKeyword +
                    "/custom_mover." + SongHard +
                    ".txt")) || (storyMode && hasCustomMover)))
            {
                var drbFile =
                    DRBFile.Parse(
                        storyMode
                            ? ExternalResources.LoadText($"STORY/SONGS/custom_mover.{SongKeyword}.{SongHard}").text
                            : File.ReadAllText(path));
                drbFile.GenerateAttributesOnPlay(SongHard);

                (PositionCurve, hasMover) = GeneratePositionCurve(drbFile);
            }
            else
            {
                (PositionCurve, hasMover) = GeneratePositionCurve(drbfile);
            }

            if (!DebugMode && ((!storyMode && File.Exists(path =
                    StaticResources.Instance.dataPath + "songs/" + SongKeyword +
                    "/custom_height." + SongHard +
                    ".txt")) || (storyMode && hasCustomHeight)))
            {
                var drbFile =
                    DRBFile.Parse(
                        storyMode
                            ? ExternalResources.LoadText($"STORY/SONGS/custom_height.{SongKeyword}.{SongHard}").text
                            : File.ReadAllText(path));
                drbFile.GenerateAttributesOnPlay(SongHard);

                GenerateHeightCurve(drbFile);
            }
            else if (!hasMover) GenerateHeightCurve(drbfile);

            //生成高度曲线
            for (int i = 0; i < 1000; i++)
            {
                HeightCurve.AddKey(i, HgtList[i]);
            }

            //仅保护TAP的判定范围
            foreach (var note in drbfile.notes.Where(note => note.IsTap()))
            {
                foreach (var note1 in drbfile.notes)
                {
                    {
                        if (isCovering(note, note1) &&
                            Mathf.Abs(note1.ms - note.ms) < GDms * 2.0f)
                        {
                            if (note1.ms < note.ms)
                            {
                                note.isJudgeTimeRangeConflicted = true;
                            }
                        }
                    }
                }
            }

            HPMask.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            HPMask.gameObject.SetActive(false);

            //背景


            //Score Combo表示
            if (playerGameComboDisplay == GameComboDisplay.COMBO)
            {
                _topBarValue.color = _topBarLabel.color =
                    new Color(comboWhite[0], comboWhite[1], comboWhite[2], 0.0f);
                _topBarValueRed.color = new Color(comboRed[0], comboRed[1], comboRed[2], 0.0f);
                _topBarValueBlue.color = new Color(comboBlue[0], comboBlue[1], comboBlue[2], 0.0f);
            }
            else if (FCAPIndicator)
            {
                _topBarLabel.color =
                    _topBarValue.color = new Color(1.0f, 0.94f, 0.196f, 1.0f);
                _topBarValueRed.color = new Color(comboRed[0], comboRed[1], comboRed[2], 0.0f);
                _topBarValueBlue.color = new Color(comboBlue[0], comboBlue[1], comboBlue[2], 0.0f);
            }
            else
            {
                _topBarValue.color = _topBarLabel.color =
                    new Color(comboWhite[0], comboWhite[1], comboWhite[2], 1.0f);
                _topBarValueRed.color = new Color(comboRed[0], comboRed[1], comboRed[2], 1.0f);
                _topBarValueBlue.color = new Color(comboBlue[0], comboBlue[1], comboBlue[2], 1.0f);
            }

            switch (playerGameComboDisplay)
            {
                case GameComboDisplay.NONE:
                    _topBarValue.text = "";
                    _topBarValueRed.text = "";
                    _topBarValueBlue.text = "";
                    _topBarLabel.text = "";
                    break;
                case GameComboDisplay.COMBO:
                    _topBarValue.text = "";
                    _topBarValueRed.text = "";
                    _topBarValueBlue.text = "";
                    _topBarLabel.text = "COMBO";
                    break;
                case GameComboDisplay.SCORE:
                    _topBarValue.text = "0";
                    _topBarValueRed.text = "0";
                    _topBarValueBlue.text = "0";
                    _topBarLabel.text = "SCORE";
                    break;
                case GameComboDisplay.MSCORE:
                    string score = GameUtil.ParseScore(Mathf.RoundToInt(MaxScore));
                    _topBarValue.text = score;
                    _topBarValueRed.text = score;
                    _topBarValueBlue.text = score;
                    _topBarLabel.text = "- SCORE";
                    break;
                case GameComboDisplay.ACCURACY:
                    _topBarValue.text = "0.00%";
                    _topBarValueRed.text = "0.00%";
                    _topBarValueBlue.text = "0.00%";
                    _topBarLabel.text = "ACCURACY";
                    break;
                case GameComboDisplay.LAGRANGE:
                    _topBarValue.text = "PM  +000,000";
                    _topBarValueRed.text = "PM  +000,000";
                    _topBarValueBlue.text = "PM  +000,000";
                    _topBarLabel.text = "PACE";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            inited = true;

            OBSManager.Instance?.StartRecording();
            progressManager.AddDelay(NoteOffset / 1000f);
            progressManager.AddDelay(0.1f);
            progressManager.AddStartDelay(ReadyTime / 1000f);
            yield return BeforeGameAnimation();
            if (hasTextBeforeStart) yield return ShowTextBeforeStart();
            progressManager.StartTiming();
            bgmManager.PlayScheduled(AudioSettings.dspTime + ReadyTime / 1000f);
            isPause = false;
            yield return new WaitForSecondsRealtime(ReadyTime / 1000f);
            if (hasVideo) VideoPlayer.Play();

            pauseable = true;
            yield return null;
        }

        IEnumerator ShowTextBeforeStart()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            float[] _times;
            string[] _lyrics;
            TextAsset textAsset = ExternalResources.LoadText($"STORY/SONGS/text_before_start.{SongKeyword}.{SongHard}");
            if (!textAsset) yield break;
            try
            {
                JObject main = JObject.Parse(textAsset.text);
                JArray jArray = main["texts"].ToObject<JArray>();
                Dictionary<string, string> timeLyricPairs = new Dictionary<string, string>();
                float timeToEnd = 0;
                foreach (JToken jToken in jArray)
                {
                    JObject jObject = jToken.ToObject<JObject>();
                    if (jObject.ContainsKey("end"))
                    {
                        timeToEnd = jObject["time"].ToObject<float>();
                    }
                    else if (!timeLyricPairs.ContainsKey(jObject["time"].ToObject<float>() + ""))
                        timeLyricPairs.Add(jObject["time"].ToObject<float>() + "", jObject["text"].ToObject<string>());
                }

                timeLyricPairs = timeLyricPairs.OrderBy(a => float.Parse(a.Key)).ToDictionary(a => a.Key, b => b.Value);
                _times = new float[timeLyricPairs.Count + 1];
                _lyrics = new string[timeLyricPairs.Count];
                int i = 0;
                foreach (KeyValuePair<string, string> timeLyricPair in timeLyricPairs)
                {
                    _times[i] = (i == 0 ? 0 : _times[i - 1]) + float.Parse(timeLyricPair.Key);
                    _lyrics[i] = timeLyricPair.Value;
                    i++;
                }

                _times[^1] = timeToEnd;

                lyricText.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                yield break;
            }

            float offset = 1000f;
            if (stopwatch.ElapsedMilliseconds - offset < 0)
                yield return new WaitWhile(() => stopwatch.ElapsedMilliseconds - offset < 0);
            for (int i = 0; i < _lyrics.Length; i++)
            {
                var i1 = i;
                yield return new WaitWhile(() => stopwatch.ElapsedMilliseconds - offset < _times[i1] * 1000f);
                lyricText.text = _lyrics[i];
            }

            yield return new WaitWhile(() => stopwatch.ElapsedMilliseconds - offset < _times[^1] * 1000f);
            lyricText.text = "";
        }

        IEnumerator BeforeGameAnimation()
        {
            yield return new WaitForSeconds(0.5f);
            Image coverFrame = BeforeGameBackground.transform.Find("CoverFrame").GetComponent<Image>(); // 0 420
            Image cover = coverFrame.transform.Find("Cover").GetComponent<Image>();
            RectTransform panel = BeforeGameBackground.transform.Find("Panel").GetComponent<RectTransform>(); // 0 -300
            Text Name = panel.Find("Name").GetComponent<Text>();
            Text Artist = panel.Find("Artist").GetComponent<Text>();
            Name.text = textSongTitle.text;
            Artist.text = textSongArtist.text;
            if (!drbfile.ndname.Equals(""))
            {
                GameObject NDText = coverFrame.transform.Find("NDText").gameObject;
                Text ND = coverFrame.transform.Find("ND").GetComponent<Text>();
                ND.text = drbfile.ndname;
                NDText.SetActive(true);
                ND.gameObject.SetActive(true);
            }

            if (songDataContainer == null)
            {
                if (sprSongImage.sprite)
                {
                    coverFrame.color = Util.GetAvgColor(sprSongImage.sprite);
                    cover.sprite = sprSongImage.sprite;
                }
            }
            else if (songDataContainer.songData.cover != null)
            {
                coverFrame.color =
                    EndBgColor ?? Util.GetAvgColor(songDataContainer.songData.cover);
                cover.sprite = songDataContainer.songData.cover;
            }
            else
            {
                coverFrame.color = Util.SpritePlaceholderBGColor;
                cover.sprite = Util.SpritePlaceholder;
            }

            coverFrame.rectTransform.DOAnchorPosY(-200f, 1f).SetEase(Ease.OutSine);
            panel.DOAnchorPosY(100f, 1f).SetEase(Ease.OutSine);
            yield return new WaitForSeconds(2.25f);
            coverFrame.rectTransform.DOAnchorPosY(420f, 1f).SetEase(Ease.InSine);
            panel.DOAnchorPosY(-300f, 1f).SetEase(Ease.InSine);
            BeforeGameBackground.GetComponent<Image>().DOFade(0, 1f).SetEase(Ease.InSine);
            yield return new WaitForSeconds(1f);
            BeforeGameBackground.SetActive(false);
        }

        public bool skillcheck;

        private void ReadSettings()
        {
            if (DebugMode || storyMode) return;
            string path = StaticResources.Instance.dataPath + "songs/" +
                          SongKeyword + "/settings.";
            if (File.Exists(path + "json"))
            {
                path += "json";
            }
            else if (File.Exists(path + SongHard + ".json"))
            {
                path += SongHard + ".json";
            }
            else return;

            JObject jObject = Util.ReadJson(path);
            if (jObject.ContainsKey("volume_scale"))
            {
                volumeScale = jObject["volume_scale"].ToObject<float>();
            }

            if (jObject.ContainsKey("end_bg_color"))
            {
                EndBgColor = Util.HexToColor(jObject["end_bg_color"].ToString());
            }
        }

        bool ReadVideo()
        {
            if (File.Exists(StaticResources.Instance.dataPath + "songs/" + SongKeyword + "/" + SongHard +
                            ".mp4"))
            {
                VideoPlayer.source = VideoSource.Url;
                VideoPlayer.url = StaticResources.Instance.dataPath + "songs/" + SongKeyword +
                                  "/" + SongHard + ".mp4";
                return true;
            }

            if (File.Exists(StaticResources.Instance.dataPath + "songs/" + SongKeyword + "/video.mp4"))
            {
                VideoPlayer.source = VideoSource.Url;
                VideoPlayer.url = StaticResources.Instance.dataPath + "songs/" + SongKeyword +
                                  "/video.mp4";
                return true;
            }

            return false;
        }

        public string md5;

        private IEnumerator ProcessMusic()
        {
            AudioClip _acHit = null,
                _acExHit = null,
                _acSlide = null,
                _acHold = null,
                _acFlick = null,
                _acFreeFlick = null,
                _acLong = null;
            bool disableOverlappingCheck = false;
            //获取效果音
            if (!DebugMode && !storyMode && !string.IsNullOrEmpty(currentSettings.selectedNoteSFX))
            {
                string songFolder = StaticResources.Instance.dataPath + "settings/note_sfx/" +
                                    currentSettings.selectedNoteSFX + "/";
                int defaultFrequency = 44100;
                if (File.Exists(songFolder + "settings.json"))
                {
                    JObject jObject = Util.ReadJson(songFolder + "settings.json");
                    if (jObject.ContainsKey("disable_overlapping_check"))
                    {
                        disableOverlappingCheck = jObject["disable_overlapping_check"].ToObject<bool>();
                    }

                    if (jObject.ContainsKey("default_frequency"))
                    {
                        defaultFrequency = jObject["default_frequency"].ToObject<int>();
                    }
                }

                string freq = originalBGM.frequency == defaultFrequency ? "" : "_" + originalBGM.frequency;

                yield return GetAudioClip("hit", result => _acHit = result);
                yield return GetAudioClip("exhit", result => _acExHit = result);
                yield return GetAudioClip("slide", result => _acSlide = result);
                yield return GetAudioClip("hold", result => _acHold = result);
                yield return GetAudioClip("flick", result => _acFlick = result);
                yield return GetAudioClip("free_flick", result => _acFreeFlick = result);
                yield return GetAudioClip("long", result => _acLong = result);

                IEnumerator GetAudioClip(string id, AudioClipReceiver audioClipReceiver)
                {
                    string tmpPath;
                    AudioType tmpAudioType;

                    (tmpPath, tmpAudioType) = GetFilePath(id);

                    if (!string.IsNullOrEmpty(tmpPath))
                    {
                        using var uwr =
                            UnityWebRequestMultimedia.GetAudioClip("file://" + tmpPath, tmpAudioType);
                        yield return uwr.SendWebRequest();
                        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);
                        if (uwr.result == UnityWebRequest.Result.Success)
                        {
                            audioClipReceiver.Invoke(audioClip.frequency == originalBGM.frequency
                                ? audioClip
                                : AudioFrequencyConverter.ConvertAudioClip(audioClip, originalBGM.frequency, id));
                            yield break;
                        }
                    }

                    audioClipReceiver.Invoke(null);
                }


                (string, AudioType) GetFilePath(string id)
                {
                    string path;
                    if (File.Exists(path = songFolder + $"{id}{freq}.ogg")) return (path, AudioType.OGGVORBIS);
                    if (File.Exists(path = songFolder + $"{id}.ogg")) return (path, AudioType.OGGVORBIS);
                    if (File.Exists(path = songFolder + $"{id}{freq}.wav")) return (path, AudioType.WAV);
                    if (File.Exists(path = songFolder + $"{id}.wav")) return (path, AudioType.WAV);
                    return ("", AudioType.UNKNOWN);
                }
            }

            if (_acHit == null)
            {
                _acHit = GetAudioClipFromRawWav("SE/hit", originalBGM.frequency, "hit");
                // _acHit = Resources.Load<AudioClip>("SE/hit");
            }

            if (_acFlick == null)
            {
                _acFlick = GetAudioClipFromRawWav("SE/flick", originalBGM.frequency, "flick");
                // _acFlick = Resources.Load<AudioClip>("SE/flick");
            }

            if (_acHit.channels != 2) _acHit = _acHit.MonoToStereo();
            if (_acFlick.channels != 2) _acFlick = _acFlick.MonoToStereo();

            if (_acExHit == null)
            {
                _acExHit = _acHit;
            }
            else if (_acExHit.channels != 2) _acExHit = _acExHit.MonoToStereo();

            if (_acSlide == null)
            {
                _acSlide = _acHit;
            }
            else if (_acSlide.channels != 2) _acSlide = _acSlide.MonoToStereo();

            if (_acHold == null)
            {
                _acHold = _acSlide;
            }
            else if (_acHold.channels != 2) _acHold = _acHold.MonoToStereo();

            if (_acFreeFlick == null)
            {
                _acFreeFlick = _acFlick;
            }
            else if (_acFreeFlick.channels != 2) _acFreeFlick = _acFreeFlick.MonoToStereo();

            if (_acLong != null && _acLong.channels != 2) _acLong = _acLong.MonoToStereo();


            //写入效果音A
            int bgmSamples = originalBGM.samples * originalBGM.channels;
            float[] f_hit = new float[_acHit.samples * _acHit.channels],
                f_exHit = new float[_acExHit.samples * _acExHit.channels],
                f_slide = new float[_acSlide.samples * _acSlide.channels],
                f_hold = new float[_acHold.samples * _acHold.channels],
                f_flick = new float[_acFlick.samples * _acFlick.channels],
                f_freeFlick = new float[_acFreeFlick.samples * _acFreeFlick.channels],
                f_long = new float[_acLong != null ? _acLong.samples * _acLong.channels : 0],
                f_song = new float[bgmSamples],
                f_fx = new float[bgmSamples];
            originalBGM.GetData(f_song, 0);

            _acHit.GetData(f_hit, 0);
            _acExHit.GetData(f_exHit, 0);
            _acSlide.GetData(f_slide, 0);
            _acHold.GetData(f_hold, 0);
            _acFlick.GetData(f_flick, 0);
            _acFreeFlick.GetData(f_freeFlick, 0);

            Array.Fill(f_fx, 0f);

            if (_acLong != null) _acLong.GetData(f_long, 0);

            //音量减半
            for (int i = 0; i < f_song.Length; i++)
            {
                f_song[i] *= volumeScale;
            }

            List<int> list_hit = new(),
                list_exHit = new(),
                list_slide = new(),
                list_hold = new(),
                list_flick = new(),
                list_freeFlick = new();

            int finalSamplesCount = originalBGM.samples +
                                    (int)MathF.Max(_acHit.samples, Mathf.Max(_acSlide.samples, _acFlick.samples));

            AudioClip main = AudioClip.Create("I'm Pepoyo's dog", finalSamplesCount, originalBGM.channels,
                originalBGM.frequency, false);
            main.SetData(f_song, 0);
            bgmManager.MainClip = main;

            if (GameEffectTap >= 1)
            {
                foreach (var noteData in drbfile.notes)
                {
                    ProcessTapEffect(noteData);
                    if (noteData.IsTail() && noteData.isLast && _acLong != null)
                    {
                        NoteData finalParent = drbfile.notes[noteData.parent];
                        while (finalParent.IsTail() && finalParent.parent != finalParent.realId)
                        {
                            finalParent = drbfile.notes[finalParent.parent];
                        }

                        int end = (int)(bgmSamples *
                                        (noteData.ms / 1000.0f / originalBGM.length));
                        int start = (int)(bgmSamples *
                                          (finalParent.ms / 1000.0f / originalBGM.length));

                        for (int c = start; c < end; c++)
                        {
                            f_fx[c] += f_long[c % f_long.Length] * 0.5f * ((GameEffectTap + 3) / 10.0f);
                        }
                    }
                }

                void ProcessTapEffect(NoteData noteData)
                {
                    //写入Tap音
                    if (noteData.kind == NoteKind.TAP || noteData.kind == NoteKind.ExTAP && _acExHit == _acHit ||
                        noteData.IsSlideSound() && _acSlide == _acHit || noteData.IsHold() && _acHold == _acHit)
                    {
                        WriteSamples(list_hit, f_hit, GameEffectTap);
                    }
                    else
                    {
                        if (noteData.kind == NoteKind.ExTAP)
                        {
                            WriteSamples(list_exHit, f_exHit, GameEffectTap);
                        }

                        if (noteData.IsSlideSound())
                        {
                            WriteSamples(list_slide, f_slide, GameEffectTap);
                        }

                        if (noteData.IsHold())
                        {
                            WriteSamples(list_hold, f_hold, GameEffectTap);
                        }
                    }

                    //写入flick音
                    if (noteData.IsFlick() || noteData.kind == NoteKind.FLICK && _acFreeFlick == _acFlick)
                    {
                        WriteSamples(list_flick, f_flick, GameEffectTap);
                    }
                    else
                    {
                        if (noteData.kind == NoteKind.FLICK)
                        {
                            WriteSamples(list_freeFlick, f_freeFlick, GameEffectTap);
                        }
                    }

                    void WriteSamples(List<int> checkOverlapping, float[] se, int volume)
                    {
                        int start = (int)(bgmSamples *
                                          (noteData.ms / 1000.0f / originalBGM.length));

                        if (disableOverlappingCheck || !checkOverlapping.Contains(start))
                        {
                            checkOverlapping.Add(start);
                            for (int c = 0; c < se.Length; c++)
                            {
                                if (start + c < f_song.Length)
                                    f_fx[start + c] += se[c] * 0.5f * ((volume + 3) / 10.0f);
                            }
                        }
                    }
                }
            }
            
            //写入gater音
            if (GameEffectGaterLevel >= 1)
            {
                foreach (var noteData in drbfile.notes)
                {
                    if (noteData.IsBitCrash())
                    {
                        int end = (int)(bgmSamples *
                                        (noteData.ms / 1000.0f / originalBGM.length));
                        int start = (int)(bgmSamples *
                                          (drbfile.notes[noteData.parent].ms / 1000.0f / originalBGM.length));

                        if (end < bgmSamples)
                        {
                            for (int c = (end - start) / 2 + start; c < end; c++)
                            {
                                f_song[c] *= (10.0f - GameEffectGaterLevel) / 10.0f;
                                f_fx[c] *= (10.0f - GameEffectGaterLevel) / 10.0f;
                            }
                        }
                    }
                }
            }

            AudioClip noteFx = AudioClip.Create("And you?", finalSamplesCount, originalBGM.channels,
                originalBGM.frequency, false);
            noteFx.SetData(f_fx, 0);
            bgmManager.NoteFxClip = noteFx;
        }

        private void RegisterAegleseekerAnomaly()
        {
            AegleseekerObject.SetActive(true);
            GameObject go7 = new GameObject("AegleseekerSceneControl");
            go7.transform.position = Vector3.zero;
            go7.transform.SetParent(gameObject.transform, false);
            Aegleseeker aegleseeker = go7.AddComponent<Aegleseeker>();
            aegleseeker.Init(this, 75f,
                AegleseekerObject.transform);
        }

        void GenerateHelloWinnerNotes()
        {
            byte[] diff = { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2 };
            Random1 = new Random(DateTime.Now.Millisecond);
            JObject jObject = Util.ReadJson(StaticResources.Instance.dataPath + "songs/" +
                                            SongKeyword + "/questions.json");
            JArray easy = jObject["easy"].ToObject<JArray>();
            JArray normal = jObject["normal"].ToObject<JArray>();
            JArray hard = jObject["hard"].ToObject<JArray>();
            bool[] easyUsed = Util.GenerateNewBoolArray(easy.Count);
            bool[] normalUsed = Util.GenerateNewBoolArray(normal.Count);
            bool[] hardUsed = Util.GenerateNewBoolArray(hard.Count);
            List<JObject> finalQuestions = new List<JObject>();
            for (int i = 0; i < diff.Length; i++)
            {
                switch (diff[i])
                {
                    case 0:
                        finalQuestions.Add(easy[EasyGetUnused()].ToObject<JObject>());

                        int EasyGetUnused()
                        {
                            int id = Random1.Next(0, easy.Count);
                            if (easyUsed[id])
                            {
                                return EasyGetUnused();
                            }

                            easyUsed[id] = true;
                            return id;
                        }

                        break;
                    case 1:
                        finalQuestions.Add(normal[NormalGetUnused()].ToObject<JObject>());

                        int NormalGetUnused()
                        {
                            int id = Random1.Next(0, normal.Count);
                            if (normalUsed[id])
                            {
                                return NormalGetUnused();
                            }

                            normalUsed[id] = true;
                            return id;
                        }

                        break;
                    case 2:
                        finalQuestions.Add(hard[HardGetUnused()].ToObject<JObject>());

                        int HardGetUnused()
                        {
                            int id = Random1.Next(0, hard.Count);
                            if (hardUsed[id])
                            {
                                return HardGetUnused();
                            }

                            hardUsed[id] = true;
                            return id;
                        }

                        break;
                    default:
                        Application.Quit();
                        throw new ArgumentException();
                }
            }

            Item<byte> orders = new Item<byte>(new byte[] { 0, 1, 2 });
            List<int> answerId = new List<int>();
            int id = 0;
            int j = 0;
            foreach (JObject finalQuestion in finalQuestions)
            {
                orders.ShuffleItems();
                byte[] finalOrder = orders.GetItems();
                string question = finalQuestion["question"].ToString();
                string[] fuck =
                {
                    finalQuestion["answer"].ToString(), finalQuestion["wrong1"].ToString(),
                    finalQuestion["wrong2"].ToString()
                };
                string[] choices = new string[finalOrder.Length];
                for (int i = 0; i < finalOrder.Length; i++)
                {
                    choices[finalOrder[i]] = fuck[i];
                    for (int k = 0; k < 32; k++)
                    {
                        NoteData note = new NoteData
                        {
                            id = id,
                            kind = (NoteKind)(i == 0 ? 5 : 10),
                            time = 19 + 4 * j + k / 32f,
                            pos = finalOrder[i] * 6,
                            width = 4f,
                            nsc = NoteData.NoteSC.GetCommonNSC(),
                            isFake = false,
                            parent = 0,
                            mode = NoteAppearMode.None
                        };
                        if (GameMirror) note.pos = 16 - note.pos - note.width;

                        drbfile.notes.Add(note);
                        drbfile.noteWeightCount += HPBar.NoteWeight[(int)note.kind];
                        id++;
                    }
                }

                answerId.Add(finalOrder[0]);
                GameObject questionObj = Instantiate(questionPrefab, canvasNormal);
                questionObj.name = "Question" + j;
                Question questionComp = questionObj.GetComponent<Question>();
                questionComp.Init(this, drbfile.CalculateDRBFileTime(16 + j * 4),
                    drbfile.CalculateDRBFileTime(16 + j * 4 + 3), question,
                    choices);
                j++;
            }
        }

        public GameObject questionPrefab;
        public Transform canvasNormal;

        private bool inited;
        public bool pauseable;

        public Image SCUpImage;
        public Image SCDownImage;
        private Tween SCUp;
        private Tween SCDown;

        public bool ended;
        public bool isHard;

        public bool IsPlaying
        {
            get
            {
                if (!inited || ended) return false;
                return progressManager.NowTime < endTime;
            }
        }

        private int activeNotes = 0;

        // Update is called once per frame
        void Update()
        {
            if (!inited || ended) return;

            if (progressManager.NowTime > lastNoteTime && JudgedNotes == noteTotal)
            {
                StartEndEvent();
                return;
            }

            if (hpManager.isCheap && isHard)
            {
                StartGameOverEvent();
                return;
            }

            //slider.value = BGMManager.time / BGMManager.clip.length * BGMManager.pitch;
            if (progressManager.NowTime < endTime)
            {
                progressManager.OnUpdate();

                //EQEffect
                if (GameEffectParamEQLevel >= 1)
                {
                    UpdateEQEffect();
                }

                //斜め　AngList
                UpdateRailAngle();

                //カメラの高さ　HgtList
                if (!drbfile.noPos)
                {
                    var pos = TheCamera.transform.position;
                    pos.y -= (pos.y - (9.0f + 18.0f * HeightCurve.Evaluate(bgmManager.Time))) * 20.0f * Time.deltaTime;
                    pos.z -= (pos.z - (-7.0f - 14.0f * HeightCurve.Evaluate(bgmManager.Time))) * 20.0f * Time.deltaTime;
                    pos.x -= (pos.x - PositionCurve.Evaluate(bgmManager.Time)) * 30.0f * Time.deltaTime;
                    TheCamera.transform.position = pos;
                }

                //HPMask描画
                HPMask.color -= (HPMask.color - new Color(1.0f, 1.0f, 1.0f, 0.0f)) * 0.2f;
                if (HPMask.color.a <= 0.001f)
                {
                    HPMask.gameObject.SetActive(false);
                }

                //HanteiBeam
                if (imgJudgeBeam[0] != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float a = imgJudgeBeam[i].color.a;
                        if (a > 0.1f)
                        {
                            a -= 2.0f * Time.deltaTime;
                        }
                        else
                        {
                            a = 0.0f;
                        }

                        imgJudgeBeam[i].color = new Color(imgJudgeBeam[i].color.r, imgJudgeBeam[i].color.g,
                            imgJudgeBeam[i].color.b, a);
                    }
                }
            }

            Distance = SCCurve.Evaluate(progressManager.NowTime);

            for (int i = CurrentSCn; i < drbfile.scs.Count; i++)
            {
                if (progressManager.NowTime + 123.4f > drbfile.CalculateDRBFileTime(drbfile.scs[i].sci))
                {
                    if (CurrentSC != drbfile.scs[i].sc)
                    {
                        CurrentSC = drbfile.scs[i].sc;
                        if (CurrentSC > 1)
                        {
                            DoSCUp();
                        }
                        else if (CurrentSC < 1)
                        {
                            DoSCDown();
                        }
                        else
                        {
                            DoSCClean();
                        }
                        //animSC.SetFloat("CurrentSC", CurrentSC);
                    }

                    CurrentSCn = i;
                }
                else
                {
                    break;
                }
            }


            //曲の進捗を表示する
            if (sliderMusicTime) sliderMusicTime.value = bgmManager.Time / bgmManager.MainClip.length;

            //終了処理
            //なし

            //ノーツ創り出す 1 Frame Max 5 Notes
            int count = 0;
            for (int i = makenotestartReal; i < drbfile.notes.Count; i++)
            {
                if (!isCreated[i])
                {
                    bool scLimit =
                        0.01f * ((drbfile.notes[i].IsTail() ? drbfile.notes[i].parent_dms : drbfile.notes[i].dms) -
                                 Distance) * drbfile.notes[i].nsc.value * RealNoteSpeed < 150.0f;
                    bool timeLimit = drbfile.notes[i].ms - progressManager.NowTime < 1000;
                    bool nscLimit =
                        drbfile.notes[i].ms - progressManager.NowTime < 10000.0f &&
                        (drbfile.notes[i].nsc.type == NoteSCType.MULTI ||
                         drbfile.notes[i].mode == NoteAppearMode.Jump);
                    if (activeNotes > 100 || !scLimit && !timeLimit && !nscLimit) continue;
                    if (!nscLimit) activeNotes++;
                    GameObject note = Instantiate(NotePrefab,
                        drbfile.notes[i].IsTap() ? notesUp.transform : notesDown.transform);
                    note.GetComponent<SpriteRenderer>().sprite = SpriteNotes[(int)drbfile.notes[i].kind];
                    if (drbfile.notes[i].IsTap())
                        note.GetComponent<SpriteRenderer>().sortingOrder = 3;
                    else if (drbfile.notes[i].IsFlick())
                        note.GetComponent<SpriteRenderer>().sortingOrder = 2;
                    else note.GetComponent<SpriteRenderer>().sortingOrder = 1;
                    note.GetComponent<TheNote>().mDrawer = meshDrawer;
                    note.GetComponent<TheNote>().Ready(this, inputManager, drbfile.notes[i]);
                    note.GetComponent<TheNote>().StartC();


                    isCreated[i] = true;
                    count++; //Max 5 notes

                    for (int ii = 0; ii < drbfile.notes.Count; ii++)
                    {
                        if (!isCreated[ii])
                        {
                            makenotestartReal = ii;
                            break;
                        }
                    }

                    //Max 5 notes
                    if (count >= 5)
                    {
                        break;
                    }
                }
            }

            for (int i = makenotestartFake; i < drbfile.fakeNotes.Count; i++)
            {
                if (!isCreated[i + drbfile.notes.Count])
                {
                    if ((0.01f * ((drbfile.fakeNotes[i].IsTail()
                                      ? drbfile.fakeNotes[i].parent_dms
                                      : drbfile.fakeNotes[i].dms) -
                                  Distance) * drbfile.fakeNotes[i].nsc.value * RealNoteSpeed < 150.0f)
                        || (drbfile.fakeNotes[i].ms - progressManager.NowTime < 1000)
                        || ((drbfile.fakeNotes[i].nsc.type == NoteSCType.MULTI ||
                             drbfile.fakeNotes[i].mode == NoteAppearMode.Jump) &&
                            drbfile.fakeNotes[i].ms - progressManager.NowTime < 10000.0f))
                    {
                        GameObject note = Instantiate(NotePrefab,
                            drbfile.fakeNotes[i].IsTap() ? notesUp.transform : notesDown.transform);
                        note.GetComponent<SpriteRenderer>().sprite = SpriteNotes[(int)drbfile.fakeNotes[i].kind];
                        if (drbfile.fakeNotes[i].IsTap())
                            note.GetComponent<SpriteRenderer>().sortingOrder = 3;
                        else if (drbfile.fakeNotes[i].IsFlick())
                            note.GetComponent<SpriteRenderer>().sortingOrder = 2;
                        else note.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        note.GetComponent<TheNote>().mDrawer = meshDrawer;
                        note.GetComponent<TheNote>().Ready(this, inputManager, drbfile.fakeNotes[i]);
                        note.GetComponent<TheNote>().StartC();

                        isCreated[i + drbfile.notes.Count] = true;
                        count++; //Max 5 notes

                        for (int ii = 0; ii < drbfile.fakeNotes.Count; ii++)
                        {
                            if (!isCreated[ii + drbfile.notes.Count])
                            {
                                makenotestartFake = ii;
                                break;
                            }
                        }

                        //Max 5 notes
                        if (count >= 5)
                        {
                            break;
                        }
                    }
                }
            }


            //SKYBOX移動
            //RenderSettings.skybox.SetFloat("_Rotation", 90.0f + 20.0f * Mathf.Sin(Time.realtimeSinceStartup / 100.0f * 2.0f * Mathf.PI));
        }

        void LoadEnd(EndType endType)
        {
            if (DebugMode)
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
                return;
            }

            if (endType == EndType.GAME_OVER)
            {
                QuitGame();
                return;
            }

            if (IsDankai)
            {
                dankaiDataContainer.nowId++;
                dankaiDataContainer.results.Add(new()
                {
                    score = Score,
                    pj = PerfectJ,
                    p = Perfect,
                    g = good,
                    m = miss
                });
                if (dankaiDataContainer.results.Count == dankaiDataContainer.songs.Length)
                {
                    FadeManager.Instance.JumpScene("dankaiResult");
                    return;
                }

                dankaiDataContainer.hpNow = hpManager.HpNow;

                FadeManager.Instance.JumpScene("game");
            }
            else if (storyMode)
            {
                switch (SongKeyword)
                {
                    case "sayonarahatsukoi":
                        if (storyUnlock.Equals("tutorial"))
                        {
                            NotificationBarManager.Instance.Show("Sayonara Hastsukoi [Past 1] 通过");
                            PlayerPrefs.SetInt("story_tutorial", 1);
                            PlayerPrefs.Save();
                        }

                        break;
                    case "etherstrike":
                        EtherStrike();
                        return;
                    case "fractureray":
                        NotificationBarManager.Instance.Show("Ether Strike异象触发，Fracture Ray通过");
                        // PlayerPrefs.SetInt("story_etherstrike", 1);
                        // PlayerPrefs.Save();
                        break;
                    case "grievouslady":
                        NotificationBarManager.Instance.Show("Axium Crisis异象触发，Grievous Lady通过");
                        // PlayerPrefs.SetInt("story_axiumcrisis", 1);
                        // PlayerPrefs.Save();
                        break;
                }

                QuitGame();
            }
            else
            {
                GameObject go = new GameObject("ResultDataContainer") { tag = "ResultData" };
                ResultDataContainer resultDataContainer = go.AddComponent<ResultDataContainer>();
#if UNITY_EDITOR
                resultDataContainer.endType = endType;
#else
                resultDataContainer.endType = GameAuto
                    ? EndType.AUTO_PLAY
                    : endType;
#endif
                resultDataContainer.PERFECT_J = PerfectJ;
                resultDataContainer.PERFECT = Perfect;
                resultDataContainer.GOOD = good;
                resultDataContainer.MISS = miss;
                resultDataContainer.FAST = fast;
                resultDataContainer.SLOW = slow;
                resultDataContainer.SCORE = Score;
                resultDataContainer.MAXCOMBO = MaxCombo;
                resultDataContainer.md5 = md5;
                resultDataContainer.bgColor = EndBgColor;
                resultDataContainer.hp = hpManager.HpNow;
                resultDataContainer.Accuracy = Accuracy;
                resultDataContainer.noteTotal = noteTotal;
                resultDataContainer.msDetails = msDetailsList;
                DontDestroyOnLoad(go);
                GlobalSettings.CurrentSettings = currentSettings;
                GlobalSettings.Save();
                FadeManager.Instance.JumpScene("result");
            }
        }

        void QuitGame()
        {
            CheckDataContainers.CleanSongDataContainer();
            CheckDataContainers.CleanDankaiDataContainer();
            GlobalSettings.CurrentSettings = currentSettings;
            GlobalSettings.Save();
            FadeManager.Instance.Back();
        }

        IEnumerator CleanTypeAnimation(GameObject cleanTypeObj)
        {
            var text = (RectTransform)cleanTypeObj.transform.Find("Text");
            var bg = (RectTransform)cleanTypeObj.transform.Find("ImageBg");
            var find = cleanTypeObj.transform.Find("ImageLight");
            if (find)
            {
                var flash = find.gameObject.GetComponent<Image>();
                flash.color = new Color(1, 1, 1, 0.1f);
                flash.DOFade(0, 0.2f);
            }

            text.anchoredPosition = new Vector2(1500f, 0f);
            text.DOLocalMoveX(0, 0.2f).SetEase(Ease.OutSine);
            yield return new WaitForSeconds(0.2f);
            text.DOLocalMoveX(-1500, 0.2f).SetEase(Ease.InSine);
            bg.DOLocalMoveX(3000, 0.2f).SetEase(Ease.InSine);
        }

        private void StartEndEvent()
        {
            ended = true;
            progressManager.StopTiming();
            if (hpManager.isCheap)
            {
                StartCoroutine(EndEvent(EndType.FAILED));
            }
            else if (miss != 0)
            {
                StartCoroutine(EndEvent(EndType.COMPLETED));
            }
            else if (good != 0)
            {
                StartCoroutine(EndEvent(EndType.FULL_COMBO));
            }
            else
            {
                StartCoroutine(EndEvent(EndType.ALL_PERFECT));
            }
        }

        private void MakeStoryChallenge(TheSelectManager.SongData songData, string unlock, GameSide gameSide,
            int maxInput = 20, string customTierIdentifier = "Tier", Color? customTierColor = null,
            bool ratingPlus = false)
        {
            CheckDataContainers.CleanSongDataContainer();

            GameObject go = new GameObject("StoryChallengeContainer") { tag = "SongData" };
            StoryChallengeContainer storyChallengeContainer = go.AddComponent<StoryChallengeContainer>();
            storyChallengeContainer.songData = songData;
            storyChallengeContainer.music =
                ExternalResources.LoadAudioClip($"STORY/SONGS/{storyChallengeContainer.songData.keyword}");
            storyChallengeContainer.songData.cover =
                ExternalResources.LoadSprite("STORY/SONGS/" + storyChallengeContainer.songData.keyword);
            storyChallengeContainer.isComputer = Application.platform switch
            {
                RuntimePlatform.WindowsEditor => true,
                RuntimePlatform.WindowsPlayer => true,
                RuntimePlatform.LinuxEditor => true,
                RuntimePlatform.LinuxPlayer => true,
                _ => false
            };
            storyChallengeContainer.selectedDiff = storyChallengeContainer.songData.hards[0];
            storyChallengeContainer.saveAudio = false;
            storyChallengeContainer.unlock = unlock;
            storyChallengeContainer.hasTextBeforeStart = false;
            storyChallengeContainer.tierIdentifier = customTierIdentifier;
            storyChallengeContainer.customTierColor = customTierColor;
            storyChallengeContainer.ratingPlus = ratingPlus;
            DontDestroyOnLoad(storyChallengeContainer);
            GlobalSettings.CurrentSettings = currentSettings;
            GlobalSettings.Save();
            FadeManager.Instance.JumpScene("game");
        }

        private void EtherStrike()
        {
            MakeStoryChallenge(new TheSelectManager.SongData
            {
                keyword = "fractureray",
                songName = "RnJhY3R1cmUgUmF5",
                songArtist = "U2FrdXp5bw==",
                bpm = "200",
                hards = new[] { 11 },
            }, "etherstrike", GameSide.LIGHT, 20, "Future", Util.GetTierColor(16));
        }

        public IEnumerator AxiumCrisis()
        {
            if (!storyMode) yield break;
            ended = true;
            progressManager.StopTiming();
            if (hasVideo) VideoPlayer.Stop();
            GameObject cleanTypeObj = AnimationGrades[(int)EndType.AXIUM_CRISIS];
            if (cleanTypeObj)
            {
                cleanTypeObj.SetActive(true);
                StartCoroutine(CleanTypeAnimation(cleanTypeObj));
            }

            WaitForSeconds w01s = new WaitForSeconds(0.1f);

            for (int i = 0; i < 10; i++)
            {
                bgmManager.Volume -= 0.1f;
                yield return w01s;
            }

            MakeStoryChallenge(new TheSelectManager.SongData
            {
                keyword = "grievouslady",
                songName = "R3JpZXZvdXMgTGFkeQ==",
                songArtist = "VGVhbSBHcmltb2lyZSB2cyBMYXVy",
                bpm = "210",
                hards = new[] { 11 }
            }, "axiumcrisis", GameSide.DARK, 20, "Future", Util.GetTierColor(16));
            GlobalSettings.CurrentSettings = currentSettings;
            GlobalSettings.Save();
            FadeManager.Instance.JumpScene("game");
        }

        IEnumerator EndEvent(EndType grade)
        {
            GameObject cleanTypeObj = AnimationGrades[(int)grade];
            if (cleanTypeObj)
            {
                cleanTypeObj.SetActive(true);
                if (grade != EndType.FAILED && grade != EndType.GAME_OVER)
                {
                    StartCoroutine(CleanTypeAnimation(cleanTypeObj));
                }
            }


            bgmManager.DoFade(0, 1, Ease.Linear);
            yield return new WaitForSeconds(1f);

            //終了処理
            LoadEnd(grade);
        }

        public void StartGameOverEvent()
        {
            ended = true;
            progressManager.StopTiming();
            StartCoroutine(GameOverEvent());
        }

        IEnumerator GameOverEvent()
        {
            if (AnimationGrades[(int)EndType.GAME_OVER])
                AnimationGrades[(int)EndType.GAME_OVER].SetActive(true);

            bgmManager.Pause();
            if (hasVideo) VideoPlayer.Pause();
            progressManager.StopTiming();

            AudioSource ac = gameObject.AddComponent<AudioSource>();
            ac.volume = 0.5f;
            ac.PlayOneShot(_acGameover);

            // WaitForSeconds w01s = new WaitForSeconds(0.1f);
            //
            // for (int i = 0; i < 25; i++)
            // {
            //     yield return w01s;
            // }

            yield return new WaitForSeconds(2.5f);
            //GameOver処理

            LoadEnd(EndType.GAME_OVER);
        }

        private void UpdateEQEffect()
        {
            if (EQList.Count <= 0)
            {
                AudioMixerFreq -= (AudioMixerFreq - 1.0f) * 20 * Time.deltaTime;
                AudioMixerCenter -= (AudioMixerCenter - 0.5f) * 20 * Time.deltaTime;
            }
            else
            {
                float adv = EQList.Average();

                AudioMixerFreq -= (AudioMixerFreq - (1.0f + 0.15f * GameEffectParamEQLevel)) * 20.0f *
                                  Time.deltaTime;
                AudioMixerCenter -= (AudioMixerCenter - adv) * 20.0f * Time.deltaTime;
            }

            //High Pass
            AudioMixerHiPass = HPList.Count <= 0 ? 0.0f : HPList.Average();

            //Low Pass
            AudioMixerLoPass = LPList.Count <= 0 ? 1.0f : LPList.Average();

            if (SteroList.Count <= 0)
            {
                bgmManager.panStereo = 0;
            }
            else
            {
                float result = SteroList.Average();
                if (result > 1) result = 1;
                if (result < -1) result = -1;
                bgmManager.panStereo = result;
            }

            float center = EQCurve.Evaluate(AudioMixerCenter);
            float hp = EQCurve.Evaluate(AudioMixerHiPass);
            float lp = EQCurve.Evaluate(AudioMixerLoPass);
            
            audioMixer.SetFloat("Center", center);
            audioMixer.SetFloat("Freq", AudioMixerFreq);
            audioMixer.SetFloat("HPFreq", hp);
            audioMixer.SetFloat("LPFreq", lp);
            audioMixer.SetFloat("Center1", center);
            audioMixer.SetFloat("Freq1", AudioMixerFreq);
            audioMixer.SetFloat("HPFreq1", hp);
            audioMixer.SetFloat("LPFreq1", lp);
            
            EQList.Clear();
            HPList.Clear();
            LPList.Clear();
            SteroList.Clear();
        }

        private void UpdateRailAngle()
        {
            if (AngList.Count <= 0)
            {
                var angle = TheCamera.transform.eulerAngles;
                if (angle.z > 180.0f) angle.z -= 360.0f;
                angle.z -= (angle.z - 0.0f) * 20.0f * Time.deltaTime;
                if (angle.z < 0.0f) angle.z += 360.0f;
                TheCamera.transform.eulerAngles = angle;
            }
            else
            {
                var angle = TheCamera.transform.eulerAngles;
                if (angle.z > 180.0f) angle.z -= 360.0f;
                angle.z -= (angle.z - AngList.Average()) * 10.0f * Time.deltaTime;
                if (angle.z < 0.0f) angle.z += 360.0f;
                TheCamera.transform.eulerAngles = angle;
            }

            AngList.Clear();
        }

        bool isCovering(NoteData o1, NoteData o2)
        {
            float l1, w1, l2, w2;
            /*l1 = o1.pos;
        w1 = o1.width;
        l2 = o2.pos;
        w2 = o2.width;
        */
            l1 = o1.pos - 1;
            w1 = o1.width + 2;
            l2 = o2.pos - 1;
            w2 = o2.width + 2;

            if (l2 + w2 <= l1) return false;
            if (l2 >= l1 + w1) return false;

            return true;
        }

        public void JumpTo()
        {
            if (!inited) return;
            float time;
            try
            {
                time = drbfile.CalculateDRBFileTime(float.Parse(inputJumpTo.text)) * bgmManager.Pitch;
                if (bgmManager.MainClip.length < time / 1000)
                {
                    time = bgmManager.MainClip.length * 1000 - 1;
                }

                if (time < 0)
                {
                    time = 0;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }

            float from = progressManager.NowTime;

            // for (int i = 0; i < isCreated.Count; i++)
            // {
            //     if (time < from)
            //     {
            //         if (drbfile.note[i].ms >= time && drbfile.note[i].ms < from * 1000)
            //         {
            //             isCreated[i] = false;
            //         }
            //     }
            //     else
            //     {
            //         if (drbfile.note[i].ms < time && drbfile.note[i].ms >= from * 1000)
            //         {
            //             isCreated[i] = false;
            //         }
            //     }
            // }
            for (int i = makenotestartReal; i < drbfile.notes.Count; i++)
            {
                if (from > time) break;
                if (!isCreated[i])
                {
                    if (drbfile.notes[i].ms < time && drbfile.notes[i].ms >= from)
                    {
                        JudgeDirectly(drbfile.notes[i].kind, drbfile.notes[i].ms, drbfile.notes[i].isFake);

                        isCreated[i] = true;

                        for (int ii = 0; ii < drbfile.notes.Count; ii++)
                        {
                            if (!isCreated[ii])
                            {
                                makenotestartReal = ii;
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = makenotestartFake; i < drbfile.fakeNotes.Count; i++)
            {
                if (from > time) break;
                if (!isCreated[i + drbfile.notes.Count])
                {
                    if (drbfile.fakeNotes[i].ms < time && drbfile.fakeNotes[i].ms >= from)
                    {
                        JudgeDirectly(drbfile.fakeNotes[i].kind, drbfile.notes[i].ms, drbfile.fakeNotes[i].isFake);

                        isCreated[i + drbfile.notes.Count] = true;

                        for (int ii = 0; ii < drbfile.fakeNotes.Count; ii++)
                        {
                            if (!isCreated[ii + drbfile.notes.Count])
                            {
                                makenotestartFake = ii;
                                break;
                            }
                        }
                    }
                }
            }

            progressManager.AddDelay((from - time) / 1000f);
            Distance = SCCurve.Evaluate(time);
            bgmManager.Time = time / 1000;
            if (hasVideo) VideoPlayer.time = time / 1000;
            theLyricManager.JumpTo(time);

            progressManager.ContinueTiming();
            isPause = false;
            bgmManager.UnPause();
            pauseUI.SetActive(false);
        }
#if !UNITY_EDITOR
        private void OnApplicationPause(bool pause)
        {
            if (inited && !isPause) Pause();
        }


        private void OnApplicationFocus(bool focus)
        {
            if (inited && !isPause) Pause();
        }
#endif

        public GameObject changeSpeed;
        public Text speedLabel;
        public Slider slider;
        public string speedLabelText;

        public void Pause()
        {
            if (!pauseable || bgmManager.Time <= 0) return;
            isPause = true;
            bgmManager.Pause();
            VideoPlayer.Pause();
            progressManager.StopTiming();
            pauseUI.SetActive(true);
            jumpTo.SetActive(GameAuto);
            changeSpeed.SetActive(!GameAuto);
            VideoMask.DOKill();
            if (!GameAuto)
            {
                speedLabel.text = speedLabelText + (RealNoteSpeed / 2f).ToString("N1") + "x";
            }
        }

        public void UpdateNoteSpeed()
        {
            currentSettings.NoteSpeed = NoteSpeed = (int)slider.value;
            RealNoteSpeed = Mathf.Abs(bgmManager.Pitch - 1.0f) > 0.1f && currentSettings.enableSCFix
                ? NoteSpeed / bgmManager.Pitch
                : NoteSpeed;
            speedLabel.text = speedLabelText + (RealNoteSpeed / 2f).ToString("N1") + "x";
        }

        public void Retry()
        {
            if (!pauseable || bgmManager.Time <= 0 || IsDankai) return;
            GlobalSettings.CurrentSettings = currentSettings;
            GlobalSettings.Save();
            FadeManager.Instance.JumpScene("game");
        }

        public void Resume()
        {
            if (!pauseable || bgmManager.Time <= 0) return;
            isPause = false;
            pauseUI.SetActive(false);
            TimeGoBack(3000f);
        }

        public void Quit()
        {
            if (!pauseable || bgmManager.Time <= 0) return;
            progressManager.StopTiming();
            QuitDirectly();
        }

        public void QuitDirectly()
        {
#if UNITY_EDITOR
            if (DebugMode)
            {
                EditorApplication.isPlaying = false;
                return;
            }
#endif
            if (!DebugMode) QuitGame();
            else Application.Quit();
        }

        void TimeGoBack(float delta)
        {
            StartCoroutine(TimeGoBackCoroutine(delta));
        }

        IEnumerator TimeGoBackCoroutine(float delta)
        {
            float from = progressManager.NowTime + NoteOffset + 100f;
            float to = progressManager.NowTime + NoteOffset + 100f - delta * bgmManager.Pitch;
            for (var i = 0; i < notesUp.transform.childCount; i++)
            {
                try
                {
                    notesUp.transform.GetChild(i).GetComponent<TheNote>().StopC();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

            for (var i = 0; i < notesDown.transform.childCount; i++)
            {
                try
                {
                    notesDown.transform.GetChild(i).GetComponent<TheNote>().StopC();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

            if (bgmManager.Time < delta * bgmManager.Pitch / 1000f)
            {
                delta = bgmManager.Time * 1000f / bgmManager.Pitch;
                bgmManager.Time = 0;
                if (hasVideo) VideoPlayer.time = 0;
                progressManager.AddDelay(delta / 1000f);
            }
            else
            {
                if (bgmManager.MainClip.length > to / 1000f) bgmManager.Time = to / 1000f;
                if (hasVideo && VideoPlayer.length > to / 1000f) VideoPlayer.time = to / 1000f;
                progressManager.AddDelay(delta / 1000f);
            }

            for (int i = 0; i < drbfile.notes.Count; i++)
            {
                if (drbfile.notes[i].ms < from)
                {
                    isCreated[i] = true;
                }
            }

            for (int i = 0; i < drbfile.fakeNotes.Count; i++)
            {
                if (drbfile.notes[i].ms < from)
                {
                    isCreated[i + drbfile.notes.Count] = true;
                }
            }

            for (var i = 0; i < notesUp.transform.childCount; i++)
            {
                try
                {
                    notesUp.transform.GetChild(i).GetComponent<TheNote>().StartC();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

            for (var i = 0; i < notesDown.transform.childCount; i++)
            {
                try
                {
                    notesDown.transform.GetChild(i).GetComponent<TheNote>().StartC();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

            progressManager.ContinueTiming();
            bgmManager.UnPause();
            if (hasVideo) VideoPlayer.Play();

            bgmManager.Volume = 0;
            int subdivision = Mathf.FloorToInt(delta / 100f);
            if (subdivision <= 0)
            {
                bgmManager.Volume = 1;
                yield break;
            }

            var qwq = new WaitForSeconds(delta / subdivision / 1000f);
            if (hasVideo)
            {
                VideoMask.color = new Color(1, 1, 1, 1);
                VideoMask.DOFade(180 / 255f, delta / 1000f);
            }

            for (int i = 1; i <= subdivision; i++)
            {
                bgmManager.Volume = i * 1.0f / subdivision;
                yield return qwq;
            }
        }

        void SCORE_INIT()
        {
            Score = 0;
            LiLunZhi = MaxScore = _scoreType switch
            {
                SCORE_TYPE.ORIGINAL => 3000000,
                SCORE_TYPE.ARCAEA => 10000000 + noteTotal,
                SCORE_TYPE.PHIGROS => 1000000,
                _ => throw new ArgumentOutOfRangeException()
            };
            Combo = 0;
            MaxCombo = 0;
            PerfectJ = 0;
            Perfect = 0;
            good = 0;
            miss = 0;
            fast = 0;
            slow = 0;
            PassedNotes = 0;
            AccMSList.Clear();
            msDetailsList.Clear();
        }

        private void JudgeDirectly(NoteKind kind, float noteMs, bool isFake)
        {
            if (isFake) return;
            msDetailsList.Add(0f);
            PerfectJ++;
            PassedNotes++;
            AddCombo();
            hpManager.InCreaseHp(hpManager.HpBar.PerfectJHP(kind, drbfile.noteWeightCount, this));
            // if (A.Instance) A.Instance.Judge(JudgeType.PERFECT_J, noteMs);
        }

        //判定処理
        public void Judge(float ms, NoteKind kind, bool isFake, Vector3 pos, float width)
        {
            //LISTに登録
            var go2 = Instantiate(HanteiPrefab, pos, Quaternion.identity);
            GameObject go;
            float realMS = ms / bgmManager.Pitch;
            if (!isFake) msDetailsList.Add(realMS);
            int displayMS = isFake ? 0 : (int)realMS;
            JudgeType judgeType;

            activeNotes--;
            PassedNotes++;
            //PERFECT JUSTICE 判定
            if (Mathf.Abs(ms) <= PJms)
            {
                judgeType = JudgeType.PERFECT_J;
                if (!isFake)
                {
                    PerfectJ++;
                    AddCombo();
                    hpManager.InCreaseHp(hpManager.HpBar.PerfectJHP(kind, drbfile.noteWeightCount, this));
                    if (imgJudgeBeam[0])
                        imgJudgeBeam[0].color = new Color(imgJudgeBeam[0].color.r, imgJudgeBeam[0].color.g,
                            imgJudgeBeam[0].color.b, 1.0f);
                }

                go = Instantiate(prefabEffect[(int)kind], pos, Quaternion.identity);

                if (gameSubJudgeDisplay == GameSubJudgeDisplay.MS && kind == NoteKind.TAP)
                    go2.GetComponent<JudgeImage>().Init(4, true, displayMS);
                else go2.GetComponent<JudgeImage>().Init(4);
            }
            //PERFECT 判定
            else if (Mathf.Abs(ms) <= PFms)
            {
                judgeType = JudgeType.PERFECT;
                Perfect++;
                AddCombo();
                FastOrSlow(ms);
                hpManager.InCreaseHp(hpManager.HpBar.PerfectHP(kind, drbfile.noteWeightCount, this));

                go = Instantiate(prefabEffect[(int)kind], pos, Quaternion.identity);
                switch (gameSubJudgeDisplay)
                {
                    case GameSubJudgeDisplay.NONE:
                        go2.GetComponent<JudgeImage>().Init(3);
                        break;
                    case GameSubJudgeDisplay.FAST_SLOW:
                        go2.GetComponent<JudgeImage>().Init(3, false, 0, ms >= 0 ? 2 : 1);
                        break;
                    case GameSubJudgeDisplay.MS:
                        go2.GetComponent<JudgeImage>().Init(3, true, displayMS, ms >= 0 ? 2 : 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (imgJudgeBeam[1])
                    imgJudgeBeam[1].color = new Color(imgJudgeBeam[1].color.r, imgJudgeBeam[1].color.g,
                        imgJudgeBeam[1].color.b, 1.0f);

                ////Event
                //if (isEvent)
                //{
                //    BossSkillCnt += 1;
                //    if (BossSkillCnt >= 20)
                //    {
                //        StartCoroutine(BossSkill());
                //        BossSkillCnt = 0;
                //    }
                //}
            }
            //GOOD 判定
            else if (Mathf.Abs(ms) <= GDms)
            {
                judgeType = JudgeType.GOOD;
                good++;
                AddCombo();
                FastOrSlow(ms);
                hpManager.DecreaseHp(hpManager.HpBar.GoodHP(kind, drbfile.noteWeightCount, this));

                go = Instantiate(prefabEffect[(int)kind], pos, Quaternion.identity);
                switch (gameSubJudgeDisplay)
                {
                    case GameSubJudgeDisplay.NONE:
                        go2.GetComponent<JudgeImage>().Init(2);
                        break;
                    case GameSubJudgeDisplay.FAST_SLOW:
                        go2.GetComponent<JudgeImage>().Init(2, false, 0, ms >= 0 ? 2 : 1);
                        break;
                    case GameSubJudgeDisplay.MS:
                        go2.GetComponent<JudgeImage>().Init(2, true, displayMS, ms >= 0 ? 2 : 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (imgJudgeBeam[2])
                    imgJudgeBeam[2].color = new Color(imgJudgeBeam[2].color.r, imgJudgeBeam[2].color.g,
                        imgJudgeBeam[2].color.b, 1.0f);
                //Event
                //if (isEvent)
                //{
                //    BossSkillCnt += 2;
                //    if (BossSkillCnt >= 20)
                //    {
                //        StartCoroutine(BossSkill());
                //        BossSkillCnt = 0;
                //    }
                //}
            }
            //MISS 判定
            else
            {
                judgeType = JudgeType.MISS;
                miss++;
                if (Combo >= 30)
                {
                    //血が出る警告
                    ShowHPMask();
                }

                Combo = 0;
                ReflashCombo();
                hpManager.DecreaseHp(hpManager.HpBar.MissHP(kind, drbfile.noteWeightCount, this));

                go = null;
                go2.GetComponent<JudgeImage>().Init(1);
                if (imgJudgeBeam[3])
                    imgJudgeBeam[3].color = new Color(imgJudgeBeam[3].color.r, imgJudgeBeam[3].color.g,
                        imgJudgeBeam[3].color.b, 1.0f);
                ////Event
                //if (isEvent)
                //{
                //    BossSkillCnt += 3;
                //    if (BossSkillCnt >= 20)
                //    {
                //        StartCoroutine(BossSkill());
                //        BossSkillCnt = 0;
                //    }
                //}
            }

            // if (A.Instance) A.Instance.Judge(judgeType, progressManager.NowTime + ms);

            if (go)
            {
                go.transform.localScale = new Vector3(width * 0.2f + 2.0f, width * 0.2f + 2.0f, width * 0.2f + 2.0f);
            }
        }

        void AddCombo()
        {
            Combo++;
            MaxCombo = Mathf.Max(MaxCombo, Combo);
            ReflashCombo();
        }

        void ReflashCombo()
        {
            //表示物反応
            switch (_scoreType)
            {
                case SCORE_TYPE.ORIGINAL:
                    Score = 3000000.0f * (PerfectJ * 1.0f + Perfect * 0.99f + good / 3.0f) / noteTotal;
                    MaxScore = 3000000.0f -
                               3000000.0f * (Perfect * 0.01f + good / 3.0f * 2.0f + miss * 1.0f) / noteTotal;
                    Accuracy = AccMSList.Count > 0 ? AccMSList.Average() / GDms * 100f : 0.0f;
                    break;
                case SCORE_TYPE.ARCAEA:
                    Score = 10000000.0f * (PerfectJ + Perfect + good * 0.5f) / noteTotal + PerfectJ;
                    MaxScore = 10000000.0f - 10000000.0f * (good * 0.5f + miss) / noteTotal +
                               (noteTotal - Perfect - good - miss);
                    Accuracy = AccMSList.Count > 0 ? AccMSList.Average() / GDms * 100f : 0.0f;
                    break;
                case SCORE_TYPE.PHIGROS:
                    Score = 900000.0f * (PerfectJ + Perfect + good * 0.65f) / noteTotal +
                            MaxCombo * 100000f / noteTotal;
                    MaxScore = 1000000.0f - 900000.0f * (miss + good * 0.35f) / noteTotal -
                               100000.0f * (JudgedNotes - MaxCombo) / noteTotal;
                    Accuracy = 100.0f * (JudgedNotes == 0
                        ? 0.0f
                        : (PerfectJ + Perfect + good * 0.65f) / JudgedNotes) / GDms * 100f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            if (textScore) textScore.text = GameUtil.ParseScore(Mathf.RoundToInt(Score), _scoreType);
            if (textMaxcombo) textMaxcombo.text = MaxCombo + "";
            if (textPerfect) textPerfect.text = PerfectJ + "";
            if (textPerfect2) textPerfect2.text = Perfect + "";
            if (textGood) textGood.text = good + "";
            if (textMiss) textMiss.text = miss + "";


            if (objCombo[0] != null)
            {
                if (playerGameComboDisplay == GameComboDisplay.NONE ||
                    playerGameComboDisplay == GameComboDisplay.COMBO && Combo <= 2)
                {
                    _topBarValue.color = _topBarLabel.color =
                        new Color(comboWhite[0], comboWhite[1], comboWhite[2], 0.0f);
                    _topBarValueRed.color = new Color(comboRed[0], comboRed[1], comboRed[2], 0.0f);
                    _topBarValueBlue.color = new Color(comboBlue[0], comboBlue[1], comboBlue[2], 0.0f);
                }
                else
                {
                    if (miss > 0 || !FCAPIndicator)
                    {
                        _topBarValue.color = _topBarLabel.color =
                            new Color(comboWhite[0], comboWhite[1], comboWhite[2], 1.0f);
                        _topBarValueRed.color = new Color(comboRed[0], comboRed[1], comboRed[2], 1.0f);
                        _topBarValueBlue.color =
                            new Color(comboBlue[0], comboBlue[1], comboBlue[2], 1.0f);
                    }
                    else if (good > 0)
                    {
                        _topBarLabel.color =
                            _topBarValue.color = new Color(0f, 1.0f, 2f / 15f, 1.0f);
                        _topBarValueRed.color = new Color(comboRed[0], comboRed[1], comboRed[2], 1.0f);
                        _topBarValueBlue.color =
                            new Color(comboBlue[0], comboBlue[1], comboBlue[2], 1.0f);
                    }
                    else if (Perfect > 0)
                    {
                        _topBarLabel.color =
                            _topBarValue.color = new Color(1.0f, 0.5f, 0.196f, 1.0f);
                        _topBarValueRed.color = new Color(comboRed[0], comboRed[1], comboRed[2], 1.0f);
                        _topBarValueBlue.color =
                            new Color(comboBlue[0], comboBlue[1], comboBlue[2], 1.0f);
                    }
                    else
                    {
                        _topBarLabel.color =
                            _topBarValue.color = new Color(1.0f, 0.94f, 0.196f, 1.0f);
                        _topBarValueRed.color = new Color(comboRed[0], comboRed[1], comboRed[2], 1.0f);
                        _topBarValueBlue.color =
                            new Color(comboBlue[0], comboBlue[1], comboBlue[2], 1.0f);
                    }

                    string disp = "";
                    if (playerGameComboDisplay == GameComboDisplay.COMBO)
                    {
                        disp = Combo.ToString();
                    }
                    else if (playerGameComboDisplay == GameComboDisplay.SCORE)
                    {
                        disp = Mathf.RoundToInt(Score) + "";
                    }
                    else if (playerGameComboDisplay == GameComboDisplay.MSCORE)
                    {
                        disp = Mathf.RoundToInt(MaxScore) + "";
                    }
                    else if (playerGameComboDisplay == GameComboDisplay.ACCURACY)
                    {
                        disp = Accuracy.ToString("0.00") + "%";
                    }
                    else if (playerGameComboDisplay == GameComboDisplay.LAGRANGE)
                    {
                        disp = ParseLagrangePace();
                    }

                    _topBarValue.text = disp;
                    _topBarValueRed.text = disp;
                    _topBarValueBlue.text = disp;

                    objCombo[0].transform.localScale = new Vector3(
                        Mathf.Min(2f, objCombo[0].transform.localScale.x + 0.1f),
                        Mathf.Min(2f, objCombo[0].transform.localScale.x + 0.1f),
                        Mathf.Min(2f, objCombo[0].transform.localScale.x + 0.1f));
                    objCombo[1].transform.localScale = new Vector3(
                        Mathf.Min(2f, objCombo[1].transform.localScale.x + 0.1f),
                        Mathf.Min(2f, objCombo[1].transform.localScale.x + 0.1f),
                        Mathf.Min(2f, objCombo[1].transform.localScale.x + 0.1f));
                    objCombo[2].transform.localScale = new Vector3(
                        Mathf.Min(2f, objCombo[2].transform.localScale.x + 0.1f),
                        Mathf.Min(2f, objCombo[2].transform.localScale.x + 0.1f),
                        Mathf.Min(2f, objCombo[2].transform.localScale.x + 0.1f));
                    objCombo[1].transform.position +=
                        new Vector3(
                            UnityEngine.Random.value < 0.5
                                ? UnityEngine.Random.Range(-0.08f, -0.04f)
                                : UnityEngine.Random.Range(0.04f, 0.08f),
                            UnityEngine.Random.value < 0.5
                                ? UnityEngine.Random.Range(-0.08f, -0.04f)
                                : UnityEngine.Random.Range(0.04f, 0.08f), 0.0f);
                    objCombo[2].transform.position +=
                        new Vector3(
                            UnityEngine.Random.value < 0.5
                                ? UnityEngine.Random.Range(-0.08f, -0.04f)
                                : UnityEngine.Random.Range(0.04f, 0.08f),
                            UnityEngine.Random.value < 0.5
                                ? UnityEngine.Random.Range(-0.08f, -0.04f)
                                : UnityEngine.Random.Range(0.04f, 0.08f), 0.0f);
                }
            }
        }

        void FastOrSlow(float ms)
        {
            if (ms >= 0)
            {
                slow++;
            }
            else
            {
                fast++;
            }
        }

        public Sprite GetSpriteArror(int num)
        {
            switch (gameSide)
            {
                case GameSide.LIGHT:
                    return SpriteArrorLight[num];
                case GameSide.DARK:
                    return SpriteArrorDark[num];
                case GameSide.COLORLESS:
                    return SpriteArrorLight[num];
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        class EQ
        {
            public float center; //20~22000;
            public float range; //1-5;
            public float freq; //1-3;
        }

        public void AddEQ(float f)
        {
            EQList.Add(f);
        }

        public void AddHPass(float f)
        {
            HPList.Add(f);
        }

        public void AddLPass(float f)
        {
            LPList.Add(f);
        }

        public void AddAngel(float f)
        {
            AngList.Add(f);
        }

        public void AddStereo(float f)
        {
            SteroList.Add(f);
        }

        public void ShowHPMask()
        {
            HPMask.gameObject.SetActive(true);
            HPMask.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        public GameObject background;
        public VideoPlayer VideoPlayer;
        public Image VideoMask;
        public GlobalSettings currentSettings;
        private Text _topBarValue;
        private Text _topBarValueRed;
        private Text _topBarValueBlue;
        private Text _topBarLabel;

        public void DoSCUp()
        {
            if (SCUp.active) SCUp.Kill();
            if (SCDown.active) SCDown.Kill();
            SCUp = SCUpImage.DOFade(0.1f, 0.1f);
            SCDown = SCDownImage.DOFade(0f, 0.1f);
        }

        public void DoSCDown()
        {
            if (SCUp.active) SCUp.Kill();
            if (SCDown.active) SCDown.Kill();
            SCUp = SCUpImage.DOFade(0f, 0.1f);
            SCDown = SCDownImage.DOFade(0.1f, 0.1f);
        }

        public void DoSCClean()
        {
            if (SCUp.active) SCUp.Kill();
            if (SCDown.active) SCDown.Kill();
            SCUp = SCUpImage.DOFade(0f, 0.1f);
            SCDown = SCDownImage.DOFade(0f, 0.1f);
        }

        private AudioClip GetAudioClipFromRawWav(string path, int frequency = -1, string clipName = "")
        {
            RawWav rawWav = Resources.Load<RawWav>(path);
            if (rawWav == null) return null;
            WAV wav = new WAV(rawWav.data);
            if (frequency > 0 && frequency != wav.Frequency)
            {
                return AudioFrequencyConverter.ConvertWavToAudioClip(rawWav.data, frequency, clipName);
            }

            return wav.ToAudioClip();
        }

        private (AnimationCurve, bool) GeneratePositionCurve(DRBFile drbfile)
        {
            List<float> existedTime = new List<float>();
            List<Keyframe> kfpos = new List<Keyframe>();
            kfpos.Add(new Keyframe(0, 0));
            float currentp = 0.0f;
            for (var i = 0; i < drbfile.notes.Count; i++)
            {
                var note = drbfile.notes[i];
                if (note.IsMover())
                {
                    if (!existedTime.Exists(time => time == note.parent_time))
                    {
                        existedTime.Add(note.parent_time);
                        kfpos.Add(new Keyframe(note.parent_ms / 1000.0f, currentp));
                    }

                    currentp += (note.pos + note.width * 0.5f) - (note.parent_pos + note.parent_width * 0.5f);
                    kfpos.Add(new Keyframe(note.ms / 1000.0f, currentp));
                }
            }

            Keyframe[] kfposa = kfpos.ToArray();
            Util.LinearKeyframe(kfposa);
            return (new AnimationCurve(kfposa), kfpos.Count > 1);
        }

        private void GenerateHeightCurve(DRBFile drbfile)
        {
            //调整摄像机高度
            for (var i = 0; i < drbfile.notes.Count; i++)
            {
                var note = drbfile.notes[i];
                if (note.pos < 0)
                {
                    int s = (int)(note.ms / 1000.0f);
                    if (s + 0 >= 0 && s + 0 < HgtList.Length)
                        HgtList[s + 0] = Mathf.Max(note.pos / (-16.0f), HgtList[s + 0]);
                    if (s + 1 >= 0 && s + 1 < HgtList.Length)
                        HgtList[s + 1] = Mathf.Max(note.pos / (-16.0f), HgtList[s + 1]);

                    if (note.pos < -8)
                    {
                        if (s - 1 >= 0 && s - 1 < HgtList.Length)
                            HgtList[s - 1] = Mathf.Max(note.pos / (-32.0f), HgtList[s - 1]);
                        if (s + 2 >= 0 && s + 2 < HgtList.Length)
                            HgtList[s + 2] = Mathf.Max(note.pos / (-32.0f), HgtList[s + 2]);
                    }
                }

                if (note.pos + note.width > 16)
                {
                    int s = (int)(note.ms / 1000.0f);
                    if (s + 0 >= 0 && s + 0 < HgtList.Length)
                        HgtList[s + 0] = Mathf.Max((note.pos + note.width - 16.0f) / 16.0f,
                            HgtList[s + 0]);
                    if (s + 1 >= 0 && s + 1 < HgtList.Length)
                        HgtList[s + 1] = Mathf.Max((note.pos + note.width - 16.0f) / 16.0f,
                            HgtList[s + 1]);

                    if (note.pos > 24)
                    {
                        if (s - 1 >= 0 && s - 1 < HgtList.Length)
                            HgtList[s - 1] = Mathf.Max(
                                (note.pos + note.width - 16.0f) / 32.0f,
                                HgtList[s - 1]);
                        if (s + 2 >= 0 && s + 2 < HgtList.Length)
                            HgtList[s + 2] = Mathf.Max(
                                (note.pos + note.width - 16.0f) / 32.0f,
                                HgtList[s + 2]);
                    }
                }
            }
        }

        public void SetTimeToEnd(float value)
        {
            lastNoteTime = value;
            endTime = Math.Max(value + GDms, bgmManager.MainClip.length * 1000f - 1);
        }

        private TestifyAnomaly GenerateTestifyAnomaly()
        {
            TestifyAnomaly testifyAnomaly =
                JsonConvert.DeserializeObject<TestifyAnomaly>(
                    Resources.Load<TextAsset>("testify_shader_arguments").text);
            testifyAnomaly.args.Sort((a, b) => a.startTime - b.startTime);
            foreach (TestifyAnomalyArguments arg in testifyAnomaly.args)
            {
                arg.endTime = arg.startTime + arg.duration;
                arg.endStrength = arg.startStrength + arg.deltaStrength;
            }

            return testifyAnomaly;
        }

        private string ParseLagrangePace()
        {
            float maxScoreNow = 10000000f * PassedNotes / drbfile.TotalNotes;
            float scoreK = Score / maxScoreNow;
            if (good + miss == 0)
            {
                return $"PM +{GameUtil.ParseScore(PerfectJ, 6, true)}";
            }

            string rank;
            float basicScoreK;
            (rank, basicScoreK) = scoreK switch
            {
                >= 1.0f => ("PM", 1.0f),
                >= 0.99f => ("EX+", 0.99f),
                >= 0.965f => ("EX", 0.98f),
                >= 0.935f => ("AA", 0.95f),
                >= 0.905f => ("A", 0.92f),
                >= 0.875f => ("B", 0.89f),
                _ => ("C", 0.86f)
            };
            float deltaScore = Score - basicScoreK * maxScoreNow;
            return $"{rank}  {(deltaScore < 0 ? "" : "+")}{GameUtil.ParseScore(Mathf.FloorToInt(deltaScore), 6, true)}";
        }
    }

    internal class Item<T>
    {
        T[] item;

        //构造函数
        public Item(T[] obj)
        {
            item = new T[obj.Length];
            for (int i = 0; i < obj.Length; i++)
            {
                item[i] = obj[i];
            }
        }

        public T[] GetItems()
        {
            return item;
        }

        public void ShuffleItems()
        {
            //生成一个新数组：用于在之上计算和返回
            T[] temp;
            temp = new T[item.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = item[i];
            }

            //打乱数组中元素顺序
            for (int i = 0; i < temp.Length; i++)
            {
                int x, y;
                T t;
                x = TheGameManager.Random1.Next(0, temp.Length);
                do
                {
                    y = TheGameManager.Random1.Next(0, temp.Length);
                } while (y == x);

                t = temp[x];
                temp[x] = temp[y];
                temp[y] = t;
            }

            item = temp;
        }
    }
}