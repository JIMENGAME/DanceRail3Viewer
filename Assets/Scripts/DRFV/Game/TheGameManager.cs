using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DG.Tweening;
using DRFV.Data;
using DRFV.End;
using DRFV.Enums;
using DRFV.Game.HPBars;
using DRFV.Game.SceneControl;
using DRFV.Game.Side;
using DRFV.Global;
using DRFV.inokana;
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


        [SerializeField, Range(1, 30)] public int NoteSpeed = 12;
        [SerializeField] float NoteOffset; //[SerializeField, Range(-200, 200)]
        [SerializeField] public bool GameAuto = true, GameMirror;

        [SerializeField, Range(0, 10)]
        public int GameEffectGaterLevel = 8, GameEffectParamEQLevel = 8, GameEffectTap = 6;

        [SerializeField] GameComboDisplay playerGameComboDisplay = GameComboDisplay.COMBO;

        [SerializeField] public float PJms = 30.0f, PFms = 60.0f, GDms = 100.0f;

        public GameObject BeforeGameBackground;

        public int tapSize, freeFlickSize, flickSize, tapAlpha, freeFlickAlpha, flickAlpha;

        private GameObject songDataObject;
        public SongDataContainer songDataContainer;

        public GameObject autoPlayHint;
        public Text ProgramInfoTitle;

        public InputField inputJumpTo;

        public GameObject pauseUI;

        public GameSubJudgeDisplay gameSubJudgeDisplay;

        public TheLyricManager theLyricManager;

        public bool FCAPIndicator;

        public GameObject AegleseekerObject;

        public Sprite CoverPlaceholder;

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

        private AudioClip originalBGM;
        private bool saveAudio;
        public bool DebugMode;
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
        public AudioSource BGMManager;

        private AudioClip _acGameover;

        [SerializeField] // [SerializeField, HideInInspector] 
        public HPManager hpManager;

        [SerializeField] public SceneControlManager sceneControlManager;

        [SerializeField] // [SerializeField, HideInInspector] 
        MeshDrawer meshDrawer;

        [SerializeField] // [SerializeField, HideInInspector] 
        Sprite[] SpriteNoteLight, SpriteNotesDark;

        private Sprite[] SpriteNotes;

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

        [SerializeField] // [SerializeField, HideInInspector] 
        AudioMixer audioMixer;

        [SerializeField] // [SerializeField, HideInInspector] 
        Image HPMask;

        public Transform AnimationGradesParent;

        [SerializeField] // [SerializeField, HideInInspector] 
        GameObject[] AnimationGrades;

        public float SkillDamage = 1.0f;


        // [HideInInspector]
        public AnimationCurve BPMCurve, //ss to realtime s
            SCCurve; //realtime ms to drawtime

        // [HideInInspector]
        public double Distance;
        int CurrentSCn;
        float CurrentSC = 1.0f;

        // [HideInInspector] 
        public bool isPause = true;
        public GameObject jumpTo;


        public DRBFile drbfile;
        public int noteTotal;

        List<bool> isCreated = new List<bool>();
        int makenotestart;

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

        private SCORE_TYPE scoreType;

        private bool hasTextBeforeStart;

        public Text lyricText;

        private string customTierIdentifier = "Tier";
        private Color? customTierColor = null;
        private bool ratingPlus;

        void Start()
        {
            //メモリ解放
            Resources.UnloadUnusedAssets();
            VideoPlayer.targetTexture.Release();
            BeforeGameBackground.SetActive(true);

            SCUp = SCUpImage.DOFade(0, 0);
            SCDown = SCDownImage.DOFade(0, 0);

            speedLabelText = "更改谱面速度：";

            progressManager.Init(this);
            VideoPlayer.errorReceived += (_, _) =>
            {
                background.SetActive(true);
                VideoPlayer.transform.parent.gameObject.SetActive(false);
                videoPrepared = true;
            };
            VideoPlayer.prepareCompleted += _ => videoPrepared = true;
            StartCoroutine(ApplySettings());
        }

        private IEnumerator ApplySettings()
        {
            songDataObject = GameObject.FindWithTag("SongData");
#if UNITY_EDITOR
            DebugMode = songDataObject == null;
#endif
            if (DebugMode)
            {
                originalBGM = Resources.Load<AudioClip>("DEBUG/" + SongKeyword);
                if (sprSongImage) sprSongImage.sprite = Resources.Load<Sprite>("DEBUG/" + SongKeyword);
                tapSize = 2;
                tapAlpha = 3;
                flickSize = 3;
                flickAlpha = 3;
                freeFlickSize = 3;
                freeFlickAlpha = 3;
                VideoPlayer.playbackSpeed = BGMManager.pitch;
                if (SongKeyword.Equals("hello") && SongHard == 13)
                {
                    isHard = true;
                }

                hasVideo = !String.IsNullOrEmpty(VideoPlayer.url) || VideoPlayer.clip != null;
            }
            else if (songDataObject != null)
            {
                _currentSettings = GlobalSettings.CurrentSettings;
                songDataContainer = songDataObject.GetComponent<SongDataContainer>();
                SongKeyword = songDataContainer.songData.keyword;
                SongHard = songDataContainer.selectedDiff;
                SongTitle = songDataContainer.songData.songName;
                SongArtist = songDataContainer.songData.songArtist;
                NoteSpeed = songDataContainer.speed;
                NoteOffset = songDataContainer.offset;
                GameAuto = songDataContainer.isAuto;
                GameMirror = songDataContainer.isMirror;
                isHard = songDataContainer.isHard;
                skillcheck = songDataContainer.useSkillCheck;
                originalBGM = songDataContainer.music;
                tapSize = songDataContainer.tapSize;
                tapAlpha = songDataContainer.tapAlpha;
                flickSize = songDataContainer.flickSize;
                flickAlpha = songDataContainer.flickAlpha;
                freeFlickSize = songDataContainer.freeFlickSize;
                freeFlickAlpha = songDataContainer.freeFlickAlpha;
                GameEffectParamEQLevel = songDataContainer.GameEffectParamEQLevel;
                GameEffectGaterLevel = songDataContainer.GameEffectGaterLevel;
                GameEffectTap = songDataContainer.GameEffectTap;
                playerGameComboDisplay = songDataContainer.playerGameComboDisplay;
                gameSubJudgeDisplay = songDataContainer.gameSubJudgeDisplay;
                FCAPIndicator = songDataContainer.FCAPIndicator;
                saveAudio = songDataContainer.saveAudio;
                VideoPlayer.playbackSpeed = BGMManager.pitch = songDataContainer.songSpeed;
                barType = songDataContainer.barType;
                gameSide = songDataContainer.gameSide;
                NoteJudgeRange aaa = songDataContainer.NoteJudgeRange switch
                {
                    0 => new NoteJudgeRange { PJ = 70, P = 200, G = 400 },
                    1 => new NoteJudgeRange { PJ = 40, P = 80, G = 160 },
                    3 => new NoteJudgeRange { PJ = 25, P = 50, G = 100 },
                    4 => new NoteJudgeRange { PJ = 20, P = 40, G = 60 },
                    5 => new NoteJudgeRange { PJ = 10, P = 20, G = 30 },
                    6 => new NoteJudgeRange { PJ = 10, P = 10, G = 10 },
                    _ => new NoteJudgeRange { PJ = 30, P = 60, G = 100 }
                };
                PJms = aaa.PJ;
                PFms = aaa.P;
                GDms = aaa.G;
                if (SongKeyword.Equals("hello") && SongHard == 13)
                {
                    isHard = true;
                }

                if (sprSongImage)
                    sprSongImage.sprite =
                        songDataContainer.songData.cover ? songDataContainer.songData.cover : CoverPlaceholder;
                if (barType == BarType.HARD)
                    GameObject.FindWithTag("SongData").GetComponent<SongDataContainer>().isHard =
                        songDataContainer.isHard = true;
                else if (barType == BarType.EASY)
                    GameObject.FindWithTag("SongData").GetComponent<SongDataContainer>().isHard =
                        songDataContainer.isHard = false;
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
                        FadeManager.Instance.LoadScene("hadoutest");
                    }
                    else if (uwr.isDone)
                    {
                        originalBGM = ((DownloadHandlerAudioClip)uwr.downloadHandler).audioClip;
                    }
                }
            }
            else
            {
                NotificationBarManager.Instance.Show("你怎么进来的");
                LoadSelect();
                yield break;
            }

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
            SpriteNotes = gameSide switch
            {
                GameSide.DARK => SpriteNotesDark,
                GameSide.LIGHT => SpriteNoteLight,
                GameSide.COLORLESS => SpriteNoteLight,
                _ => SpriteNotesDark
            };
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


            if (!DebugMode && !storyMode && !hadouTest) hasVideo = ReadVideo();
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
                if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                    uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    NotificationBarManager.Instance.Show("谱找不到");
                    FadeManager.Instance.LoadScene("hadoutest");
                    yield break;
                }

                s = Encoding.UTF8.GetString(uwr.downloadHandler.data).Replace("\r", "").Replace("\t", "");
            }
            else if (storyMode)
            {
                s = Resources.Load<TextAsset>("STORY/SONGS/" + SongKeyword + "." + SongHard).text.Trim()
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
                drbfile.GenerateAttributesOnPlay();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                NotificationBarManager.Instance.Show($"错误：读取谱面时出错");
                FadeManager.Instance.LoadScene("select", _currentSettings);
                yield break;
            }

            md5 = drbfile.GetMD5();
            drbfile.notes.Sort((a, b) => Mathf.RoundToInt(a.time * 10000.0f - b.time * 10000.0f));

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

            BPMCurve = drbfile.BPMCurve;
            //SC
            SCCurve = drbfile.SCCurve;
            // 特判
            if (SongKeyword.Equals("hello") && SongHard == 13 && !hadouTest)
            {
                drbfile.notes.Clear();
                drbfile.noteWeightCount = 0;
                GenerateHelloWinnerNotes();
            }

            noteTotal = drbfile.notes.Count;

            //SCORE初期化
            SCORE_INIT();

            if (DebugMode && SongKeyword.Equals("aegleseeker"))
            {
                RegisterAegleseekerAnomaly();
            }

            if (storyMode && SongKeyword == "testify")
            {
                var (min, max, strength, count, arguments) = GenerateTestifyAnomalyCurve();
                if (postProcess) postProcess.Init(progressManager, min, max, strength, count, arguments);
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

            _acGameover = Resources.Load<AudioClip>("SE/gameover");
            yield return ProcessMusic();
            StartB();
        }

        private void StartB()
        {
            if (saveAudio && !DebugMode)
                SavWav.Save(StaticResources.Instance.dataPath + "songs/" + SongKeyword +
                            "/" + SongHard +
                            ".wav", BGMManager.clip);

            foreach (NoteData data in drbfile.notes)
            {
                timeList.Add(data.ms);
            }

            SetTimeToEnd(timeList.Count > 0
                ? timeList.OrderByDescending(time => time).ToList()[0]
                : BGMManager.clip.length * 1000f - 1);


            //复杂整理03：按节点计算front和back的判定范围

            for (int i = 0; i < drbfile.notes.Count; i++)
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
                            ? Resources.Load<TextAsset>($"STORY/SONGS/custom_mover.{SongKeyword}.{SongHard}").text
                            : File.ReadAllText(path));
                drbFile.GenerateAttributesOnPlay();

                CurveAndBool generatePositionCurve = GeneratePositionCurve(drbFile);
                PositionCurve = generatePositionCurve.curve;
                hasMover = generatePositionCurve.b;
            }
            else
            {
                CurveAndBool generatePositionCurve = GeneratePositionCurve(drbfile);
                PositionCurve = generatePositionCurve.curve;
                hasMover = generatePositionCurve.b;
            }

            if (!DebugMode && ((!storyMode && File.Exists(path =
                    StaticResources.Instance.dataPath + "songs/" + SongKeyword +
                    "/custom_height." + SongHard +
                    ".txt")) || (storyMode && hasCustomHeight)))
            {
                var drbFile =
                    DRBFile.Parse(
                        storyMode
                            ? Resources.Load<TextAsset>($"STORY/SONGS/custom_height.{SongKeyword}.{SongHard}").text
                            : File.ReadAllText(path));
                drbFile.GenerateAttributesOnPlay();

                GenerateHeightCurve(drbFile);
            }
            else if (!hasMover) GenerateHeightCurve(drbfile);

            //生成高度曲线
            for (int i = 0; i < 1000; i++)
            {
                HeightCurve.AddKey(i, HgtList[i]);
            }

            //仅保护TAP的判定范围
            foreach (var note in drbfile.notes.Where(note => NoteTypeJudge.IsTap(note.kind)))
            {
                foreach (var note1 in drbfile.notes)
                {
                    {
                        if (isCovering(note, note1) &&
                            Mathf.Abs(note1.ms - note.ms) < GDms * 2.0f)
                        {
                            if (note1.ms < note.ms)
                            {
                                note.isNear = true;
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
                objCombo[0].GetComponent<Text>().color = objCombo[3].GetComponent<Text>().color =
                    new Color(comboWhite[0], comboWhite[1], comboWhite[2], 0.0f);
                objCombo[1].GetComponent<Text>().color = new Color(comboRed[0], comboRed[1], comboRed[2], 0.0f);
                objCombo[2].GetComponent<Text>().color = new Color(comboBlue[0], comboBlue[1], comboBlue[2], 0.0f);
            }
            else if (FCAPIndicator)
            {
                objCombo[3].GetComponent<Text>().color =
                    objCombo[0].GetComponent<Text>().color = new Color(1.0f, 0.94f, 0.196f, 1.0f);
                objCombo[1].GetComponent<Text>().color = new Color(comboRed[0], comboRed[1], comboRed[2], 0.0f);
                objCombo[2].GetComponent<Text>().color = new Color(comboBlue[0], comboBlue[1], comboBlue[2], 0.0f);
            }
            else
            {
                objCombo[0].GetComponent<Text>().color = objCombo[3].GetComponent<Text>().color =
                    new Color(comboWhite[0], comboWhite[1], comboWhite[2], 1.0f);
                objCombo[1].GetComponent<Text>().color = new Color(comboRed[0], comboRed[1], comboRed[2], 1.0f);
                objCombo[2].GetComponent<Text>().color = new Color(comboBlue[0], comboBlue[1], comboBlue[2], 1.0f);
            }

            if (playerGameComboDisplay == GameComboDisplay.SCORE)
            {
                objCombo[0].GetComponent<Text>().text = "0";
                objCombo[1].GetComponent<Text>().text = "0";
                objCombo[2].GetComponent<Text>().text = "0";
                objCombo[3].GetComponent<Text>().text = "SCORE";
            }

            if (playerGameComboDisplay == GameComboDisplay.MSCORE)
            {
                string score = Util.ParseScore(Mathf.RoundToInt(MaxScore));
                objCombo[0].GetComponent<Text>().text = score;
                objCombo[1].GetComponent<Text>().text = score;
                objCombo[2].GetComponent<Text>().text = score;
                objCombo[3].GetComponent<Text>().text = "- SCORE";
            }

            if (playerGameComboDisplay == GameComboDisplay.ACCURACY)
            {
                objCombo[0].GetComponent<Text>().text = "0.00%";
                objCombo[1].GetComponent<Text>().text = "0.00%";
                objCombo[2].GetComponent<Text>().text = "0.00%";
                objCombo[3].GetComponent<Text>().text = "ACCURACY";
            }

            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            inited = true;

            OBSManager.Instance.StartRecording();
            progressManager.AddDelay(NoteOffset);
            progressManager.AddDelay(100f);
            progressManager.AddStartDelay(ReadyTime);
            yield return BeforeGameAnimation();
            if (hasTextBeforeStart) yield return ShowTextBeforeStart();
            progressManager.StartTiming();
            BGMManager.PlayScheduled(AudioSettings.dspTime + ReadyTime / 1000f);
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
            TextAsset textAsset = Resources.Load<TextAsset>($"STORY/SONGS/text_before_start.{SongKeyword}.{SongHard}");
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
                coverFrame.color = new Color(1f, 130f / 255f, 1f);
                cover.sprite = CoverPlaceholder;
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
            AudioClip _acHit = null, _acSlide = null, _acFlick = null;
            //获取效果音
            if (!DebugMode && !storyMode)
            {
                string songFolder = StaticResources.Instance.dataPath + "songs/" +
                                    SongKeyword + "/";
                if (File.Exists(songFolder + "hit.ogg"))
                {
                    using var uwr =
                        UnityWebRequestMultimedia.GetAudioClip("file://" + songFolder + "hit.ogg", AudioType.OGGVORBIS);
                    yield return uwr.SendWebRequest();
                    _acHit = uwr.isDone
                        ? DownloadHandlerAudioClip.GetContent(uwr)
                        : null;
                }
                else if (File.Exists(songFolder + "hit.wav"))
                {
                    using var uwr =
                        UnityWebRequestMultimedia.GetAudioClip("file://" + songFolder + "hit.wav", AudioType.WAV);
                    yield return uwr.SendWebRequest();
                    _acHit = uwr.isDone
                        ? DownloadHandlerAudioClip.GetContent(uwr)
                        : null;
                }

                if (File.Exists(songFolder + "slide.ogg"))
                {
                    using var uwr =
                        UnityWebRequestMultimedia.GetAudioClip("file://" + songFolder + "slide.ogg",
                            AudioType.OGGVORBIS);
                    yield return uwr.SendWebRequest();
                    _acSlide = uwr.isDone ? DownloadHandlerAudioClip.GetContent(uwr) : _acHit;
                }
                else if (File.Exists(songFolder + "slide.wav"))
                {
                    using var uwr =
                        UnityWebRequestMultimedia.GetAudioClip("file://" + songFolder + "slide.wav", AudioType.WAV);
                    yield return uwr.SendWebRequest();
                    _acSlide = uwr.isDone ? DownloadHandlerAudioClip.GetContent(uwr) : null;
                }

                if (File.Exists(songFolder + "flick.ogg"))
                {
                    using var uwr =
                        UnityWebRequestMultimedia.GetAudioClip("file://" + songFolder + "flick.ogg",
                            AudioType.OGGVORBIS);
                    yield return uwr.SendWebRequest();
                    _acFlick = uwr.isDone
                        ? DownloadHandlerAudioClip.GetContent(uwr)
                        : null;
                }
                else if (File.Exists(songFolder + "flick.wav"))
                {
                    using var uwr =
                        UnityWebRequestMultimedia.GetAudioClip("file://" + songFolder + "flick.wav", AudioType.WAV);
                    yield return uwr.SendWebRequest();
                    _acFlick = uwr.isDone
                        ? DownloadHandlerAudioClip.GetContent(uwr)
                        : null;
                }
            }

            if (_acHit == null)
            {
                _acHit = GetAudioClipFromRawWav("SE/hit", originalBGM.frequency, "hit");
                // _acHit = Resources.Load<AudioClip>("SE/hit");
            }

            if (_acSlide == null)
            {
                _acSlide = _acHit;
            }

            if (_acFlick == null)
            {
                _acFlick = GetAudioClipFromRawWav("SE/flick", originalBGM.frequency, "flick");
                // _acFlick = Resources.Load<AudioClip>("SE/flick");
            }

            //写入效果音A
            int bgmSamples = originalBGM.samples * originalBGM.channels;
            float[] f_hit = new float[_acHit.samples * _acHit.channels],
                f_slide = new float[_acSlide.samples * _acSlide.channels],
                f_flick = new float[_acFlick.samples * _acFlick.channels],
                f_song = new float[bgmSamples];
            originalBGM.GetData(f_song, 0);

            _acHit.GetData(f_hit, 0);
            _acSlide.GetData(f_slide, 0);
            _acFlick.GetData(f_flick, 0);

            //音量减半
            for (int i = 0; i < f_song.Length; i++)
            {
                f_song[i] *= volumeScale;
            }

            List<int> list_hit = new(), list_slide = new(), list_flick = new();

            //写入gater音
            if (GameEffectGaterLevel >= 1)
            {
                for (int i = 0; i < drbfile.notes.Count; i++)
                {
                    if (NoteTypeJudge.IsBitCrash(drbfile.notes[i].kind))
                    {
                        int end = (int)(bgmSamples *
                                        (drbfile.notes[i].ms / 1000.0f / originalBGM.length));
                        int start = (int)(bgmSamples *
                                          (drbfile.notes[drbfile.notes[i].parent].ms / 1000.0f / originalBGM.length));

                        if (end < bgmSamples)
                        {
                            for (int c = (end - start) / 2 + start; c < end; c++)
                            {
                                f_song[c] *= (10.0f - GameEffectGaterLevel) / 10.0f;
                            }
                        }
                    }
                }
            }

            if (GameEffectTap >= 1)
            {
                for (int i = 0; i < drbfile.notes.Count; i++)
                {
                    //写入tap音
                    if (NoteTypeJudge.IsTapSound(drbfile.notes[i].kind))
                    {
                        int start = (int)(bgmSamples *
                                          (drbfile.notes[i].ms / 1000.0f / originalBGM.length));

                        if (!list_hit.Contains(start))
                        {
                            list_hit.Add(start);
                            for (int c = 0; c < f_hit.Length; c++)
                            {
                                if (start + c < f_song.Length)
                                    f_song[start + c] += f_hit[c] * 0.5f * ((GameEffectTap + 3) / 10.0f);
                            }
                        }
                    }

                    if (NoteTypeJudge.IsSlideSound(drbfile.notes[i].kind))
                    {
                        int start = (int)(bgmSamples *
                                          (drbfile.notes[i].ms / 1000.0f / originalBGM.length));

                        if (!list_slide.Contains(start))
                        {
                            list_slide.Add(start);
                            for (int c = 0; c < f_slide.Length; c++)
                            {
                                if (start + c < f_song.Length)
                                    f_song[start + c] += f_slide[c] * 0.5f * ((GameEffectTap + 3) / 10.0f);
                            }
                        }
                    }

                    //写入flick音
                    if (NoteTypeJudge.IsFlick(drbfile.notes[i].kind))
                    {
                        int start = (int)(bgmSamples *
                                          (drbfile.notes[i].ms / 1000.0f / originalBGM.length));

                        if (!list_flick.Contains(start))
                        {
                            list_flick.Add(start);
                            for (int c = 0; c < f_flick.Length; c++)
                            {
                                if (start + c < f_song.Length)
                                    f_song[start + c] += f_flick[c] * 0.5f * ((GameEffectTap + 3) / 10.0f);
                            }
                        }
                    }
                }
            }

            BGMManager.clip = AudioClip.Create("",
                originalBGM.samples + (int)MathF.Max(_acHit.samples, Mathf.Max(_acSlide.samples, _acFlick.samples)),
                originalBGM.channels, originalBGM.frequency, false);
            BGMManager.clip.SetData(f_song, 0);
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
                            nsc = NoteData.NoteSC.GetCommonNSC()
                        };
                        note.parent = 0;
                        note.mode = NoteAppearMode.None;
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
                questionComp.Init(this, BPMCurve.Evaluate(16 + j * 4), BPMCurve.Evaluate(16 + j * 4 + 3), question,
                    choices);
                j++;
            }
        }

        public GameObject questionPrefab;
        public Transform canvasNormal;

        private List<float> timeList = new();
        public float lastNoteTime;

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

        // Update is called once per frame
        void Update()
        {
            if (!inited || ended) return;

            if (progressManager.NowTime > lastNoteTime)
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
                    pos.y -= (pos.y - (9.0f + 18.0f * HeightCurve.Evaluate(BGMManager.time))) * 20.0f * Time.deltaTime;
                    pos.z -= (pos.z - (-7.0f - 14.0f * HeightCurve.Evaluate(BGMManager.time))) * 20.0f * Time.deltaTime;
                    pos.x -= (pos.x - PositionCurve.Evaluate(BGMManager.time)) * 30.0f * Time.deltaTime;
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
                if (progressManager.NowTime + 123.4f > BPMCurve.Evaluate(drbfile.scs[i].sci))
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
            if (sliderMusicTime) sliderMusicTime.value = BGMManager.time / BGMManager.clip.length;

            //終了処理
            //なし

            //ノーツ創り出す 1 Frame Max 5 Notes
            int count = 0;
            for (int i = makenotestart; i < drbfile.notes.Count; i++)
            {
                if (!isCreated[i])
                {
                    if ((0.01f * ((NoteTypeJudge.IsTail(drbfile.notes[i].kind)
                                      ? drbfile.notes[i].parent_dms
                                      : drbfile.notes[i].dms) -
                                  Distance) * drbfile.notes[i].nsc.value * NoteSpeed < 150.0f)
                        || (drbfile.notes[i].ms - progressManager.NowTime < 1000)
                        || ((drbfile.notes[i].nsc.type == NoteSCType.MULTI || drbfile.notes[i].mode == NoteAppearMode.Jump) && drbfile.notes[i].ms - progressManager.NowTime < 10000.0f))
                    {
                        GameObject note = Instantiate(NotePrefab,
                            NoteTypeJudge.IsTap(drbfile.notes[i].kind) ? notesUp.transform : notesDown.transform);
                        note.GetComponent<SpriteRenderer>().sprite = SpriteNotes[(int)drbfile.notes[i].kind];
                        if (NoteTypeJudge.IsTap(drbfile.notes[i].kind))
                            note.GetComponent<SpriteRenderer>().sortingOrder = 2;
                        else if (NoteTypeJudge.IsFlick(drbfile.notes[i].kind))
                            note.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        note.GetComponent<TheNote>().mDrawer = meshDrawer;
                        note.GetComponent<TheNote>().SetGMIMG(this, inputManager);
                        if (NoteTypeJudge.IsTail(drbfile.notes[i].kind))
                        {
                            note.GetComponent<TheNote>().Ready(
                                drbfile.notes[i].id,
                                drbfile.notes[i].time,
                                drbfile.notes[i].pos,
                                drbfile.notes[i].pos + drbfile.notes[i].width * 0.5f,
                                drbfile.notes[i].mode,
                                drbfile.notes[i].nsc,
                                drbfile.notes[i].ms,
                                drbfile.notes[i].dms,
                                drbfile.notes[i].width,
                                drbfile.notes[i].kind,
                                drbfile.notes[i].isNear,
                                tapSize, flickSize, freeFlickSize, tapAlpha, flickAlpha, freeFlickAlpha,
                                drbfile.notes[i].parent,
                                drbfile.notes[i].parent_ms,
                                drbfile.notes[i].parent_dms,
                                drbfile.notes[i].parent_pos,
                                drbfile.notes[i].parent_width
                            );
                        }
                        else
                        {
                            note.GetComponent<TheNote>().Ready(
                                drbfile.notes[i].id,
                                drbfile.notes[i].time,
                                drbfile.notes[i].pos,
                                drbfile.notes[i].pos + drbfile.notes[i].width * 0.5f,
                                drbfile.notes[i].mode,
                                drbfile.notes[i].nsc,
                                drbfile.notes[i].ms,
                                drbfile.notes[i].dms,
                                drbfile.notes[i].width,
                                drbfile.notes[i].kind,
                                drbfile.notes[i].isNear,
                                tapSize, flickSize, freeFlickSize, tapAlpha, flickAlpha, freeFlickAlpha
                            );
                        }

                        note.GetComponent<TheNote>().StartC();


                        isCreated[i] = true;
                        count++; //Max 5 notes

                        for (int ii = 0; ii < drbfile.notes.Count; ii++)
                        {
                            if (!isCreated[ii])
                            {
                                makenotestart = ii;
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
                if (!storyMode) LoadSelect();
                else LoadStory();
                return;
            }

            if (storyMode)
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

                LoadStory();
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
                DontDestroyOnLoad(go);
                FadeManager.Instance.LoadScene("result", _currentSettings);
            }
        }

        void LoadSelect()
        {
            CheckDataContainers.CleanSongDataContainer();
            FadeManager.Instance.LoadScene("select", _currentSettings);
        }

        void LoadStory()
        {
            CheckDataContainers.CleanSongDataContainer();
            FadeManager.Instance.LoadScene("story", _currentSettings);
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
                Resources.Load<AudioClip>($"STORY/SONGS/{storyChallengeContainer.songData.keyword}");
            storyChallengeContainer.songData.cover =
                Resources.Load<Sprite>("STORY/SONGS/" + storyChallengeContainer.songData.keyword);
            storyChallengeContainer.isComputer = Application.platform switch
            {
                RuntimePlatform.WindowsEditor => true,
                RuntimePlatform.WindowsPlayer => true,
                RuntimePlatform.LinuxEditor => true,
                RuntimePlatform.LinuxPlayer => true,
                _ => false
            };
            storyChallengeContainer.isAuto = storyChallengeContainer.isComputer;
            storyChallengeContainer.isMirror = false;
            storyChallengeContainer.isHard = true;
            storyChallengeContainer.useSkillCheck = false;
            storyChallengeContainer.selectedDiff = storyChallengeContainer.songData.hards[0];
            storyChallengeContainer.saveAudio = false;
            storyChallengeContainer.barType = BarType.HARD;
            storyChallengeContainer.songSpeed = 1.0f;
            storyChallengeContainer.NoteJudgeRange = 3;
            storyChallengeContainer.NoteJugeRangeLabel = "";
            storyChallengeContainer.unlock = unlock;
            storyChallengeContainer.gameSide = gameSide;
            storyChallengeContainer.speed = NoteSpeed;
            storyChallengeContainer.offset = NoteOffset;
            storyChallengeContainer.tapSize = tapSize;
            storyChallengeContainer.tapAlpha = tapAlpha;
            storyChallengeContainer.flickSize = flickSize;
            storyChallengeContainer.flickAlpha = flickAlpha;
            storyChallengeContainer.freeFlickSize = freeFlickSize;
            storyChallengeContainer.freeFlickAlpha = freeFlickAlpha;
            storyChallengeContainer.GameEffectParamEQLevel = GameEffectParamEQLevel;
            storyChallengeContainer.GameEffectGaterLevel = GameEffectGaterLevel;
            storyChallengeContainer.GameEffectTap = GameEffectTap;
            storyChallengeContainer.playerGameComboDisplay = playerGameComboDisplay;
            storyChallengeContainer.gameSubJudgeDisplay = gameSubJudgeDisplay;
            storyChallengeContainer.FCAPIndicator = FCAPIndicator;
            storyChallengeContainer.hasTextBeforeStart = false;
            storyChallengeContainer.tierIdentifier = customTierIdentifier;
            storyChallengeContainer.customTierColor = customTierColor;
            storyChallengeContainer.ratingPlus = ratingPlus;
            DontDestroyOnLoad(storyChallengeContainer);
            FadeManager.Instance.LoadScene("game", _currentSettings);
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
                BGMManager.volume -= 0.1f;
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
            FadeManager.Instance.LoadScene("game", _currentSettings);
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


            BGMManager.DOFade(0, 1).SetEase(Ease.Linear);
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

            BGMManager.Pause();
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
                BGMManager.panStereo = 0;
            }
            else
            {
                float result = SteroList.Average();
                if (result > 1) result = 1;
                if (result < -1) result = -1;
                BGMManager.panStereo = result;
            }

            audioMixer.SetFloat("Center", EQCurve.Evaluate(AudioMixerCenter));
            audioMixer.SetFloat("Freq", AudioMixerFreq);
            audioMixer.SetFloat("HPFreq", EQCurve.Evaluate(AudioMixerHiPass));
            audioMixer.SetFloat("LPFreq", EQCurve.Evaluate(AudioMixerLoPass));

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
                time = BPMCurve.Evaluate(float.Parse(inputJumpTo.text)) * BGMManager.pitch;
                if (BGMManager.clip.length < time / 1000)
                {
                    time = BGMManager.clip.length * 1000 - 1;
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
            for (int i = makenotestart; i < drbfile.notes.Count; i++)
            {
                if (from > time) break;
                if (!isCreated[i])
                {
                    if (drbfile.notes[i].ms < time && drbfile.notes[i].ms >= from)
                    {
                        JudgeDirectly(drbfile.notes[i].kind);

                        isCreated[i] = true;

                        for (int ii = 0; ii < drbfile.notes.Count; ii++)
                        {
                            if (!isCreated[ii])
                            {
                                makenotestart = ii;
                                break;
                            }
                        }
                    }
                }
            }

            progressManager.AddDelay(from - time);
            Distance = SCCurve.Evaluate(time);
            BGMManager.time = time / 1000;
            if (hasVideo) VideoPlayer.time = time / 1000;
            theLyricManager.JumpTo(time);

            progressManager.ContinueTiming();
            isPause = false;
            BGMManager.UnPause();
            pauseUI.SetActive(false);
        }

        private void OnApplicationPause(bool focus)
        {
#if UNITY_EDITOR
#else
        if (inited && !isPause) Pause();
#endif
        }

        private void OnApplicationFocus(bool focus)
        {
#if UNITY_EDITOR
#else
        if (inited && !isPause) Pause();
#endif
        }

        public GameObject changeSpeed;
        public Text speedLabel;
        public Slider slider;
        public string speedLabelText;

        public void Pause()
        {
            if (!pauseable || BGMManager.time <= 0) return;
            isPause = true;
            BGMManager.Pause();
            VideoPlayer.Pause();
            progressManager.StopTiming();
            pauseUI.SetActive(true);
            jumpTo.SetActive(GameAuto);
            changeSpeed.SetActive(!GameAuto);
            VideoMask.DOKill();
            if (!GameAuto)
            {
                speedLabel.text = speedLabelText + (NoteSpeed / 2f).ToString("N1") + "x";
            }
        }

        public void UpdateNoteSpeed()
        {
            _currentSettings.NoteSpeed = NoteSpeed = (int)slider.value;
            speedLabel.text = speedLabelText + (NoteSpeed / 2f).ToString("N1") + "x";
        }

        public void Retry()
        {
            if (!pauseable || BGMManager.time <= 0) return;
            FadeManager.Instance.LoadScene("game", _currentSettings);
        }

        public void Resume()
        {
            if (!pauseable || BGMManager.time <= 0) return;
            isPause = false;
            pauseUI.SetActive(false);
            TimeGoBack(3000f);
        }

        public void Quit()
        {
            if (!pauseable || BGMManager.time <= 0) return;
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
            if (!DebugMode) LoadSelect();
            else Application.Quit();
        }

        void TimeGoBack(float delta)
        {
            StartCoroutine(TimeGoBackCoroutine(delta));
        }

        IEnumerator TimeGoBackCoroutine(float delta)
        {
            float from = progressManager.NowTime + NoteOffset + 100f;
            float to = progressManager.NowTime + NoteOffset + 100f - delta * BGMManager.pitch;
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

            if (BGMManager.time < delta * BGMManager.pitch / 1000f)
            {
                delta = BGMManager.time * 1000f / BGMManager.pitch;
                BGMManager.time = 0;
                if (hasVideo) VideoPlayer.time = 0;
                progressManager.AddDelay(delta);
            }
            else
            {
                if (BGMManager.clip.length > to / 1000f) BGMManager.time = to / 1000f;
                if (hasVideo && VideoPlayer.length > to / 1000f) VideoPlayer.time = to / 1000f;
                progressManager.AddDelay(delta);
            }

            for (int i = 0; i < isCreated.Count; i++)
            {
                if (drbfile.notes[i].ms < from)
                {
                    isCreated[i] = true;
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
            BGMManager.UnPause();
            if (hasVideo) VideoPlayer.Play();

            BGMManager.volume = 0;
            int subdivision = Mathf.FloorToInt(delta / 100f);
            if (subdivision <= 0)
            {
                BGMManager.volume = 1;
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
                BGMManager.volume = i * 1.0f / subdivision;
                yield return qwq;
            }
        }

        void SCORE_INIT()
        {
            scoreType = GlobalSettings.CurrentSettings.ScoreType;
            Score = 0;
            LiLunZhi = MaxScore = scoreType switch
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
            AccMSList.Clear();
        }

        private void JudgeDirectly(NoteKind kind)
        {
            PerfectJ++;
            AddCombo();
            //if (imgHanteiBeam[0]) imgHanteiBeam[0].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            hpManager.InCreaseHp(hpManager.HpBar.PerfectJHP(kind, drbfile.noteWeightCount, this));
        }

        //判定処理
        public void Judge(float ms, NoteKind kind, Vector3 pos, float width)
        {
            //LISTに登録


            var go2 = Instantiate(HanteiPrefab, pos, Quaternion.identity);
            GameObject go;

            //PERFECT JUSTICE 判定
            if (Mathf.Abs(ms) <= PJms)
            {
                PerfectJ++;
                AddCombo();
                go = Instantiate(prefabEffect[(int)kind], pos, Quaternion.identity);

                if (gameSubJudgeDisplay == GameSubJudgeDisplay.MS && kind == NoteKind.TAP)
                    go2.GetComponent<JudgeImage>().Init(4, true, (int)ms);
                else go2.GetComponent<JudgeImage>().Init(4);
                if (imgJudgeBeam[0])
                    imgJudgeBeam[0].color = new Color(imgJudgeBeam[0].color.r, imgJudgeBeam[0].color.g,
                        imgJudgeBeam[0].color.b, 1.0f);
                //HP処理
                hpManager.InCreaseHp(hpManager.HpBar.PerfectJHP(kind, drbfile.noteWeightCount, this));
            }
            //PERFECT 判定
            else if (Mathf.Abs(ms) <= PFms)
            {
                Perfect++;
                AddCombo();
                FastOrSlow(ms);
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
                        go2.GetComponent<JudgeImage>().Init(3, true, (int)ms, ms >= 0 ? 2 : 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (imgJudgeBeam[1])
                    imgJudgeBeam[1].color = new Color(imgJudgeBeam[1].color.r, imgJudgeBeam[1].color.g,
                        imgJudgeBeam[1].color.b, 1.0f);
                //HP処理
                hpManager.InCreaseHp(hpManager.HpBar.PerfectHP(kind, drbfile.noteWeightCount, this));
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
                good++;
                AddCombo();
                FastOrSlow(ms);
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
                        go2.GetComponent<JudgeImage>().Init(2, true, (int)ms, ms >= 0 ? 2 : 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (imgJudgeBeam[2])
                    imgJudgeBeam[2].color = new Color(imgJudgeBeam[2].color.r, imgJudgeBeam[2].color.g,
                        imgJudgeBeam[2].color.b, 1.0f);
                //HP処理
                hpManager.DecreaseHp(hpManager.HpBar.GoodHP(kind, drbfile.noteWeightCount, this));
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
                miss++;
                if (Combo >= 30)
                {
                    //血が出る警告
                    ShowHPMask();
                }

                Combo = 0;
                ReflashCombo();
                go = null;
                go2.GetComponent<JudgeImage>().Init(1);
                if (imgJudgeBeam[3])
                    imgJudgeBeam[3].color = new Color(imgJudgeBeam[3].color.r, imgJudgeBeam[3].color.g,
                        imgJudgeBeam[3].color.b, 1.0f);
                //HP処理
                hpManager.DecreaseHp(hpManager.HpBar.MissHP(kind, drbfile.noteWeightCount, this));
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
            switch (scoreType)
            {
                case SCORE_TYPE.ORIGINAL:
                    Score = 3000000.0f * (PerfectJ * 1.0f + Perfect * 0.99f + good / 3.0f) / noteTotal;
                    MaxScore = 3000000.0f -
                               3000000.0f * (Perfect * 0.01f + good / 3.0f * 2.0f + miss * 1.0f) / noteTotal;
                    Accuracy = AccMSList.Count > 0 ? AccMSList.Average() : 0.0f;
                    break;
                case SCORE_TYPE.ARCAEA:
                    Score = 10000000.0f * (PerfectJ + Perfect + good * 0.5f) / noteTotal + PerfectJ;
                    MaxScore = 10000000.0f - 10000000.0f * (good * 0.5f + miss) / noteTotal +
                               (noteTotal - Perfect - good - miss);
                    Accuracy = AccMSList.Count > 0 ? AccMSList.Average() : 0.0f;
                    break;
                case SCORE_TYPE.PHIGROS:
                    Score = 900000.0f * (PerfectJ + Perfect + good * 0.65f) / noteTotal +
                            MaxCombo * 100000f / noteTotal;
                    MaxScore = 1000000.0f - 900000.0f * (miss + good * 0.35f) / noteTotal -
                               100000.0f * (PerfectJ + Perfect + good + miss - MaxCombo) / noteTotal;
                    Accuracy = 100.0f * (PerfectJ + Perfect + good + miss == 0
                        ? 0.0f
                        : (PerfectJ + Perfect + good * 0.65f) / (PerfectJ + Perfect + good + miss));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            if (textScore) textScore.text = Util.ParseScore(Mathf.RoundToInt(Score), scoreType);
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
                    objCombo[0].GetComponent<Text>().color = objCombo[3].GetComponent<Text>().color =
                        new Color(comboWhite[0], comboWhite[1], comboWhite[2], 0.0f);
                    objCombo[1].GetComponent<Text>().color = new Color(comboRed[0], comboRed[1], comboRed[2], 0.0f);
                    objCombo[2].GetComponent<Text>().color = new Color(comboBlue[0], comboBlue[1], comboBlue[2], 0.0f);
                }
                else
                {
                    if (miss > 0 || !FCAPIndicator)
                    {
                        objCombo[0].GetComponent<Text>().color = objCombo[3].GetComponent<Text>().color =
                            new Color(comboWhite[0], comboWhite[1], comboWhite[2], 1.0f);
                        objCombo[1].GetComponent<Text>().color = new Color(comboRed[0], comboRed[1], comboRed[2], 1.0f);
                        objCombo[2].GetComponent<Text>().color =
                            new Color(comboBlue[0], comboBlue[1], comboBlue[2], 1.0f);
                    }
                    else if (good > 0)
                    {
                        objCombo[3].GetComponent<Text>().color =
                            objCombo[0].GetComponent<Text>().color = new Color(0f, 1.0f, 2f / 15f, 1.0f);
                        objCombo[1].GetComponent<Text>().color = new Color(comboRed[0], comboRed[1], comboRed[2], 1.0f);
                        objCombo[2].GetComponent<Text>().color =
                            new Color(comboBlue[0], comboBlue[1], comboBlue[2], 1.0f);
                    }
                    else if (Perfect > 0)
                    {
                        objCombo[3].GetComponent<Text>().color =
                            objCombo[0].GetComponent<Text>().color = new Color(1.0f, 0.5f, 0.196f, 1.0f);
                        objCombo[1].GetComponent<Text>().color = new Color(comboRed[0], comboRed[1], comboRed[2], 1.0f);
                        objCombo[2].GetComponent<Text>().color =
                            new Color(comboBlue[0], comboBlue[1], comboBlue[2], 1.0f);
                    }
                    else
                    {
                        objCombo[3].GetComponent<Text>().color =
                            objCombo[0].GetComponent<Text>().color = new Color(1.0f, 0.94f, 0.196f, 1.0f);
                        objCombo[1].GetComponent<Text>().color = new Color(comboRed[0], comboRed[1], comboRed[2], 1.0f);
                        objCombo[2].GetComponent<Text>().color =
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
                        disp = scoreType == SCORE_TYPE.PHIGROS ? "N/A" : Mathf.RoundToInt(MaxScore) + "";
                    }
                    else if (playerGameComboDisplay == GameComboDisplay.ACCURACY)
                    {
                        disp = Accuracy.ToString("0.00") + "%";
                    }

                    objCombo[0].GetComponent<Text>().text = disp;
                    objCombo[1].GetComponent<Text>().text = disp;
                    objCombo[2].GetComponent<Text>().text = disp;

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
        private GlobalSettings _currentSettings;

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

        private AudioClip GetAudioClipFromRawWav(string path, int frequency = -1, string clipName = "", int bit = 16)
        {
            RawWav rawWav = Resources.Load<RawWav>(path);
            if (rawWav == null) return null;
            WAV wav = new WAV(rawWav.data);
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (frequency > 0 && frequency != wav.Frequency)
            {
                return WavConverter.ConvertWavToAudioClip(rawWav.data, frequency, bit, wav.ChannelCount, clipName);
            }
#endif

            return wav.ToAudioClip();
        }

        private CurveAndBool GeneratePositionCurve(DRBFile drbfile)
        {
            List<float> existedTime = new List<float>();
            List<Keyframe> kfpos = new List<Keyframe>();
            kfpos.Add(new Keyframe(0, 0));
            float currentp = 0.0f;
            for (var i = 0; i < drbfile.notes.Count; i++)
            {
                var note = drbfile.notes[i];
                if (note.kind == NoteKind.MOVER_CENTER || note.kind == NoteKind.MOVER_END)
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
            return new CurveAndBool(new AnimationCurve(kfposa), kfpos.Count > 1);
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
            endTime = Math.Max(lastNoteTime + GDms, BGMManager.clip.length * 1000f - 1);
        }

        private (float, float, float, int, TestifyAnomalyArguments[]) GenerateTestifyAnomalyCurve()
        {
            TestifyAnomaly testifyAnomaly =
                JsonConvert.DeserializeObject<TestifyAnomaly>(
                    Resources.Load<TextAsset>("testify_shader_arguments").text);
            var testifyAnomalyArgumentsArray = testifyAnomaly.args.ToList();
            testifyAnomalyArgumentsArray.Sort((a, b) => a.startTime - b.startTime);
            return (testifyAnomaly.minEffect, testifyAnomaly.maxEffect, testifyAnomaly.strength,
                testifyAnomaly.sampleCount, testifyAnomalyArgumentsArray.ToArray());
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

class CurveAndBool
{
    public AnimationCurve curve;
    public bool b;

    public CurveAndBool(AnimationCurve curve, bool b)
    {
        this.curve = curve;
        this.b = b;
    }
}