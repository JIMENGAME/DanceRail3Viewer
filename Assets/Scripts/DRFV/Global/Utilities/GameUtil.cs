using System;
using DRFV.inokana;
using DRFV.Select;
using DRFV.Setting;
using Newtonsoft.Json;
using UnityEngine;

namespace DRFV.Global.Utilities
{
    public class GameUtil
    {
        private static NoteJudgeRange[] noteJudgeRanges = Array.Empty<NoteJudgeRange>();
        public static int NoteJudgeRangeLimit { get; private set;  }

        public static int NoteJudgeRangeCount => noteJudgeRanges.Length;

        public static void Init()
        {
            GameJudgeList judgeList =
                JsonConvert.DeserializeObject<GameJudgeList>(Resources.Load<TextAsset>("note_judge_range").text);
            NoteJudgeRangeLimit = judgeList.limitId;
            noteJudgeRanges = judgeList.noteJudgeRanges;
        }

        #region Chart

        public static string FloatToDRBDecimal(float dec)
        {
            string qwq = dec.ToString("0.00");
            return qwq.EndsWith(".00") ? qwq.Substring(0, qwq.Length - 3) : qwq;
        }

        public static string ParseScore(int score, int? maxDigit = null, bool? usePadding = null)
        {
            return ParseScore(score, GlobalSettings.CurrentSettings.ScoreType, maxDigit, usePadding);
        }

        public static string ParseScore(int score, SCORE_TYPE type, int? maxDigit = null, bool? usePadding = null)
        {
            return ParseScore(score, maxDigit ?? type switch
            {
                SCORE_TYPE.ORIGINAL => 7,
                SCORE_TYPE.ARCAEA => 8,
                SCORE_TYPE.PHIGROS => 7,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            }, usePadding ?? type switch
            {
                SCORE_TYPE.ORIGINAL => false,
                SCORE_TYPE.ARCAEA => false,
                SCORE_TYPE.PHIGROS => true,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            });
        }

        private static string ParseScore(int score, int maxDigit, bool usePadding)
        {
            string input = score + "";
            if (score < 0) input = input.Substring(1);
            string output = "";
            while (usePadding && input.Length < maxDigit)
            {
                input = "0" + input;
            }

            for (int i = input.Length - 1, l = 1; i >= 0; i--)
            {
                output = input[i] + output;
                if (l > 0 && i > 0 && l % 3 == 0)
                {
                    output = "," + output;
                    l = 0;
                }

                l++;
            }

            return (score < 0 ? "-" : "") + output;
        }

        #endregion

        #region Judge

        public static NoteJudgeRange GetNoteJudgeRange(int id)
        {
            if (id == -1) return new NoteJudgeRange { displayName = "HADOU TEST", PJ = 30, P = 60, G = 100 };

            if (id < -1 || id > noteJudgeRanges.Length)
            {
                NotificationBarManager.Instance.Show("出了奇怪的错");
                throw new ArgumentOutOfRangeException("NoteRange");
            }

            return noteJudgeRanges[id];
        }

        #endregion
    }

    public class GameJudgeList
    {
        [JsonProperty("limit")] public int limitId;
        [JsonProperty("ranges")] public NoteJudgeRange[] noteJudgeRanges = Array.Empty<NoteJudgeRange>();
    }
}