using System.IO;
using System.Text.RegularExpressions;
using DRFV.Global;
using DRFV.JsonData;
using DRFV.Language;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Select
{
    public class SonglistDeleteManager : MonoBehaviour
    {
        public InputField KeywordField;
        public Toggle deleteFile;
    
        public Text KeywordLabel;

        public string SongKeyword = "";

        public void ChangeKeyword()
        {
            string keyword = KeywordField.text;
            if (Util.keywordRegex.IsMatch(keyword))
            {
                KeywordLabel.text = LanguageManager.Instance.GetText("select.error.keyword.format");
                KeywordField.text = SongKeyword;
            }
            else
            {
                KeywordLabel.text = "";
                SongKeyword = keyword;
            }
        }
    
        public void RemoveSongInfo()
        {
            if (SongKeyword.Equals(""))
            {
                KeywordLabel.text = LanguageManager.Instance.GetText("select.error.keyword.null");
                return;
            }
            Songlist songlist = Util.ReadSonglist();
            for (int i = 0; i < songlist.songs.Count; i++)
            {
                if (SongKeyword.Equals(songlist.songs[i].keyword))
                {
                    if (deleteFile.isOn)
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(
                            StaticResources.Instance.dataPath + "songs/" + SongKeyword);
                        directoryInfo.Delete(true);
                    }
                    songlist.songs.Remove(songlist.songs[i]);
                    break;
                }
            }
            Util.WriteSonglist(songlist);
            CleanFields();
            TheSelectManager.Instance.RefreshSongItem(true);
        }

        public void CleanFields()
        {
            KeywordField.text = "";
            KeywordLabel.text = "";
        }
    }
}
