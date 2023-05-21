using DRFV.Enums;
using DRFV.Global;
using DRFV.inokana;
using DRFV.Language;
using DRFV.Select;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.UI;

public class HadouTestPreloadManager : MonoBehaviour
{
    public InputField KeywordField;
    private string keyword;

    private int hard;

    public void ChangeKeyword(string value)
    {
        if (Util.keywordRegex.IsMatch(value))
        {
            NotificationBarManager.Instance.Show(LanguageManager.Instance.GetText("select.error.keyword.format"));
            KeywordField.text = keyword;
        }
        else
        {
            keyword = value;
        }
    }

    public void ChangeTier(string value)
    {
        hard = int.Parse(value);
    }

    public void StartGame()
    {
        GlobalSettings currentSettings = GlobalSettings.CurrentSettings;
        CheckDataContainers.CleanSongDataContainer();
        CheckDataContainers.CleanResultDataContainer();
        GameObject go = new GameObject("HadouTestDataContainer") { tag = "SongData" };
        HadouTestDataContainer songDataContainer = go.AddComponent<HadouTestDataContainer>();
        songDataContainer.speed = currentSettings.NoteSpeed;
        songDataContainer.offset = currentSettings.Offset;
        songDataContainer.songData = new TheSelectManager.SongData
        {
            keyword = keyword,
            songName = Util.ToBase64(keyword + "." + hard),
            songArtist = Util.ToBase64("-"),
            bpm = "???"
        };
        songDataContainer.isAuto = currentSettings.IsAuto;
        songDataContainer.isMirror = currentSettings.IsMirror;
        songDataContainer.isHard = currentSettings.HardMode;
        songDataContainer.useSkillCheck = currentSettings.SkillCheckMode;
        songDataContainer.selectedDiff = hard;
        songDataContainer.tapSize = currentSettings.TapSize;
        songDataContainer.freeFlickSize = currentSettings.FreeFlickSize;
        songDataContainer.flickSize = currentSettings.FlickSize;
        songDataContainer.tapAlpha = currentSettings.TapAlpha;
        songDataContainer.flickAlpha = currentSettings.FlickAlpha;
        songDataContainer.freeFlickAlpha = currentSettings.FreeFlickAlpha;
        songDataContainer.GameEffectParamEQLevel = currentSettings.GameEffectParamEQLevel;
        songDataContainer.GameEffectGaterLevel = currentSettings.GameEffectGaterLevel;
        songDataContainer.GameEffectTap = currentSettings.GameEffectTap;
        songDataContainer.playerGameComboDisplay = (GameComboDisplay)currentSettings.ComboDisp;
        songDataContainer.gameSubJudgeDisplay = (GameSubJudgeDisplay)currentSettings.SmallJudgeDisp;
        songDataContainer.FCAPIndicator = currentSettings.FCAPIndicator;
        songDataContainer.saveAudio = false;
        songDataContainer.barType = (BarType)currentSettings.HPBarType;
        songDataContainer.songSpeed = currentSettings.SongSpeed switch
        {
            0 => 1.0f,
            1 => 1.1f,
            2 => 1.2f,
            3 => 1.3f,
            4 => 1.4f,
            5 => 1.5f,
            6 => 1.6f,
            7 => 1.7f,
            8 => 1.8f,
            9 => 1.9f,
            10 => 2.0f,
            11 => 2.1f,
            12 => 2.2f,
            13 => 2.3f,
            14 => 2.4f,
            15 => 2.5f,
            16 => 2.6f,
            17 => 2.7f,
            18 => 2.8f,
            19 => 2.9f,
            20 => 3.0f,
            _ => 1.0f
        };
        songDataContainer.NoteJudgeRange = 2;
        songDataContainer.gameSide = (GameSide)currentSettings.GameSide;
        DontDestroyOnLoad(go);

        FadeManager.Instance.LoadScene("game");
    }

    public void Exit()
    {
        FadeManager.Instance.LoadScene("main");
    }
}