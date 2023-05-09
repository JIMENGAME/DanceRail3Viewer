using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DRFV.Global;
using DRFV.inokana;
using DRFV.JsonData;
using DRFV.Language;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Select
{
    public class SonglistCreateManager : MonoBehaviour
    {
        public InputField KeywordField;
        public InputField TitleField;
        public InputField ArtistField;
        public InputField BPMField;

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

        public void AddSongInfo()
        {
            if (SongKeyword == "")
            {
                KeywordLabel.text = LanguageManager.Instance.GetText("select.error.keyword.null");
                return;
            }

            Songlist songlist = Util.ReadSonglist();
            foreach (SonglistItem songlistItem in songlist.songs)
            {
                if (SongKeyword == songlistItem.keyword)
                {
                    KeywordLabel.text = LanguageManager.Instance.GetText("select.error.keyword.exist");
                    return;
                }
            }

            Util.WriteSonglist(new SonglistItem
            {
                keyword = SongKeyword,
                name = Util.ToBase64(TitleField.text),
                artist = Util.ToBase64(ArtistField.text),
                bpm = BPMField.text
            });
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
        }

        public void AutoFillInfo()
        {
            if (SongKeyword.Equals(""))
            {
                KeywordLabel.text = LanguageManager.Instance.GetText("select.error.keyword.null");
                return;
            }

            string filePath = StaticResources.Instance.dataPath + "songs/" + SongKeyword + "/info.txt";
            if (!File.Exists(filePath))
            {
                KeywordLabel.text = LanguageManager.Instance.GetText("select.error.autofill.null");
                return;
            }

            StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8);
            string[] infos = new string[3];
            for (int i = 0; i < 3; i++)
            {
                infos[i] = streamReader.ReadLine();
                if (infos[i] == null)
                {
                    KeywordLabel.text = LanguageManager.Instance.GetText("select.error.autofill.format");
                    return;
                }
            }

            TitleField.text = infos[0];
            ArtistField.text = infos[1];
            BPMField.text = infos[2];
            KeywordLabel.text = "";
        }

        public void BatchImport()
        {
            if (SongKeyword.Equals(""))
            {
                KeywordLabel.text = LanguageManager.Instance.GetText("select.error.batch.keywordnull");
                return;
            }

            string filePath = StaticResources.Instance.dataPath + "songs/" + SongKeyword + ".txt";
            if (!File.Exists(filePath))
            {
                KeywordLabel.text = LanguageManager.Instance.GetText("select.error.batch.null");
                return;
            }

            List<string> existedSid = new List<string>();
            Songlist songlist = Util.ReadSonglist();
            foreach (SonglistItem item in songlist.songs)
            {
                existedSid.Add(item.keyword);
            }

            StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8);
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                string keyword = line.Trim();
                Regex regex = new Regex("[^a-z0-9_]");
                if (keyword.Equals("") || regex.IsMatch(keyword))
                {
                    continue;
                }

                if (existedSid.Contains(keyword))
                {
                    continue;
                }

                string filePath1 = StaticResources.Instance.dataPath + "songs/" + keyword + "/info.txt";
                if (!File.Exists(filePath1))
                {
                    continue;
                }

                bool flag = false;
                StreamReader streamReader1 = new StreamReader(filePath1, Encoding.UTF8);
                string[] infos = new string[3];
                for (int i = 0; i < 3; i++)
                {
                    infos[i] = streamReader1.ReadLine();
                    if (infos[i] != null) continue;
                    flag = true;
                    break;
                }

                if (flag) continue;

                SonglistItem songlistItem = new SonglistItem
                {
                    keyword = keyword,
                    name = Util.ToBase64(infos[0]),
                    artist = Util.ToBase64(infos[1]),
                    bpm = infos[2]
                };
                songlist.songs.Add(songlistItem);
            }

            Util.WriteSonglist(songlist);
            CleanFields();
            TheSelectManager.Instance.RefreshSongItem(true);
        }

        public void ImportChartPackage()
        {
            FileBrowser.SetFilters(false, ".drz");
            FileBrowser.ShowLoadDialog(OnChartPackageSelected, () => { }, FileBrowser.PickMode.Files, false,
                StaticResources.Instance.dataPath, null, "Load DRZ...");
        }

        private void OnChartPackageSelected(string[] path)
        {
            if (path.Length < 1) return;
            FileStream fileStream = new FileStream(path[0], FileMode.Open, FileAccess.Read);
            using ZipInputStream zipInputStream = new ZipInputStream(fileStream);
            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
            List<string> directories = new List<string>();
            ZipEntry zipEntry;
            string id = "";
            DRZInfo? drzInfo1 = null;
            zipInputStream.Password = "44G644G944KI";
            while ((zipEntry = zipInputStream.GetNextEntry()) != null)
            {
                string name = zipEntry.Name;
                if (name.EndsWith("/"))
                {
                    directories.Add(name);
                }
                else
                {
                    byte[] data;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        int size = 2048, read;
                        byte[] buffer = new byte[size];
                        while ((read = zipInputStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memoryStream.Write(buffer, 0, read);
                        }

                        data = memoryStream.ToArray();
                    }
                    if (name.Equals("drzinfo"))
                    {
                        try
                        {
                            drzInfo1 = JsonConvert.DeserializeObject<DRZInfo>(Encoding.UTF8.GetString(data));
                        }
                        catch (Exception)
                        {
                            NotificationBarManager.Instance.Show("错误：谱包信息不合法");
                            return;
                        }
                    }
                    else
                    {
                        files.Add(name, data);
                    }
                }
            }

            if (drzInfo1 == null)
            {
                NotificationBarManager.Instance.Show("错误：谱包信息不存在");
                return;
            }

            DRZInfo drzInfo = (DRZInfo)drzInfo1;

            string songsPath = StaticResources.Instance.dataPath + "songs/";
            string songPath = songsPath + drzInfo.keyword + "/";
            if (Directory.Exists(songPath))
            {
                NotificationBarManager.Instance.Show("错误：keyword已存在");
                return;
            }

            Directory.CreateDirectory(songPath);
            foreach (string directory in directories)
            {
                Util.CreateDirectory(new DirectoryInfo(songPath + "/" + directory));
            }

            foreach (KeyValuePair<string, byte[]> file in files)
            {
                using FileStream fk = new FileStream(songPath + "/" + file.Key, FileMode.Create,
                    FileAccess.Write);
                fk.Write(file.Value);
            }

            Util.WriteSonglist(new SonglistItem
            {
                keyword = drzInfo.keyword,
                name = drzInfo.title,
                artist = drzInfo.artist,
                bpm = drzInfo.bpm
            });
            CleanFields();
            TheSelectManager.Instance.RefreshSongItem(true);
        }
    }

    public struct DRZInfo
    {
        [JsonProperty("keyword")] public string keyword;
        [JsonProperty("title")] public string title;
        [JsonProperty("artist")] public string artist;
        [JsonProperty("bpm")] public string bpm;
    }
}