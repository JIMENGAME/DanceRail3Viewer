#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using DRFV.Global;
using DRFV.Global.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DRFV.Test
{
    public class Preview : MonoBehaviour
    {
        public InputField keywordIF, timeIF;
        public AudioSource previewSource;
        private string _keyword;
        private float _time = -1f;
        public void Test()
        {
            if (String.IsNullOrEmpty(_keyword) || _time < 0) return;
            StartCoroutine(TestC());
        }

        public void CheckKeyword(string value)
        {
            if (Util.keywordRegex.IsMatch(value))
            {
                keywordIF.text = _keyword;
            }
            else
            {
                _keyword = keywordIF.text;
            }
        }

        public void CheckTime(string value)
        {
            try
            {
                _time = float.Parse(value);
            }
            catch (FormatException)
            {
                timeIF.text = _time < 0 ? "0" : _time + "";
            }
        }

        private IEnumerator TestC()
        {
            string path = StaticResources.Instance.dataPath + "songs/" + _keyword + "/base.ogg";
            if (!File.Exists(path)) yield break;
            using UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS);
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                AudioClip full = DownloadHandlerAudioClip.GetContent(uwr);
                if (previewSource.isPlaying) previewSource.Stop();
                previewSource.clip = CutPreview(full, _time);
                previewSource.Play();
            }
        }
        private AudioClip CutPreview(AudioClip preview, float time)
        {
            if (preview == null) return null;
            float[] data1 = new float[preview.samples * preview.channels];
            preview.GetData(data1, (int)(time * preview.frequency));
            int length = preview.frequency * preview.channels * 20;
            float[] data2 = new float[length];
            for (int index = 0; index < length; ++index)
            {
                if (index <= preview.frequency * preview.channels)
                {
                    data2[index] = (float)(data1[index] * (index * 1f / (preview.frequency * preview.channels)) * 0.7);
                }
                else if (index >= preview.frequency * preview.channels * 19)
                {
                    data2[index] = (float)(data1[index] *
                                           ((length - index) * 1f / (preview.frequency * preview.channels)) * 0.7);
                }
                else
                {
                    data2[index] = data1[index] * 0.7f;
                }
            }

            AudioClip clip = AudioClip.Create("preview", length / 2, preview.channels, preview.frequency, false);
            clip.SetData(data2, 0);
            return clip;
        }

    }
}
#endif