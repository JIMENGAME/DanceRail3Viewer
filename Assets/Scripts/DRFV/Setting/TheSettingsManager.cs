using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DRFV.Data;
using DRFV.Global;
using DRFV.Global.Components;
using DRFV.Global.Managers;
using DRFV.Global.Utilities;
using DRFV.JsonData;
using DRFV.Language;
using DRFV.Login;
using DRFV.Pool;
using Newtonsoft.Json;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Setting
{
    public class TheSettingsManager : MonoBehaviour
    {
        public GlobalSettings currentSettings;
        [SerializeField] private Sprite imageButton, imageButtonPressed;

        [SerializeField] private GameObject[] FPSButtons;

        [SerializeField] private Image TapExample;
        [SerializeField] private Image FreeFlickExample;
        [SerializeField] private Image FlickExample;
        [SerializeField] private Slider TapSizeSlider;
        [SerializeField] private Slider FreeFlickSizeSlider;
        [SerializeField] private Slider FlickSizeSlider;
        [SerializeField] private Slider TapAlphaSlider;
        [SerializeField] private Slider FreeFlickAlphaSlider;
        [SerializeField] private Slider FlickAlphaSlider;

        [SerializeField] private SettingSlider MasterVolumeSlider;
        [SerializeField] private SettingSlider MusicVolumeSlider;
        [SerializeField] private SettingSlider ParamEQSlider;
        [SerializeField] private SettingSlider GaterSlider;
        [SerializeField] private SettingSlider TapEffectSlider;

        [SerializeField] private ButtonGroup[] Groups;
        [SerializeField] private Dropdown noteSfxDropdown;

        [SerializeField] private Toggle FCAPIndicatorToggle, OBSRecordModeToggle;

        private SettingLanguage _settingLanguage;

        [SerializeField] private GameObject aboutObj, cleanCacheObj;

        [SerializeField] private RectTransform content;

        [SerializeField] private Text tLogout;

        [SerializeField] private GameObject InputWindowPrefab;

        [SerializeField] private Toggle DebugModeToggle, UseMemoryCacheToggle, EnableJudgeRangeFixToggle, EnableSCFixToggle;

        // Start is called before the first frame update
        void Start()
        {
            currentSettings = GlobalSettings.CurrentSettings;
            foreach (var group in Groups)
            {
                group.Init(this);
            }

            _settingLanguage = gameObject.GetComponent<SettingLanguage>();
            TapSizeSlider.value = currentSettings.TapSize - 1;
            FreeFlickSizeSlider.value = currentSettings.FreeFlickSize - 1;
            FlickSizeSlider.value = currentSettings.FlickSize - 1;
            TapAlphaSlider.value = currentSettings.TapAlpha;
            FreeFlickAlphaSlider.value = currentSettings.FreeFlickAlpha;
            FlickAlphaSlider.value = currentSettings.FlickAlpha;
            FCAPIndicatorToggle.isOn = currentSettings.FCAPIndicator;
            for (int i = 0; i < 3; i++)
            {
                ChangeSize(i);
                ChangeAlpha(i);
            }

            SetFPS(currentSettings.MaxFPS);
            MasterVolumeSlider.SetValue(currentSettings.GameMasterVolume);
            MusicVolumeSlider.SetValue(currentSettings.GameMusicVolume);
            ParamEQSlider.SetValue(currentSettings.GameEffectParamEQLevel);
            GaterSlider.SetValue(currentSettings.GameEffectGaterLevel);
            TapEffectSlider.SetValue(currentSettings.GameEffectTap);
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0);
            offset = currentSettings.Offset;
            inputOffset.text = offset + "";
            tLogout.text = AccountInfo.Instance.acountStatus == AccountInfo.AcountStatus.LOGINED ? "退出登录" : "登录";
            OBSRecordModeToggle.isOn = currentSettings.OBSRecord;
            DebugModeToggle.isOn = DebugModeController.Instance.DebugMode;
            UseMemoryCacheToggle.isOn = currentSettings.UseMemoryCache;
            EnableJudgeRangeFixToggle.isOn = currentSettings.enableJudgeRangeFix;
            EnableSCFixToggle.isOn = currentSettings.enableSCFix;
            string[] directories = Directory.GetDirectories(StaticResources.Instance.dataPath + "settings/note_sfx");
            int selectedNoteSFXId = 0;
            noteSfxDropdown.options.Add(new Dropdown.OptionData
            {
                text = "原版"
            });
            for (var i = 0; i < directories.Length; i++)
            {
                var str = directories[i];
                string directoryName = new DirectoryInfo(str).Name;
                if (directoryName == currentSettings.selectedNoteSFX) selectedNoteSFXId = i + 1;
                noteSfxDropdown.options.Add(new Dropdown.OptionData
                {
                    text = directoryName
                });
            }
            noteSfxDropdown.value = selectedNoteSFXId;
            PoolManager.Instance.RefreshSetting();
        }
        public void SetFPS(int fps)
        {
            int id = 0;
            switch (fps)
            {
                case 60:
                    id = 0;
                    break;
                case 90:
                    id = 1;
                    break;
                case 120:
                    id = 2;
                    break;
                case 144:
                    id = 3;
                    break;
                case 165:
                    id = 4;
                    break;
                case 240:
                    id = 5;
                    break;
            }

            for (int j = 0; j < FPSButtons.Length; j++)
            {
                if (j == id) FPSButtons[j].GetComponent<Image>().sprite = imageButtonPressed;
                else FPSButtons[j].GetComponent<Image>().sprite = imageButton;
            }

            currentSettings.MaxFPS = fps;
        }

        public void ChangeSize(int id)
        {
            switch (id)
            {
                case 0:
                    TapExample.transform.localScale =
                        new Vector3((TapSizeSlider.value + 1) / 4, (TapSizeSlider.value + 1) / 4, 1);
                    break;
                case 1:
                    FreeFlickExample.transform.localScale = new Vector3((FreeFlickSizeSlider.value + 1) / 4,
                        (FreeFlickSizeSlider.value + 1) / 4, 1);
                    break;
                case 2:
                    FlickExample.transform.localScale =
                        new Vector3((FlickSizeSlider.value + 1) / 4, (FlickSizeSlider.value + 1) / 4, 1);
                    break;
            }
        }

        public void ChangeAlpha(int id)
        {
            switch (id)
            {
                case 0:
                    TapExample.color = new Color(1, 1, 1, TapAlphaSlider.value / 3f);
                    break;
                case 1:
                    FreeFlickExample.color = new Color(1, 1, 1, FreeFlickAlphaSlider.value / 3f);
                    break;
                case 2:
                    FlickExample.color = new Color(1, 1, 1, FlickAlphaSlider.value / 3f);
                    break;
            }
        }

        public void Save()
        {
            currentSettings.TapSize = (int)TapSizeSlider.value + 1;
            currentSettings.FreeFlickSize = (int)TapSizeSlider.value + 1;
            currentSettings.FlickSize = (int)FlickSizeSlider.value + 1;
            currentSettings.TapAlpha = (int)TapAlphaSlider.value;
            currentSettings.FreeFlickAlpha = (int)FreeFlickAlphaSlider.value;
            currentSettings.FlickAlpha = (int)FlickAlphaSlider.value;
            currentSettings.GameMasterVolume = MasterVolumeSlider.Value;
            currentSettings.GameMusicVolume = MusicVolumeSlider.Value;
            currentSettings.GameEffectParamEQLevel = ParamEQSlider.Value;
            currentSettings.GameEffectGaterLevel = GaterSlider.Value;
            currentSettings.GameEffectTap = TapEffectSlider.Value;
            currentSettings.FCAPIndicator = FCAPIndicatorToggle.isOn;
            currentSettings.Offset = offset;
            currentSettings.OBSRecord = OBSRecordModeToggle.isOn;
            currentSettings.UseMemoryCache = UseMemoryCacheToggle.isOn;
            currentSettings.enableJudgeRangeFix = EnableJudgeRangeFixToggle.isOn;
            currentSettings.enableSCFix = EnableSCFixToggle.isOn;
            currentSettings.selectedNoteSFX =
                noteSfxDropdown.value == 0 ? "" : noteSfxDropdown.options[noteSfxDropdown.value].text;
            GlobalSettings.CurrentSettings = currentSettings;
            GlobalSettings.Save();
        }

        public void Back()
        {
            currentSettings.GameMasterVolume = currentSettings.GameMasterVolume;
            FadeManager.Instance.Back();
        }

        public void ResetSettings()
        {
            GlobalSettings settings = new GlobalSettings
            {
                SaveVersion = 0
            };
            GlobalSettings.CurrentSettings = settings;
            GlobalSettings.Save();
            FadeManager.Instance.JumpScene("settings");
        }

        public void ExportScore()
        {
            Songlist songlist = Util.ReadSonglist();
            PlayerFile playerFile = new PlayerFile();
            playerFile.Settings = GlobalSettings.CurrentSettings;
            foreach (var item in songlist.songs)
            {
                string keyword = item.keyword;
                if (!Directory.Exists(StaticResources.Instance.dataPath + "songs/" +
                                      keyword)) continue;

                string[] files = Directory.GetFiles(StaticResources.Instance.dataPath +
                                                    "songs/" + keyword);
                foreach (string charts in files)
                {
                    if (charts.EndsWith(".txt") &&
                        !new Regex("[^0-9]").IsMatch(Path.GetFileNameWithoutExtension(charts)))
                    {
                        try
                        {
                            int hard = int.Parse(Path.GetFileNameWithoutExtension(charts));
                            string chartPath = StaticResources.Instance.dataPath + "songs/" + keyword + "/" + hard +
                                               ".txt";
                            if (!File.Exists(chartPath)) continue;
                            string scoreId = "SongScore_" + Util.GetMd5OfChart(keyword, hard);
                            if (!PlayerPrefs.HasKey(scoreId)) continue;
                            playerFile.scores.Add(new ScoreFileScore{ id = scoreId, score = JsonConvert.DeserializeObject<ResultData>(PlayerPrefs.GetString(scoreId))});
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                }
            }

            // if (scoreFile.scores.Count <= 0)
            // {
            //     _settingLanguage.SetScoreIOStatus("settings.score.status.export.null");
            //     return;
            // }
            
            using FileStream fileStream =
                new FileStream(
                    StaticResources.Instance.dataPath + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ".dr3fvscore",
                    FileMode.CreateNew,
                    FileAccess.Write);

            fileStream.Write(DataEncryptor.Encode(JsonConvert.SerializeObject(playerFile, Formatting.None)));
            _settingLanguage.SetScoreIOStatus("settings.score.status.export.success");
        }

        public void ImportScore()
        {
            FileBrowser.SetFilters(false, ".dr3fvscore");
            FileBrowser.ShowLoadDialog(OnScoreLoaded, OnScoreLoadCancel, FileBrowser.PickMode.Files, false,
                StaticResources.Instance.dataPath, null,
                "Load Score File...");
        }

        private void OnScoreLoaded(string[] path)
        {
            using FileStream fileStream = new FileStream(path[0], FileMode.Open, FileAccess.Read);
            byte[] encodedScore = new byte[fileStream.Length];
            fileStream.Read(encodedScore);
            try
            {
                string output = DataEncryptor.Decode(encodedScore);
                try
                {
                    PlayerFile players = JsonConvert.DeserializeObject<PlayerFile>(output);

                    foreach (ScoreFileScore keyValuePair in players.scores)
                    {
                        PlayerPrefs.SetString(keyValuePair.id,
                            JsonConvert.SerializeObject(keyValuePair.score, Formatting.None));
                    }

                    PlayerPrefs.Save();
                    GlobalSettings.CurrentSettings = players.Settings;
                    GlobalSettings.Save();
                    _settingLanguage.SetScoreIOStatus("settings.score.status.import.success");
                }
                catch (Exception)
                {
                    _settingLanguage.SetScoreIOStatus("settings.score.status.import.invalid");
                }
            }
            catch (Exception)
            {
                _settingLanguage.SetScoreIOStatus("settings.score.status.import.invalid");
            }
        }

        private void OnScoreLoadCancel()
        {
            _settingLanguage.SetScoreIOStatus("settings.score.status.import.canceled");
        }

        public void CleanCache()
        {
            if (Directory.Exists(StaticResources.Instance.cachePath))
            {
                string[] files = Directory.GetFiles(StaticResources.Instance.cachePath);
                string[] folders = Directory.GetDirectories(StaticResources.Instance.cachePath);
                foreach (string file in files)
                {
                    File.Delete(file);
                }

                foreach (string folder in folders)
                {
                    Directory.Delete(folder, true);
                }

                EnableCleanCache();
            }
        }

        public void EnableAbout()
        {
            aboutObj.SetActive(true);
        }

        public void DisableAbout()
        {
            aboutObj.SetActive(false);
        }

        public void EnableCleanCache()
        {
            cleanCacheObj.SetActive(true);
        }

        public void DisableCleanCache()
        {
            cleanCacheObj.SetActive(false);
        }

        public InputField inputOffset;
        public float offset;

        public void UpdateOffset()
        {
            if (inputOffset.text.Trim().Equals(""))
            {
                inputOffset.text = "0";
                offset = 0;
            }

            try
            {
                float temp = float.Parse(inputOffset.text);

                offset = temp;
            }
            catch (Exception)
            {
                inputOffset.text = offset + "";
            }
        }

        public void LogOut()
        {
            AccountInfo.Instance.Init();
            Back();
        }

        public void OnRecordEnabled(bool value)
        {
            if (!value)
            {
                OBSManager.Instance.DisableRecordMode();
                return;
            }

            InputMask.Instance.EnableInputMask();
            if (!OBSManager.Instance.EnableRecordMode())
            {
                OBSRecordModeToggle.isOn = false;
            }

            InputMask.Instance.DisableInputMask();
        }

        public void SetDebugMode(bool value)
        {
            if (value)
            {
                if (DebugModeController.Instance.DebugMode) return;
                InputWindow inputWindow = Instantiate(InputWindowPrefab, GameObject.FindWithTag("MainCanvas").transform).GetComponent<InputWindow>();
                // Debug模式密码
                inputWindow.Show(null, null, new byte[] {79, 112, 101, 110, 68, 101, 98, 117, 103, 77, 111, 100, 101, 33, 33, 33, 33},
                    b =>
                    {
                        if (b) DebugModeController.Instance.DebugMode = true;
                        else DebugModeToggle.isOn = false;
                    });
            }
            else DebugModeController.Instance.DebugMode = false;
        }

        public void IntoOffset()
        {
            FadeManager.Instance.LoadScene("offset");
        }

        public void UpdateMasterVolume(Single value)
        {
            StaticResources.Instance.audioMixer.SetFloat("MasterVolume", value * 0.8f - 80f);
        }
    }

    public class PlayerFile
    {
        [JsonProperty("scores")] public List<ScoreFileScore> scores = new();
        [JsonProperty("setting")] public GlobalSettings Settings;
    }

    public class ScoreFileScore
    {
        [JsonProperty("id")] public string id = "";
        [JsonProperty("score")] public ResultData score = new ResultData();
    }
}