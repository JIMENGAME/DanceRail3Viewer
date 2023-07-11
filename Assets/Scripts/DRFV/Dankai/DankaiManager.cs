using System.Collections.Generic;
using System.Linq;
using DRFV.Global.Managers;
using DRFV.Select;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Dankai
{
    public class DankaiManager : MonoBehaviour
    {
        private Dictionary<string, DankaiData[]> _dankaiList = new Dictionary<string, DankaiData[]>();
        private object loo = new();
        private int selectedDankai = 12;
        private int selectedId = 0;
        [SerializeField] private Transform dankaiButtonPanel;
        [SerializeField] private GameObject dankaiButtonPrefab;
        [SerializeField] private Text tTile, tHint, tTiers;
        // Start is called before the first frame update
        void Start()
        {
            _dankaiList = JsonConvert.DeserializeObject<Dictionary<string, DankaiData[]>>(Resources.Load<TextAsset>("DANKAI/dankailist").text);
            if (!_dankaiList.ContainsKey(selectedDankai.ToString()))
            {
                FadeManager.Instance.Back();
                return;
            }
            DankaiData[] dankaiData = _dankaiList[selectedDankai.ToString()];
            for (var i = 0; i < dankaiData.Length; i++)
            {
                GameObject instantiate = Instantiate(dankaiButtonPrefab, dankaiButtonPanel);
                instantiate.GetComponent<DankaiButton>().Init(selectedDankai, i, this);
            }

            RefreshSelectedDankaiUI();
        }

        public void RefreshSelectedDankaiNow(int dankai, int id)
        {
            if (selectedDankai == dankai && selectedId == id) return;
            lock (loo)
            {
                selectedDankai = dankai;
                selectedId = id;
                RefreshSelectedDankaiUI();
            }
        }

        private void RefreshSelectedDankaiUI()
        {
            tTile.text = $"技能检测 Lv.{selectedDankai} Vol.{selectedId + 1}";
            DankaiData dankaiData = _dankaiList[selectedDankai.ToString()][selectedId];
            tHint.text = $"<size=50>要求：</size>\nHP最大值限制为{dankaiData.hp:0.##}点。\nHP大于0时完成所有歌曲。";
            tHint.text = "曲目难度：" + dankaiData.tiers;
        }

        public void EnterDankai()
        {
            DankaiData _dankaiData = _dankaiList[selectedDankai.ToString()][selectedId];
            Dictionary<string,SongData> songDatas = JsonConvert.DeserializeObject<Dictionary<string, SongData>>(Resources.Load<TextAsset>("DANKAI/songlist").text);
            GameObject mainObj = new GameObject("DankaiDataContainer")
            {
                tag = "DankaiData"
            };
            DankaiDataContainer dankaiDataContainer = mainObj.AddComponent<DankaiDataContainer>();
            dankaiDataContainer.hpNow = dankaiDataContainer.hpMax = _dankaiData.hp;
            dankaiDataContainer.songs = new SongDataContainer[_dankaiData.songs.Count];
            for (var i = 0; i < _dankaiData.songs.Count; i++)
            {
                _dankaiData.tiers = string.Join(", ", _dankaiList[selectedDankai.ToString()][selectedId].songs.Select(song => song.tier));
                var song = _dankaiData.songs[i];

                GameObject obj = new GameObject("DANKAI SONG" + (i + 1))
                {
                    tag = "SongData"
                };
                obj.transform.SetParent(mainObj.transform);
                dankaiDataContainer.songs[i] = obj.AddComponent<SongDataContainer>();
                dankaiDataContainer.songs[i].songData = new TheSelectManager.SongData()
                {
                    keyword = song.keyword,
                    songName = songDatas[song.keyword].name,
                    songArtist = songDatas[song.keyword].artist,
                    cover = Resources.Load<Sprite>("DANKAI/SONGS/" + song.keyword),
                    hards = new[] { song.tier }
                };
                dankaiDataContainer.songs[i].selectedDiff = song.tier;
                dankaiDataContainer.songs[i].music = Resources.Load<AudioClip>($"DANKAI/SONGS/{song.keyword}.{song.tier}");
                if (!dankaiDataContainer.songs[i].music)
                    dankaiDataContainer.songs[i].music = Resources.Load<AudioClip>($"DANKAI/SONGS/{song.keyword}");
            }
            DontDestroyOnLoad(mainObj);
            FadeManager.Instance.LoadScene("game");
        }

        public void Exit()
        {
            FadeManager.Instance.Back();
        }
    }

    public class DankaiData
    {
        public float hp;
        public List<Song> songs;
        [JsonIgnore]
        public string tiers;
    }

    public class Song
    {
        public string keyword;
        public int tier;
    }

    public class SongData
    {
        public string name;
        public string artist;
    }
}