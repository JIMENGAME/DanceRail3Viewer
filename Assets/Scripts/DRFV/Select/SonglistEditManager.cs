using System.Text.RegularExpressions;
using DRFV.Global;
using DRFV.JsonData;
using DRFV.Language;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Select
{
    public class SonglistEditManager : MonoBehaviour
    {
        public InputField KeywordField;
        public InputField TitleField;
        public InputField ArtistField;
        public InputField BPMField;
        public Toggle nameToggle;
        public Toggle artistToggle;
        public Toggle bpmToggle;

        public Text KeywordLabel;

        public string SongKeyword = "";

        public void ChangeKeyword(string value)
        {
            if (Util.keywordRegex.IsMatch(value))
            {
                KeywordLabel.text = LanguageManager.Instance.GetText("select.error.keyword.format");
                KeywordField.text = SongKeyword;
            }
            else
            {
                KeywordLabel.text = "";
                SongKeyword = value;
            }
        }

        public void ChangeNameField(bool isOn)
        {
            TitleField.interactable = isOn;
        }

        public void ChangeArtistField(bool isOn)
        {
            ArtistField.interactable = isOn;
        }

        public void ChangeBPMField(bool isOn)
        {
            BPMField.interactable = isOn;
        }

        public void StartEdit()
        {
            if (SongKeyword.Equals(""))
            {
                KeywordLabel.text = LanguageManager.Instance.GetText("select.error.keyword.null");
                return;
            }

            Songlist songlist = Util.ReadSonglist();
            Songlist newSonglist = new Songlist();
            bool contain_flag = false;
            foreach (SonglistItem songlistItem in songlist.songs)
            {
                if (SongKeyword.Equals(songlistItem.keyword))
                {
                    if (TitleField.interactable) songlistItem.name = Util.ToBase64(TitleField.text);
                    if (ArtistField.interactable) songlistItem.artist = Util.ToBase64(ArtistField.text);
                    if (BPMField.interactable) songlistItem.bpm = BPMField.text;
                    contain_flag = true;
                }

                newSonglist.songs.Add(songlistItem);
            }

            if (!contain_flag)
            {
                KeywordLabel.text = LanguageManager.Instance.GetText("select.error.keyword.notexist");
                return;
            }
            Util.WriteSonglist(newSonglist);
            CleanFields();
            TheSelectManager.Instance.RefreshSongItem(true);
        }

        public void CleanFields()
        {
            KeywordField.text = "";
            KeywordLabel.text = "";
            TitleField.text = "";
            ArtistField.text = "";
            BPMField.text = "";
            nameToggle.isOn = false;
            artistToggle.isOn = false;
            bpmToggle.isOn = false;
        }
    }
}