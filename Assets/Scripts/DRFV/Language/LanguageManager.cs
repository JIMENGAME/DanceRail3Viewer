using System;
using System.Collections.Generic;
using System.Text;
using DRFV.inokana;
using UnityEngine;

namespace DRFV.Language
{
    public class LanguageManager : MonoSingleton<LanguageManager>
    {
        public bool isDone = false;
        private Dictionary<string, string> _languageMap;

        public void SetLanguage(string identity)
        {
            ReadLanguageFile(identity);
            // PlayerPrefs.SetString("Language", identity);
            // PlayerPrefs.Save();
        }

        private void ReadLanguageFile(string identity)
        {
            try
            {
                _languageMap = new Dictionary<string, string>();
                string[] texts = Resources.Load<TextAsset>("LANG/" + identity).text.Replace("\r", "").Split("\n");
                foreach (string text in texts)
                {
                    string qwq = text.Trim().Replace("\\n", "\n");
                    if (qwq.Equals(""))
                    {
                        continue;
                    }

                    if (!qwq.EndsWith(";"))
                    {
                        throw new ArgumentException("Wrong Line: " + text.Trim());
                    }

                    string[] array = qwq.Substring(0, qwq.Length - 1).Split("=");
                    if (array.Length == 2)
                    {
                        _languageMap.Add(array[0], array[1]);
                    }
                    else if (array.Length >= 2)
                    {
                        string[] newArray = new string[array.Length - 1];
                        array.CopyTo(newArray, 1);
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.AppendJoin('=', newArray);
                        _languageMap.Add(array[0], stringBuilder.ToString());
                    }
                    else
                    {
                        throw new ArgumentException("Wrong Line: " + text);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            SetLanguage("chinese");
            // SetLanguage(PlayerPrefs.GetString("Language", "chinese"));
            isDone = true;
        }

        public string GetText(string key)
        {
            return _languageMap.ContainsKey(key) ? _languageMap[key] : key;
        }
    }
}