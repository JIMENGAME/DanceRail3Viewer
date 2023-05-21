using DRFV.Select;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace DRFV.Setting
{
    public class GlobalSettings
    {
        [JsonIgnore]
        public static GlobalSettings CurrentSettings
        {
            get
            {
                if (!PlayerPrefs.HasKey("Global_Settings")) return new GlobalSettings();
                var str = PlayerPrefs.GetString("Global_Settings");
                return JsonConvert.DeserializeObject<GlobalSettings>(str);
            }
            set
            {
                var str = JsonConvert.SerializeObject(value, Formatting.None);
                PlayerPrefs.SetString("Global_Settings", str);
                PlayerPrefs.Save();
            }
        }

        public int SaveVersion;
        public float Offset;
        public int NoteSpeed;
        public int TapSize;
        public int FreeFlickSize;
        public int FlickSize;
        public int TapAlpha;
        public int FreeFlickAlpha;
        public int FlickAlpha;
        private int _fps;

        public int MaxFPS
        {
            get => _fps;
#if UNITY_EDITOR
            set => _fps = value;
#else
            set => Application.targetFrameRate = _fps = value;
#endif
        }

        public int GameEffectParamEQLevel;
        public int GameEffectGaterLevel;
        public int GameEffectTap;
        public int ComboDisp;
        public int SmallJudgeDisp;
        public bool FCAPIndicator;
        public int AutoplayHint;
        public int SongSpeed;
        public int HPBarType;
        public int NoteJudgeRange;
        public bool OBSRecord;
        public int GameSide;
        public bool IsAuto;
        public bool IsMirror;
        public bool SkillCheckMode;
        public bool HardMode;
        public bool UseMemoryCache;
        public SCORE_TYPE ScoreType;
        public SELECT_ORDER SelectOrder;

        public GlobalSettings()
        {
            SaveVersion = 0;
            Offset = 0f;
            NoteSpeed = 12;
            TapSize = 2;
            FreeFlickSize = 3;
            FlickSize = 3;
            TapAlpha = 3;
            FreeFlickAlpha = 3;
            FlickAlpha = 3;
            MaxFPS = 60;
            GameEffectParamEQLevel = 10;
            GameEffectGaterLevel = 10;
            GameEffectTap = 10;
            ComboDisp = 1;
            SmallJudgeDisp = 1;
            FCAPIndicator = false;
            AutoplayHint = 0;
            SongSpeed = 0;
            HPBarType = 0;
            OBSRecord = false;
            GameSide = 1;
            NoteJudgeRange = 2;
            IsAuto = true;
            IsMirror = false;
            SkillCheckMode = false;
            HardMode = false;
            UseMemoryCache = true;
            ScoreType = SCORE_TYPE.ORIGINAL;
            SelectOrder = SELECT_ORDER.SONGLIST;
        }
    }

    public enum SCORE_TYPE
    {
        ORIGINAL = 0,
        ARCAEA = 1,
        PHIGROS = 2
    }
}