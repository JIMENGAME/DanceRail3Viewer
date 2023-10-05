using DRFV.Enums;
using DRFV.Global;
using DRFV.Global.Managers;
using DRFV.Global.Utilities;
using DRFV.inokana;
using DRFV.Language;
using DRFV.Select;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.HadouTestPreload
{
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
            CheckDataContainers.CleanSongDataContainer();
            CheckDataContainers.CleanResultDataContainer();
            GameObject go = new GameObject("HadouTestDataContainer") { tag = "SongData" };
            HadouTestDataContainer songDataContainer = go.AddComponent<HadouTestDataContainer>();
            songDataContainer.songData = new TheSelectManager.SongData
            {
                keyword = keyword,
                songName = Util.ToBase64(keyword + "." + hard),
                songArtist = Util.ToBase64("-"),
                bpm = "???"
            };
            songDataContainer.selectedDiff = hard;
            songDataContainer.saveAudio = false;
            DontDestroyOnLoad(go);

            FadeManager.Instance.LoadScene("game");
        }

        public void Exit()
        {
            FadeManager.Instance.Back();
        }
    }
}