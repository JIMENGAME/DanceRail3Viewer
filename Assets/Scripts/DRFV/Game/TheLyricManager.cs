using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DRFV.Global;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Game
{
    public class TheLyricManager : MonoBehaviour
    {
        public Text lyricText;
        public TheGameManager theGameManager;

        private AnimationCurve _BPMCurve;
        private float[] _times;
        private string[] _lyrics;
        private int _idx;
        private bool _inited;

        public IEnumerator Init()
        {
            if (_inited || theGameManager.DebugMode) yield break;
            _BPMCurve = theGameManager.BPMCurve;
            if (theGameManager.storyMode)
            {
                TextAsset textAsset = Resources.Load<TextAsset>($"STORY/SONGS/lyric.{theGameManager.SongKeyword}.{theGameManager.SongHard}");
                if (textAsset == null)
                {
                    textAsset = Resources.Load<TextAsset>($"STORY/SONGS/lyric.{theGameManager.SongKeyword}");
                    if (textAsset == null) yield break;
                }
                SetLyric(JObject.Parse(textAsset.text));
            }
            else
            {
                string filePath = StaticResources.Instance.dataPath + "songs/" +
                                  theGameManager.SongKeyword + "/lyric." + theGameManager.SongHard + ".json";
                if (!File.Exists(filePath))
                {
                    filePath = StaticResources.Instance.dataPath + "songs/" +
                               theGameManager.SongKeyword + "/lyric.json";
                    if (!File.Exists(filePath)) yield break;
                }
                SetLyric(Util.ReadJson(filePath));
            }
        }

        private void SetLyric(JObject main)
        {
            try
            {
                JArray jArray = main["lyrics"].ToObject<JArray>();
                Dictionary<string, string> timeLyricPairs = new Dictionary<string, string>();
                foreach (JToken jToken in jArray)
                {
                    JObject jObject = jToken.ToObject<JObject>();
                    if (!timeLyricPairs.ContainsKey(jObject["time"].ToObject<float>() + ""))
                        timeLyricPairs.Add(jObject["time"].ToObject<float>() + "", jObject["text"].ToObject<string>());
                }

                timeLyricPairs = timeLyricPairs.OrderBy(a => float.Parse(a.Key)).ToDictionary(a => a.Key, b => b.Value);
                _times = new float[timeLyricPairs.Count];
                _lyrics = new string[timeLyricPairs.Count];
                int i = 0;
                foreach (KeyValuePair<string, string> timeLyricPair in timeLyricPairs)
                {
                    _times[i] = _BPMCurve.Evaluate(float.Parse(timeLyricPair.Key));
                    _lyrics[i] = timeLyricPair.Value;
                    i++;
                }

                lyricText.gameObject.SetActive(true);
                _inited = true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Destroy(this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!_inited || _idx >= _times.Length || theGameManager.progressManager.NowTime < _times[_idx]) return;
            lyricText.text = _lyrics[_idx];
            _idx++;
        }

        public void JumpTo(float progress)
        {
            if (!lyricText.gameObject.activeInHierarchy) return;
            for (int i = 0; i < _times.Length - 1; i++)
            {
                if (progress >= _times[i] && progress < _times[i + 1])
                {
                    _idx = i;
                    return;
                }
            }

            _idx = _times.Length - 1;
        }
    }
}