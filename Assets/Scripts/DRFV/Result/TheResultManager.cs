using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using DRFV.Data;
using DRFV.Enums;
using DRFV.Global;
using DRFV.Global.Managers;
using DRFV.Global.Utilities;
using DRFV.inokana;
using DRFV.Login;
using DRFV.Select;
using DRFV.Setting;
using LeTai.TrueShadow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Math = System.Math;

namespace DRFV.Result
{
    public class TheResultManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI score;
        [SerializeField] private TextMeshProUGUI Tier;
        [SerializeField] private Image TierBackground;

        [SerializeField] private TextMeshProUGUI PerfectJ;
        [SerializeField] private TextMeshProUGUI Perfect;
        [SerializeField] private TextMeshProUGUI Good;
        [SerializeField] private TextMeshProUGUI Miss;
        [SerializeField] private TextMeshProUGUI Fast;
        [SerializeField] private TextMeshProUGUI Slow;
        [SerializeField] private TextMeshProUGUI MaxCombo;
        [SerializeField] private TextMeshProUGUI scoreDelta;
        [SerializeField] private GameObject tNewScore;
        [SerializeField] private GameObject skillCheckIndicator;
        [SerializeField] private GameObject mirrorIndicator;
        [SerializeField] private GameObject hardIndicator;
        [SerializeField] private GameObject easyIndicator;
        [SerializeField] private GameObject hardbarIndicator;
        [SerializeField] private GameObject judgeRangeFixInficator;
        [SerializeField] private GameObject noModImage;

        [SerializeField] private Text endType;
        [SerializeField] private TrueShadow endTypeShadow;

        [SerializeField] private Image CoverBackground;
        [SerializeField] private Image Cover;
        [SerializeField] private Text Title;

        [SerializeField] private Text Artist;

        [SerializeField] private Text HPAcc;

        [SerializeField] private Text SongSpeed;

        [SerializeField] private Text JudgeInput;

        [SerializeField] private GameObject AccDetailsPanel;
        [SerializeField] private MSDetailsDrawer MSDetailsDrawer;

        [SerializeField] private TextMeshProUGUI TMPRank;

        [SerializeField] private TMP_ColorGradient[] TCGRank;
        [SerializeField] private TMP_ColorGradient TCGError;

        [SerializeField] private float accAnimTimeStart, accAnimTimeEnd;
        [SerializeField] private Ease accAnimEase;

        private static Color orange = new(1f, 0.5f, 0.5f),
            greyShadow = new(0.9f, 0.9f, 0.9f, 0.6f),
            greenShadow = new(0f, 0.3f, 0f, 0.6f),
            orangeShadow = new(0.3f, 0.15f, 0.15f, 0.6f),
            yellowShadow = new(0.3f, 0.2764706f, 0.004705882f, 0.6f);

#if UNITY_EDITOR
        private bool debugMode = false;
#endif
        private GlobalSettings _globalSettings;

        // Start is called before the first frame update
        void Start()
        {
            _globalSettings = GlobalSettings.CurrentSettings;
            float originalRate = _globalSettings.PlayerRating.GetRate();
            AccountInfo.Instance.UpdateAccountPanel();
            SongDataContainer songDataContainer = GameObject.FindWithTag("SongData").GetComponent<SongDataContainer>();
            ResultDataContainer resultDataContainer =
                GameObject.FindWithTag("ResultData").GetComponent<ResultDataContainer>();
#if UNITY_EDITOR
            if (resultDataContainer.endType == EndType.AUTO_PLAY) resultDataContainer.endType = EndType.ALL_PERFECT;
            if (songDataContainer.songData == null)
            {
                songDataContainer.songData = new TheSelectManager.SongData
                {
                    songName = "44Gu44GE44KN",
                    songArtist = "44G644G944KI",
                    cover = Resources.Load<Sprite>("DemoAvatar")
                };
                debugMode = true;
                Util.Init();
            }
#endif
            float songSpeed = Util.TransformSongSpeed(_globalSettings.SongSpeed);
            BarType hpBarType = (BarType) _globalSettings.HPBarType;
            bool enableJudgeRangeFix = _globalSettings.enableJudgeRangeFix &&
                                       Math.Abs(songSpeed - 1.0f) > 0.1f;
            bool isHadouTest = songDataContainer.GetContainerType() == SongDataContainerType.HADOU_TEST;
            float realScore = _globalSettings.ScoreType != SCORE_TYPE.ORIGINAL
                ? (3000000.0f *
                    (resultDataContainer.PERFECT_J + resultDataContainer.PERFECT * 0.99f +
                     resultDataContainer.GOOD / 3f) / resultDataContainer.noteTotal)
                : resultDataContainer.SCORE;
            int thisScore = Mathf.RoundToInt(realScore);
            bool isValid =
                resultDataContainer.PERFECT_J + resultDataContainer.PERFECT + resultDataContainer.GOOD +
                resultDataContainer.MISS ==
                resultDataContainer.noteTotal;
            Grade rank = Util.ScoreToRank(thisScore);
            bool scoreError = rank == Grade.ERR;
            TMPRank.colorGradientPreset = rank > 0 ? TCGRank[(int) rank] : TCGError;
            TMPRank.text = RankToRankText((int)rank);
            if (!isValid || scoreError)
            {
                if (!isValid)
                {
                    NotificationBarManager.Instance.Show("警告：游玩成绩Note数量之和不等于谱面物量，成绩作废");
                }
                else if (scoreError)
                {
                    NotificationBarManager.Instance.Show("警告：分数异常，成绩作废");
                }

                scoreDelta.text = "Score Error";
                tNewScore.SetActive(false);
            }
            else if (resultDataContainer.endType != EndType.AUTO_PLAY &&
                     !_globalSettings.SkillCheckMode &&
                     hpBarType != BarType.EASY && (isHadouTest || _globalSettings.NoteJudgeRange >= GameUtil.NoteJudgeRangeLimit))
            {
                string key = "SongScore_" +
                             resultDataContainer.md5;
                bool newRecord = true;
                ResultData resultDataOld;
                if (PlayerPrefs.HasKey(key))
                {
                    resultDataOld = JsonConvert.DeserializeObject<ResultData>(PlayerPrefs.GetString(key));
                    if (resultDataOld == null) throw new ArgumentException();
                    newRecord = thisScore > resultDataOld.score;
                }
                else
                {
                    resultDataOld = new ResultData();
                }

                if (_globalSettings.ScoreType != SCORE_TYPE.ORIGINAL)
                {
                    scoreDelta.text = "Non-origin score";
                    tNewScore.SetActive(false);
                }
                else
                {
                    scoreDelta.text = GameUtil.ParseScore(resultDataOld.score, null, true) + "  " +
                                      (resultDataOld.score <= thisScore ? "+" : "") +
                                      GameUtil.ParseScore(thisScore - resultDataOld.score, null, true);
                    tNewScore.SetActive(newRecord);
                }
                
                ResultData resultData = new ResultData
                {
                    score = thisScore,
                    endType = resultDataContainer.endType switch
                    {
                        EndType.FAILED => "fd",
                        EndType.COMPLETED => "cp",
                        EndType.FULL_COMBO => "fc",
                        EndType.ALL_PERFECT => resultDataContainer.PERFECT == 0 ? "apj" : "ap",
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    perfectJ = resultDataContainer.PERFECT_J,
                    perfect = resultDataContainer.PERFECT,
                    good = resultDataContainer.GOOD,
                    miss = resultDataContainer.MISS,
                    fast = resultDataContainer.FAST,
                    slow = resultDataContainer.SLOW,
                    hp = MathF.Round(resultDataContainer.hp, 2, MidpointRounding.AwayFromZero),
                    acc = MathF.Round(resultDataContainer.Accuracy, 2, MidpointRounding.AwayFromZero),
                    rate = Util.ScoreToRate(realScore, songDataContainer.selectedDiff, songSpeed)
                };
                _globalSettings.PlayerRating.PopResult(resultData);
                GlobalSettings.CurrentSettings = _globalSettings;
                GlobalSettings.Save();

                if (newRecord)
                {
                    string resultStr = JsonConvert.SerializeObject(resultData, Formatting.None);
                    if (!isHadouTest)
                        UploadScore(key, resultStr);

                    PlayerPrefs.SetString(key, resultStr);
                    PlayerPrefs.Save();
                }
            }
            else
            {
                scoreDelta.text = "unrecorded score";
                tNewScore.SetActive(false);
            }

            Tier.text = "Tier " + songDataContainer.selectedDiff;
            TierBackground.color = Tier.color = Util.GetTierColor(songDataContainer.selectedDiff);
            if (songDataContainer.selectedDiff > 20 || songDataContainer.selectedDiff < 0)
            {
                Tier.outlineColor = new Color(1, 1, 1);
            }
            else
            {
                Tier.outlineColor = new Color(0, 0, 0);
            }

            if (songDataContainer.songData.cover != null)
            {
                CoverBackground.color =
                    resultDataContainer.bgColor ?? Util.GetAvgColor(songDataContainer.songData.cover);
                Cover.sprite = songDataContainer.songData.cover;
            }
            else
            {
                CoverBackground.color = Util.SpritePlaceholderBGColor;
                Cover.sprite = Util.SpritePlaceholder;
            }

            Title.text = songDataContainer.songData.songName;
            Artist.text = songDataContainer.songData.songArtist;
            score.text = GameUtil.ParseScore(Mathf.RoundToInt(resultDataContainer.SCORE));
            PerfectJ.text = resultDataContainer.PERFECT_J + "";
            Perfect.text = resultDataContainer.PERFECT + "";
            Good.text = resultDataContainer.GOOD + "";
            Miss.text = resultDataContainer.MISS + "";
            Fast.text = "FAST" + resultDataContainer.FAST.ToString("0000") + "";
            Slow.text = "SLOW" + resultDataContainer.SLOW.ToString("0000") + "";
            MaxCombo.text = resultDataContainer.MAXCOMBO + "/" + resultDataContainer.noteTotal;
            if (scoreError)
            {
                endType.text = "UNKNOWN";
                endType.color = Color.grey;
                endTypeShadow.Color = greyShadow;
            }
            else
                switch (resultDataContainer.endType)
                {
                    case EndType.GAME_OVER:
                        endType.text = "GAMEOVER";
                        endType.color = Color.grey;
                        endTypeShadow.Color = greyShadow;
                        break;
                    case EndType.FAILED:
                        endType.text = "FAILED";
                        endType.color = Color.grey;
                        endTypeShadow.Color = greyShadow;
                        break;
                    case EndType.COMPLETED:
                        endType.text = "COMPLETED";
                        endType.color = Color.green;
                        endTypeShadow.Color = greenShadow;
                        break;
                    case EndType.FULL_COMBO:
                        endType.text = "FULL COMBO";
                        endType.color = orange;
                        endTypeShadow.Color = orangeShadow;
                        break;
                    case EndType.ALL_PERFECT:
                        endType.text = "ALL PERFECT";
                        endType.color = Color.yellow;
                        endTypeShadow.Color = yellowShadow;
                        break;
                    case EndType.AUTO_PLAY:
                        endType.text = "AUTO PLAY";
                        endType.color = Color.yellow;
                        endTypeShadow.Color = yellowShadow;
                        break;
                    case EndType.AXIUM_CRISIS:
                        endType.text = "AXIUM SCRISIS";
                        endType.color = Color.yellow;
                        endTypeShadow.Color = yellowShadow;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            bool isNoMod = true;
            isNoMod &= !_globalSettings.SkillCheckMode;
            isNoMod &= !_globalSettings.IsMirror;
            isNoMod &= !_globalSettings.HardMode;
            isNoMod &= hpBarType != BarType.EASY;
            isNoMod &= hpBarType != BarType.HARD;
            isNoMod &= !enableJudgeRangeFix;
            noModImage.SetActive(isNoMod);
            skillCheckIndicator.SetActive(_globalSettings.SkillCheckMode);
            mirrorIndicator.SetActive(_globalSettings.IsMirror);
            hardIndicator.SetActive(_globalSettings.HardMode);
            easyIndicator.SetActive(hpBarType == BarType.EASY);
            hardbarIndicator.SetActive(hpBarType == BarType.HARD);
            judgeRangeFixInficator.SetActive(enableJudgeRangeFix);
            HPAcc.text = "HP: " + resultDataContainer.hp.ToString("0.00") + "%" + "   " + "ACC: " +
                         resultDataContainer.Accuracy.ToString("0.00") + "%";
            SongSpeed.text = $"SPEED: {songSpeed:N1}x";
            JudgeInput.text = "JUDGE: " + GameUtil.GetNoteJudgeRange(isHadouTest ? -1 : _globalSettings.NoteJudgeRange).displayName;
            MSDetailsDrawer.Init();
            if (OBSManager.Instance.isActive) StartCoroutine(StopOBS());
            StartCoroutine(PopRate(Util.ScoreToRate(realScore, songDataContainer.selectedDiff, songSpeed), originalRate));
        }

        private IEnumerator PopRate(float newSongRate, float originalRate)
        {
            yield return new WaitWhile(FadeManager.Instance.isFading);
            NotificationBarManager.Instance.Show($"本次游玩Rate：{newSongRate:0.#####}");
            // yield return new WaitWhile(() => NotificationBarManager.Instance.IsDisplaying);
            // float newRate = _globalSettings.PlayerRating.GetRate();
            // float playerRate = MathF.Round(MathF.Round(newRate, 2) - MathF.Round(originalRate, 2), 2);
            // NotificationBarManager.Instance.Show($"玩家现Rate：{newRate:0.00} ({(playerRate < 0 ? "" : "+")}{playerRate:0.00})");
        }

        private bool accIsAnimating = false;

        public void ChangeVisibilityAccDetails(bool value)
        {
            if (accIsAnimating) return;
            accIsAnimating = true;
            StartCoroutine(ChangeVisibilityAccDetailsC(value));
        }

        private IEnumerator ChangeVisibilityAccDetailsC(bool value)
        {
            if (value)
            {
                AccDetailsPanel.transform.parent.gameObject.SetActive(true);
                MSDetailsDrawer.gameObject.SetActive(true);
                AccDetailsPanel.transform.DOScaleY(2, accAnimTimeStart).SetEase(accAnimEase);
                yield return MSDetailsDrawer.ChangeState(true, accAnimTimeStart, accAnimEase);
            }
            else
            {
                AccDetailsPanel.transform.DOScaleY(0, accAnimTimeEnd).SetEase(accAnimEase);
                yield return MSDetailsDrawer.ChangeState(false, accAnimTimeEnd, accAnimEase);
                AccDetailsPanel.transform.parent.gameObject.SetActive(false);
                MSDetailsDrawer.gameObject.SetActive(false);
            }

            accIsAnimating = false;
        }

        private IEnumerator StopOBS()
        {
            yield return new WaitForSecondsRealtime(3f);
            OBSManager.Instance.StopRecording();
        }

        private void UploadScore(string key, string value)
        {
            if (AccountInfo.Instance.acountStatus != AccountInfo.AcountStatus.LOGINED) return;
            JObject post = new JObject
            {
                { "type", "upload_score" }, { "token", AccountInfo.Instance.loginToken }, { "key", key },
                { "value", JObject.Parse(value) }
            };
            HttpRequest.Instance.Post(AccountInfo.Instance.urlPrefix, Encoding.UTF8.GetBytes(post.ToString()));
        }

        public void Exit()
        {
            CheckDataContainers.CleanSongDataContainer();
            CheckDataContainers.CleanResultDataContainer();
            FadeManager.Instance.Back();
        }

        public void Retry()
        {
            CheckDataContainers.CleanResultDataContainer();
            FadeManager.Instance.JumpScene("game");
        }

        private string RankToRankText(int rank)
        {
            return rank switch
            {
                0 => "F",
                1 => "D",
                2 => "C",
                3 => "B",
                4 => "B\n +",
                5 => "A",
                6 => "A\n +",
                7 => "A\n A",
                8 => "A\n A\n  +",
                9 => "A\n A\n  A",
                10 => "A\n A\n  A\n   +",
                11 => "S",
                12 => "S\n +",
                13 => "S\n S",
                14 => "S\n S\n  +",
                15 => "S\n S\n  S",
                16 => "S\n S\n  S\n   +",
                17 => "A\n P\n  J",
                _ => "E\n R\n  O"
            };
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!debugMode) return;
            if (Input.GetKeyDown(KeyCode.R)) Start();
        }
#endif
    }
    
    public enum Grade
    {
        ERR = -1,
        F = 0,
        D = 1,
        C = 2,
        B = 3,
        B_Plus = 4,
        A = 5,
        A_Plus = 6,
        AA = 7,
        AA_Plus = 8,
        AAA = 9,
        AAA_Plus = 10,
        S = 11,
        S_Plus = 12,
        SS = 13,
        SS_Plus = 14,
        SSS = 15,
        SSS_Plus = 16,
        APJ = 17
    }
}