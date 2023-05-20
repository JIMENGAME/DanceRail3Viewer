using System;
using System.Collections;
using System.Text;
using DRFV.Enums;
using DRFV.Global;
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

namespace DRFV.Result
{
    public class TheResultManager : MonoBehaviour
    {
        public TextMeshProUGUI score;
        public TextMeshProUGUI Tier;
        public Image TierBackground;

        public TextMeshProUGUI PerfectJ;
        public TextMeshProUGUI Perfect;
        public TextMeshProUGUI Good;
        public TextMeshProUGUI Miss;
        public TextMeshProUGUI Fast;
        public TextMeshProUGUI Slow;
        public Text MaxCombo;
        public Text scoreDelta;
        public GameObject tNewScore;
        public GameObject skillCheckIndicator;
        public GameObject mirrorIndicator;
        public GameObject hardIndicator;
        public GameObject easyIndicator;
        public GameObject hardbarIndicator;
        public GameObject noModImage;

        public Text endType;
        public TrueShadow endTypeShadow;

        public Image CoverBackground;
        public Image Cover;
        public Text Title;

        public Text Artist;

        public Sprite CoverPlaceHolder;

        public Text HPAcc;

        public Text SongSpeed;

        public Text JudgeInput;

        public GameObject AccDetailsPanel, AccDetails;

        public TextMeshProUGUI TMPRank;

        public TMP_ColorGradient[] TCGRank;
        public TMP_ColorGradient TCGError;

        private static Color orange = new(1f, 0.5f, 0.5f),
            greyShadow = new(0.9f, 0.9f, 0.9f, 0.6f),
            greenShadow = new(0f, 0.3f, 0f, 0.6f),
            orangeShadow = new(0.3f, 0.15f, 0.15f, 0.6f),
            yellowShadow = new(0.3f, 0.2764706f, 0.004705882f, 0.6f);

        // Start is called before the first frame update
        void Start()
        {
            AccountInfo.Instance.UpdateAccountPanel();
            SongDataContainer songDataContainer = GameObject.FindWithTag("SongData").GetComponent<SongDataContainer>();
            ResultDataContainer resultDataContainer =
                GameObject.FindWithTag("ResultData").GetComponent<ResultDataContainer>();
#if UNITY_EDITOR
            songDataContainer.songData ??= new TheSelectManager.SongData
            {
                songName = "5Y+q5Zug5L2g5aSq576O",
                songArtist = "U1dJTi1T",
                cover = null
            };
#endif
            float realScore = GlobalSettings.CurrentSettings.ScoreType != SCORE_TYPE.ORIGINAL
                ? 3000000.0f *
                (resultDataContainer.PERFECT_J + resultDataContainer.PERFECT * 0.99f +
                 resultDataContainer.GOOD / 3f) / resultDataContainer.noteTotal
                : resultDataContainer.SCORE;
            int thisScore = Mathf.RoundToInt(realScore);
            bool isValid =
                resultDataContainer.PERFECT_J + resultDataContainer.PERFECT + resultDataContainer.GOOD +
                resultDataContainer.MISS ==
                resultDataContainer.noteTotal;
            int rank = Util.ScoreToRank(thisScore);
            bool scoreError = rank < 0;
            TMPRank.colorGradientPreset = rank > 0 ? TCGRank[rank] : TCGError;
            TMPRank.text = RankToRankText(rank);
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

                scoreDelta.text = "成绩出错";
                tNewScore.SetActive(false);
            }
            else if (resultDataContainer.endType != EndType.AUTO_PLAY &&
                     !songDataContainer.useSkillCheck &&
                     songDataContainer.barType != BarType.EASY && songDataContainer.NoteJudgeRange > 1)
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

                if (GlobalSettings.CurrentSettings.ScoreType != SCORE_TYPE.ORIGINAL)
                {
                    scoreDelta.text = "非原版计分方式";
                    tNewScore.SetActive(false);
                }
                else
                {
                    scoreDelta.text = Util.ParseScore(resultDataOld.score) + "  " +
                                      (resultDataOld.score <= thisScore ? "+" : "-") +
                                      Util.ParseScore(thisScore - resultDataOld.score);
                    tNewScore.SetActive(newRecord);
                }

                if (newRecord)
                {
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
                        acc = MathF.Round(resultDataContainer.Accuracy, 2, MidpointRounding.AwayFromZero)
                    };
                    string resultStr = JsonConvert.SerializeObject(resultData, Formatting.None);
                    if (songDataContainer.GetContainerType() != SongDataContainerType.HADOU_TEST)
                        UploadScore(key, resultStr);

                    PlayerPrefs.SetString(key, resultStr);
                    PlayerPrefs.Save();
                }
            }
            else
            {
                scoreDelta.text = "";
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
                CoverBackground.color = new Color(1f, 130f / 255f, 1f);
                Cover.sprite = CoverPlaceHolder;
            }

            Title.text = songDataContainer.songData.songName;
            Artist.text = songDataContainer.songData.songArtist;
            score.text = Util.ParseScore(Mathf.RoundToInt(resultDataContainer.SCORE));
            PerfectJ.text = resultDataContainer.PERFECT_J + "";
            Perfect.text = resultDataContainer.PERFECT + "";
            Good.text = resultDataContainer.GOOD + "";
            Miss.text = resultDataContainer.MISS + "";
            Fast.text = "FAST" + resultDataContainer.FAST + "";
            Slow.text = "SLOW" + resultDataContainer.SLOW + "";
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            bool isNoMod = true;
            isNoMod = isNoMod && !songDataContainer.useSkillCheck;
            isNoMod = isNoMod && !songDataContainer.isMirror;
            isNoMod = isNoMod && !songDataContainer.isHard;
            isNoMod = isNoMod && songDataContainer.barType != BarType.EASY;
            isNoMod = isNoMod && songDataContainer.barType != BarType.HARD;
            noModImage.SetActive(isNoMod);
            skillCheckIndicator.SetActive(songDataContainer.useSkillCheck);
            mirrorIndicator.SetActive(songDataContainer.isMirror);
            hardIndicator.SetActive(songDataContainer.isHard);
            easyIndicator.SetActive(songDataContainer.barType == BarType.EASY);
            hardbarIndicator.SetActive(songDataContainer.barType == BarType.HARD);
            HPAcc.text = "HP: " + resultDataContainer.hp.ToString("0.00") + "%" + "   " + "ACC: " +
                         resultDataContainer.Accuracy.ToString("0.00") + "%";
            SongSpeed.text = $"SPEED: {songDataContainer.songSpeed:N1}x";
            JudgeInput.text = "JUDGE: " + songDataContainer.NoteJugeRangeLabel;
            if (OBSManager.Instance.isActive) StartCoroutine(StopOBS());
        }

        public void ChangeVisibilityAccDetails(bool value)
        {
            AccDetailsPanel.SetActive(value);
            AccDetails.SetActive(value);
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
            FadeManager.Instance.LoadScene("select");
        }

        public void Retry()
        {
            CheckDataContainers.CleanResultDataContainer();
            FadeManager.Instance.LoadScene("game");
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
    }

    public class ResultData
    {
        [JsonProperty("score")] public int score;
        [JsonProperty("type")] public string endType;
        [JsonProperty("perfectj")] public int perfectJ;
        [JsonProperty("perfect")] public int perfect;
        [JsonProperty("good")] public int good;
        [JsonProperty("miss")] public int miss;
        [JsonProperty("fast")] public int fast;
        [JsonProperty("slow")] public int slow;
        [JsonProperty("hp")] public float hp;
        [JsonProperty("acc")] public float acc;

        public ResultData()
        {
            score = 0;
            endType = "cp";
            acc = perfectJ = perfect = good = miss = fast = slow = 0;
            hp = 114.514f;
        }
    }
    //
    // public enum Grade
    // {
    //     F = 0,
    //     D = 1,
    //     C = 2,
    //     B = 3,
    //     B_Plus = 4,
    //     A = 5,
    //     A_Plus = 6,
    //     AA = 7,
    //     AA_Plus = 8,
    //     AAA = 9,
    //     AAA_Plus = 10,
    //     S = 11,
    //     S_Plus = 12,
    //     SS = 13,
    //     SS_Plus = 14,
    //     SSS = 15,
    //     SSS_Plus = 16,
    //     APJ = 17
    // }
}