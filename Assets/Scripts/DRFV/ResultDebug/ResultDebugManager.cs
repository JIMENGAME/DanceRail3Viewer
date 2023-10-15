#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DRFV.Enums;
using DRFV.Game;
using DRFV.Global;
using DRFV.Global.Managers;
using DRFV.Global.Utilities;
using DRFV.inokana;
using DRFV.JsonData;
using DRFV.Result;
using DRFV.Select;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;


public class ResultDebugManager : MonoBehaviour
{
    [SerializeField] private Dropdown ddSongKeyword, ddTier;
    [SerializeField] private InputField ifPj, ifP, ifG;
    [SerializeField] private Text tNoteCount;
    private string songsPath;
    private Regex number = new("[^0-9]");
    private string selectedKeyword;
    private int selectedTier;
    private int noteCount;
    private int[] tiers;
    private bool[] isGoodable;
    private static readonly Random Random = new();
    private Dictionary<string, SonglistItem> songsDic = new();
    private string md5;

    // Start is called before the first frame update
    void Start()
    {
        ddSongKeyword.onValueChanged.AddListener(UpdateDropdownTier);
        ddTier.onValueChanged.AddListener(UpdateNoteCount);
        ddSongKeyword.ClearOptions();
        ddTier.ClearOptions();
        songsPath = StaticResources.Instance.dataPath + "/songs/";
        var songlist = Util.ReadSonglist();
        foreach (var song in songlist.songs)
        {
            ddSongKeyword.options.Add(new Dropdown.OptionData(song.keyword));
            songsDic.Add(song.keyword, song);
        }

        ddSongKeyword.RefreshShownValue();
        UpdateDropdownTier(ddSongKeyword.value);
    }

    private void UpdateDropdownTier(int id)
    {
        ddTier.ClearOptions();
        selectedKeyword = ddSongKeyword.options[id].text;
        string[] files = Directory.GetFiles(songsPath + selectedKeyword);

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

        this.tiers = tiers.ToArray();

        foreach (int tier in tiers)
        {
            ddTier.options.Add(new Dropdown.OptionData(tier + ""));
        }

        ddTier.RefreshShownValue();
        UpdateNoteCount(ddTier.value);
    }

    private void UpdateNoteCount(int id)
    {
        selectedTier = tiers[id];
        DRBFile drbFile = DRBFile.Parse(File.ReadAllText(songsPath + "/" + selectedKeyword + "/" + selectedTier + ".txt"));
        noteCount = drbFile.TotalNotes;
        md5 = drbFile.GetMD5();
        tNoteCount.text = noteCount + "";

        isGoodable = drbFile.notes.Select(noteData => noteData.kind == NoteKind.TAP).ToArray();
    }

    public void EnterGame()
    {
        int pj = int.Parse(ifPj.text);
        int p = int.Parse(ifP.text);
        int g = int.Parse(ifG.text);
        if (pj + p + g > noteCount)
        {
            NotificationBarManager.Instance.Show("判定比note多，你在写啥");
            return;
        }
        string picturePath = songsPath + selectedKeyword + "/base";
        foreach (string suffix in Util.ImageSuffixes)
        {
            if (File.Exists(picturePath + suffix))
            {
                picturePath = string.Concat(picturePath, suffix);
                break;
            }
        }
        GameObject objSongDataContainer = new GameObject("DebugSongDataContainer")
        {
            tag = "SongData"
        };
        SongDataContainer songDataContainer = objSongDataContainer.AddComponent<SongDataContainer>();
        DontDestroyOnLoad(objSongDataContainer);
        GameObject objResultDataContainer = new GameObject("DebugResultDataContainer")
        {
            tag = "ResultData"
        };
        ResultDataContainer resultDataContainer = objResultDataContainer.AddComponent<ResultDataContainer>();
        DontDestroyOnLoad(objResultDataContainer);
        songDataContainer.selectedDiff = selectedTier;
        songDataContainer.songData = new TheSelectManager.SongData
        {
            keyword = selectedKeyword,
            songName = songsDic[selectedKeyword].name,
            songArtist = songsDic[selectedKeyword].artist,
            cover = Util.ByteArrayToSprite(File.ReadAllBytes(picturePath), 512, 512)
        };

        int miss = noteCount - pj - p - g;
        resultDataContainer.PERFECT_J = pj;
        resultDataContainer.PERFECT = p;
        resultDataContainer.GOOD = g;
        resultDataContainer.MISS = miss;
        resultDataContainer.endType =
            miss == 0 ? (g == 0 ? EndType.ALL_PERFECT : EndType.FULL_COMBO) : EndType.COMPLETED;

        resultDataContainer.noteTotal = noteCount;
        resultDataContainer.SCORE = 3000000 * (pj + 0.99f * p + g / 3f) / noteCount;
        resultDataContainer.MAXCOMBO = miss == 0 ? noteCount : Random.Next(0, noteCount - miss + 1);
        if (g + miss == 0) resultDataContainer.hp = 100f;
        else while (resultDataContainer.hp is <= 0 or > 100)
            resultDataContainer.hp = (float) (Random.NextDouble() * 100.0);
        List<float> randomHitResult = new(), randomAcc = new();
        NoteJudgeRange noteJudgeRange = GameUtil.GetNoteJudgeRange(GlobalSettings.CurrentSettings.NoteJudgeRange);
        foreach (bool b in isGoodable)
        {
            if (Random.NextBool() && miss > 0)
            {
                miss--;
                randomHitResult.Add(noteJudgeRange.G * 2f);
            }
            else if (!b) // Perfect-J
            {
                pj--;
                randomHitResult.Add(0f);
            }
            else if (Random.NextBool() && pj > 0) // isPerfect-J
            {
                pj--;
                var next = Random.Next(0, (int)(noteJudgeRange.PJ * 1000f) + 1) / 1000f;
                bool isNegative = Random.NextBool();
                randomHitResult.Add((isNegative ? -1 : 1) * next);
            }
            else if (Random.NextBool() && p > 0) // isPerfect
            {
                p--;
                float next = Random.Next((int)(noteJudgeRange.PJ * 1000f) + 1, (int)(noteJudgeRange.P * 1000f) + 1) / 1000f;
                bool isNegative = Random.NextBool();
                randomHitResult.Add((isNegative ? -1 : 1) * next);
                if (isNegative)
                {
                    resultDataContainer.FAST++;
                }
                else
                {
                    resultDataContainer.SLOW++;
                }
            }
            else if (g > 0) // Good
            {
                g--;
                float next = Random.Next((int)(noteJudgeRange.P * 1000f) + 1, (int)(noteJudgeRange.G * 1000f) + 1) / 1000f;
                bool isNegative = Random.NextBool();
                randomHitResult.Add((isNegative ? -1 : 1) * next);
                if (isNegative)
                {
                    resultDataContainer.FAST++;
                }
                else
                {
                    resultDataContainer.SLOW++;
                }
            }
        }

        if (p + g != 0)
        {
            NotificationBarManager.Instance.Show("小p和good加起来比tap多，你在写啥");
            return;
        }
        randomAcc.AddRange(randomHitResult.Select(f => noteJudgeRange.G - f));
        resultDataContainer.msDetails = randomHitResult;

        resultDataContainer.Accuracy = randomAcc.Average() / noteJudgeRange.G * 100f;

        resultDataContainer.md5 = md5;
        
        FadeManager.Instance.LoadScene("result");
    }
}

#endif