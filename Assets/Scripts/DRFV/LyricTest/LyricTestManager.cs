using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DRFV.Global;
using DRFV.inokana;
using DRFV.Game;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DRFV.LyricTest
{
    public class LyricTestManager : MonoBehaviour
    {
        public int SongHard;
        public string SongKeyword;

        private DRBFile drbfile;

        public AnimationCurve BPMCurve;

        public bool inited;

        public ProgressManager progressManager;

        float ReadyTime = 1990f;

        // Start is called before the first frame update
        void Start()
        {
            Resources.UnloadUnusedAssets();
            dataPath = GetDataPath();
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            if (!Directory.Exists(dataPath + "songs"))
            {
                Directory.CreateDirectory(dataPath + "songs");
            }

            StartCoroutine(StartReading());
        }

        void LinearKeyframe(Keyframe[] keys)
        {
            if (keys.Length >= 2)
            {
                for (int i = 0; i < keys.Length - 1; i++)
                {
                    keys[i].outTangent = keys[i + 1].inTangent =
                        (keys[i + 1].value - keys[i].value) / (keys[i + 1].time - keys[i].time);
                }
            }
        }

        private void Update()
        {
            if (!inited) return;
            if (BGMManager.isPlaying) progressManager.OnUpdate();
            if (_idx >= _times.Length || progressManager.NowTime < _times[_idx]) return;
            lyricText.text = _lyrics[_idx];
            _idx++;
        }

        public AudioSource BGMManager;

        IEnumerator StartReading()
        {
            progressManager.Init(() =>
            {
                NotificationBarManager.Instance.Show("计时器精准度可能有问题，请与inokana取得联系");
            }, () => NotificationBarManager.Instance.Show("dsp"), GetPitch);
            NoteOffset = PlayerPrefs.GetFloat("noteOffset", 0f);
            if (File.Exists(dataPath + "songs/" + SongKeyword + "/" + SongHard + ".ogg"))
            {
                using var uwr =
                    UnityWebRequestMultimedia.GetAudioClip(
                        "file://" + dataPath + "songs/" + SongKeyword + "/" + SongHard + ".ogg",
                        AudioType.OGGVORBIS);
                yield return uwr.SendWebRequest();
                if (uwr.isDone)
                {
                    BGMManager.clip = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }
            else if (File.Exists(dataPath + "songs/" + SongKeyword + "/base.ogg"))
            {
                using var uwr =
                    UnityWebRequestMultimedia.GetAudioClip(
                        "file://" + dataPath + "songs/" + SongKeyword + "/base.ogg",
                        AudioType.OGGVORBIS);
                yield return uwr.SendWebRequest();
                if (uwr.isDone)
                {
                    BGMManager.clip = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }
            else
            {
                yield break;
            }

            if (File.Exists(dataPath + "songs/" + SongKeyword + "/" + SongHard + ".txt"))
            {
                ReadLyricFile(dataPath + "songs/" + SongKeyword + "/" + SongHard + ".txt");
            }
            else
            {
                yield break;
            }

            Keyframe[] BPMKeyframe = new Keyframe[drbfile.bpms.Count + 1];

            BPMKeyframe[0] = new Keyframe(0.0f, drbfile.offset * 1000.0f);
            float[] BPM_REALTIME = new float[drbfile.bpms.Count + 1];

            for (int i = 0; i < drbfile.bpms.Count; i++)
            {
                if (i == 0)
                {
                    BPM_REALTIME[i] = drbfile.offset * 1000.0f;
                }
                else
                {
                    BPM_REALTIME[i] =
                        (drbfile.bpms[i].time - drbfile.bpms[i - 1].time) *
                        (60 / drbfile.bpms[i - 1].bpm * 4 * drbfile.beat) * 1000.0f + BPM_REALTIME[i - 1];
                }
            }

            BPM_REALTIME[drbfile.bpms.Count] =
                (10000 - drbfile.bpms[drbfile.bpms.Count - 1].time) *
                (60 / drbfile.bpms[drbfile.bpms.Count - 1].bpm * 4 * drbfile.beat) * 1000.0f +
                BPM_REALTIME[drbfile.bpms.Count - 1];
            for (int i = 1; i < drbfile.bpms.Count; i++)
            {
                BPMKeyframe[i] = new Keyframe(drbfile.bpms[i].time, BPM_REALTIME[i]);
            }

            BPMKeyframe[drbfile.bpms.Count] = new Keyframe(10000, BPM_REALTIME[drbfile.bpms.Count]);
            LinearKeyframe(BPMKeyframe);
            BPMCurve = new AnimationCurve(BPMKeyframe);
            string filePath = dataPath + "songs/" +
                              SongKeyword + "/lyric." + SongHard + ".json";
            if (!File.Exists(filePath))
            {
                filePath = dataPath + "songs/" +
                           SongKeyword + "/lyric.json";
                if (!File.Exists(filePath)) goto start;
            }

            try
            {
                JArray jArray = Util.ReadJson(filePath)["lyrics"].ToObject<JArray>();
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
                    _times[i] = BPMCurve.Evaluate(float.Parse(timeLyricPair.Key));
                    _lyrics[i] = timeLyricPair.Value;
                    i++;
                }

                lyricText.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            start:
            progressManager.AddDelay(NoteOffset / 1000f);
            progressManager.AddStartDelay(ReadyTime / 1000f);
            progressManager.StartTiming();
            BGMManager.PlayScheduled(AudioSettings.dspTime + ReadyTime / 1000f);
            inited = true;
        }

        private float GetPitch()
        {
            return BGMManager.pitch;
        }

        public Text lyricText;
        private float NoteOffset;

        [SerializeField] private float[] _times;
        [SerializeField] private string[] _lyrics;
        [SerializeField] private int _idx;

        void ReadLyricFile(string path)
        {
            string[] s;

            using StreamReader streamReader = new StreamReader(path, Encoding.UTF8);
            string line;
            List<string> list = new List<string>();
            while ((line = streamReader.ReadLine()) != null)
            {
                if (line.Trim() == "") continue;
                list.Add(line.Trim().Replace("\r", "").Replace("\t", ""));
            }

            s = list.ToArray();
            drbfile = DRBFile.Parse(String.Join("\n", s));
        }

        public string dataPath;

        private static string GetDataPath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return Application.dataPath + "/../";
                case RuntimePlatform.IPhonePlayer:
                    return Application.persistentDataPath + "/";
                case RuntimePlatform.Android:
                    return "/storage/emulated/0/DR3Viewer/";
                default:
                    Application.Quit();
                    throw new ArgumentException("Unsupported System");
            }
        }

        public InputField inputJumpTo;

        public void JumpTo()
        {
            if (!inited) return;
            progressManager.StopTiming();
            BGMManager.Pause();
            float time, from = progressManager.NowTime;
            try
            {
                time = BPMCurve.Evaluate(float.Parse(inputJumpTo.text));
            }
            catch (Exception e)
            {
                Debug.Log(e);
                BGMManager.UnPause();
                progressManager.ContinueTiming();
                return;
            }

            progressManager.AddDelay((from - time) / 1000f);
            BGMManager.time = time / 1000;
            bool hasLyric = false;
            for (int i = 0; i < _times.Length - 1; i++)
            {
                if (!(progressManager.NowTime >= _times[i]) || !(progressManager.NowTime < _times[i + 1])) continue;
                _idx = i;
                hasLyric = true;
                break;
            }

            if (!hasLyric) _idx = _times.Length - 1;

            BGMManager.UnPause();
            progressManager.ContinueTiming();
        }
    }
}