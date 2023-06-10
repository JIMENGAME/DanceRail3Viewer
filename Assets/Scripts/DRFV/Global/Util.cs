using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DRFV.Enums;
using DRFV.JsonData;
using DRFV.Game;
using DRFV.inokana;
using DRFV.Select;
using DRFV.Setting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unimage;
using UnityEngine;

namespace DRFV.Global
{
    public static class Util
    {
        public static readonly Dictionary<string, EndType> endTypeFromShort = new();
        private static NoteJudgeRange[] noteJudgeRanges;
        public static readonly Regex keywordRegex = new("[^a-z0-9-_]");
        public static string localizationId = "zh-cn";
        private static readonly DateTime timeStampStart = new(1970, 1, 1);
        public static readonly string[] ImageSuffixes = { ".jpg", ".png", ".jpeg", ".webp", ".bmp", ".tga", ".gif" };
        public static Sprite SpritePlaceholder { get; private set; }
        public static readonly Color SpritePlaceholderBGColor = new(1f, 130f / 255f, 1f);

        ///
        ///         if (File.Exists(filePath + ".jpg")) filePath += ".jpg";
        ///         else if (File.Exists(filePath + ".png")) filePath += ".png";
        ///         else if (File.Exists(filePath + ".jpeg")) filePath += ".jpeg";
        ///         else if (File.Exists(filePath + ".webp")) filePath += ".webp";
        ///         else if (File.Exists(filePath + ".bmp")) filePath += ".bmp";
        ///         else if (File.Exists(filePath + ".tga")) filePath += ".tga";
        ///         else if (File.Exists(filePath + ".gif")) filePath += ".gif";
        /// 
        public static void Init()
        {
            var endTypeShortJObject = JObject.Parse(Resources.Load<TextAsset>("end_type_short").text);
            foreach (KeyValuePair<string, JToken?> valuePair in endTypeShortJObject)
            {
                endTypeFromShort.Add(valuePair.Key, valuePair.Value.ToObject<EndType>());
            }

            JArray noteJudgeRangesJArray = JArray.Parse(Resources.Load<TextAsset>("note_judge_range").text);
            noteJudgeRanges = new NoteJudgeRange[noteJudgeRangesJArray.Count];
            for (var i = 0; i < noteJudgeRangesJArray.Count; i++)
            {
                var token = noteJudgeRangesJArray[i];
                noteJudgeRanges[i] = token.ToObject<NoteJudgeRange>();
            }

            SpritePlaceholder = Resources.Load<Sprite>("placeholder");
        }

        public static bool[] GenerateNewBoolArray(int length)
        {
            bool[] bools = new bool[length];
            for (int i = 0; i < bools.Length; i++)
            {
                bools[i] = false;
            }

            return bools;
        }

        public static void LinearKeyframe(Keyframe[] keys)
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

        public static JObject ReadJson(string path)
        {
            string result = File.ReadAllText(path, Encoding.UTF8);
            return JObject.Parse(result);
        }

        public static void CreateSonglist()
        {
            if (File.Exists(StaticResources.Instance.dataPath + "Songlist.json")) return;
            WriteSonglist(new Songlist());
        }

        public static Songlist ReadSonglist()
        {
            return JsonConvert.DeserializeObject<Songlist>(
                File.ReadAllText(StaticResources.Instance.dataPath + "Songlist.json", Encoding.UTF8));
        }

        public static void WriteSonglist(SonglistItem songlistItem)
        {
            Songlist songlist = ReadSonglist();
            songlist.songs.Add(songlistItem);
            WriteSonglist(songlist);
        }

        public static void WriteSonglist(Songlist songlist)
        {
            File.WriteAllText(StaticResources.Instance.dataPath + "Songlist.json",
                JsonConvert.SerializeObject(songlist, Formatting.None), Encoding.UTF8);
        }

        public static string GetMd5OfChart(string keyword, int tier)
        {
            string path = StaticResources.Instance.dataPath + "songs/" + keyword +
                          "/" + tier +
                          ".txt";
            using StreamReader streamReader = new StreamReader(path, Encoding.UTF8);
            string line;
            List<string> list = new List<string>();
            while ((line = streamReader.ReadLine()) != null)
            {
                if (line.Trim() == "") continue;
                list.Add(line.Trim().Replace("\r", "").Replace("\t", ""));
            }

            string s = String.Join("\n", list.ToArray());
            DRBFile drbFile = DRBFile.Parse(s);
            return drbFile.GetMD5();
        }

        public static string GetMD5(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);
            byte[] bytes = new byte[fs.Length];
            if (fs.Read(bytes) != bytes.Length) throw new ArgumentException();
            return GetMD5(bytes);
        }

        public static string GetMD5(byte[] data)
        {
            byte[] result = MD5.Create().ComputeHash(data);
            StringBuilder sBuilder = new StringBuilder();
            foreach (var t in result)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static Color GetAvgColor(Sprite sprite)
        {
            Color[] pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height);
            float r = 0, g = 0, b = 0;
            foreach (var p in pixels)
            {
                if (p == new Color(0, 0, 0)) continue;
                r += p.r;
                g += p.g;
                b += p.b;
            }

            // Debug.Log($"{r}, {g}, {b}, {pixelCount}");
            r /= pixels.Length;
            g /= pixels.Length;
            b /= pixels.Length;
            // Debug.Log($"result: {r}, {g}, {b}");
            return new Color(r, g, b);
        }

        private static Color white = new(1.0f, 1.0f, 1.0f),
            blue = new(0.5f, 0.5f, 1.0f),
            yellow = new(1.0f, 1.0f, 0.5f),
            red = new(1.0f, 0.5f, 0.5f),
            purple = new(1.0f, 0.5f, 1.0f),
            grey = new(0.1f, 0.1f, 0.1f);

        public static Color GetTierColor(int hard)
        {
            return hard switch
            {
                0 => white,
                > 0 and < 6 => blue,
                > 5 and < 11 => yellow,
                > 10 and < 16 => red,
                > 15 and < 21 => purple,
                _ => grey
            };
        }

        public static string FloatToDRBDecimal(float dec)
        {
            string qwq = dec.ToString("0.00");
            return qwq.EndsWith(".00") ? qwq.Substring(0, qwq.Length - 3) : qwq;
        }

        public static string ToBase64(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        public static string FromBase64(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        public static void CreateDirectory(DirectoryInfo directoryInfo)
        {
            if (directoryInfo.Exists) return;
            if (directoryInfo.Parent is { Exists: false })
            {
                CreateDirectory(directoryInfo.Parent);
            }
            directoryInfo.Create();
        }

        public static string ParseScore(int score, int? maxDigit = null, bool? usePadding = null)
        {
            return ParseScore(score, GlobalSettings.CurrentSettings.ScoreType, maxDigit, usePadding);
        }

        public static string ParseScore(int score, SCORE_TYPE type, int? maxDigit = null, bool? usePadding = null)
        {
            return ParseScore(score, maxDigit ?? type switch
            {
                SCORE_TYPE.ORIGINAL => 7,
                SCORE_TYPE.ARCAEA => 8,
                SCORE_TYPE.PHIGROS => 7,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            }, usePadding ?? type switch
            {
                SCORE_TYPE.ORIGINAL => false,
                SCORE_TYPE.ARCAEA => false,
                SCORE_TYPE.PHIGROS => true,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            });
        }

        private static string ParseScore(int score, int maxDigit, bool usePadding)
        {
            string input = score + "";
            if (score < 0) input = input.Substring(1);
            string output = "";
            while (usePadding && input.Length < maxDigit)
            {
                input = "0" + input;
            }

            for (int i = input.Length - 1, l = 1; i >= 0; i--)
            {
                output = input[i] + output;
                if (l > 0 && i > 0 && l % 3 == 0)
                {
                    output = "," + output;
                    l = 0;
                }

                l++;
            }

            return (score < 0 ? "-" : "") + output;
        }

        public static Color HexToColor(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            switch (hex.Length)
            {
                case 3:
                    byte[] rgb = new byte[3];
                    for (int i = 0; i < 3; i++)
                    {
                        rgb[i] = byte.Parse(hex.Substring(i, 1), NumberStyles.HexNumber);
                    }

                    return new Color(rgb[0] / 255f, rgb[1] / 255f, rgb[2] / 255f, 1f);
                case 6:
                    byte[] rgb1 = new byte[3];
                    for (int i = 0; i < 3; i++)
                    {
                        rgb1[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
                    }

                    return new Color(rgb1[0] / 255f, rgb1[1] / 255f, rgb1[2] / 255f, 1f);
                case 8:
                    byte[] rgba = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        rgba[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
                    }

                    return new Color(rgba[0] / 255f, rgba[1] / 255f, rgba[2] / 255f, rgba[3] / 255f);
                default:
                    throw new ArgumentException("Illegal Hex Color: " + hex);
            }
        }


        public static Color ModifyAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Vector3 ModifyX(this Vector3 position, float x)
        {
            return new Vector3(x, position.y, position.z);
        }

        public static float SineOutEase(float a)
        {
            return Mathf.Sin(a * Mathf.PI / 2f);
        }

        public static NoteJudgeRange GetNoteJudgeRange(int id)
        {
            if (id == -1) return new NoteJudgeRange { displayName = "HADOU TEST", PJ = 30, P = 60, G = 100 };

            if (id < -1 || id > noteJudgeRanges.Length)
            {
                NotificationBarManager.Instance.Show("出了奇怪的错");
                throw new ArgumentOutOfRangeException("NoteRange");
            }

            return noteJudgeRanges[id];
        }

        public static int GetNoteJudgeRangeLimit()
        {
            return 3;
        }

        public static int GetNoteJudgeRangeCount()
        {
            return noteJudgeRanges.Length;
        }

        public static float Variance(this float[] data)
        {
            float average = data.Average();
            return (float)data.Sum(x => Math.Pow(x - average, 2)) / data.Length;
        }

        public static float StandardDeviation(this float[] data)
        {
            return Mathf.Pow(data.Variance(), 0.5f);
        }

        public static int ScoreToRank(int score)
        {
            return score switch
            {
                < 0 => -1,
                > 3000000 => -1,
                < 2000000 => 0, // F
                < 2100000 => 1, // D
                < 2200000 => 2, // C
                < 2300000 => 3, // B
                < 2400000 => 4, // B+
                < 2450000 => 5, // A
                < 2500000 => 6, // A+
                < 2550000 => 7, // AA
                < 2600000 => 8, // AA+
                < 2650000 => 9, // AAA
                < 2700000 => 10, // AAA+
                < 2750000 => 11, // S
                < 2800000 => 12, // S+
                < 2850000 => 13, // SS
                < 2900000 => 14, // SS+
                < 2950000 => 15, // SSS
                < 3000000 => 16, // SSS+
                _ => 17 // APJ
            };
        }

        public static string BoolArrayToString(bool[] arr)
        {
            StringBuilder sb = new StringBuilder();
            foreach (bool b in arr)
            {
                sb.Append(b ? "T" : "F");
            }

            return sb.ToString();
        }

        public static bool[] StringToBoolArray(string str)
        {
            bool[] result = new bool[str.Length];
            for (int i = 0; i < result.Length; i++)
            {
                string q = str.Substring(i, 1);
                result[i] = q == "T";
                if (!result[i] && q != "F") Debug.LogWarning("警告：不合法的参数");
            }

            return result;
        }

        public static long DateTimeToTimeStamp(DateTime? dateTime)
        {
            if (dateTime == null) return 0;

            TimeSpan ts = Convert.ToDateTime(dateTime).ToUniversalTime() -
                          timeStampStart; //ToUniversalTime()转换为标准时区的时间,去掉的话直接就用北京时间
            return Convert.ToInt64(ts.TotalSeconds);
        }

        public static DateTime TimeStampToDateTime(long seconds)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(timeStampStart); //当地时区
            return startTime.AddSeconds(seconds);
        }

        public static float TransformSongSpeed(int speed)
        {
            return speed switch
            {
                0 => 1.0f,
                1 => 1.1f,
                2 => 1.2f,
                3 => 1.3f,
                4 => 1.4f,
                5 => 1.5f,
                6 => 1.6f,
                7 => 1.7f,
                8 => 1.8f,
                9 => 1.9f,
                10 => 2.0f,
                11 => 2.1f,
                12 => 2.2f,
                13 => 2.3f,
                14 => 2.4f,
                15 => 2.5f,
                16 => 2.6f,
                17 => 2.7f,
                18 => 2.8f,
                19 => 2.9f,
                20 => 3.0f,
                _ => 1.0f
            };
        }

        public static void FullGC()
        {
            GC.Collect();
            GC.Collect();
            GC.Collect();
            GC.Collect();
            GC.Collect();
            GC.Collect();
            GC.Collect();
            GC.Collect();
        }

        public static AudioClip MonoToStereo(this AudioClip audioClip)
        {
            if (audioClip == null || audioClip.channels == 2) return audioClip;
            if (audioClip.channels != 1) throw new Exception();
            float[] data = new float[audioClip.samples], output = new float[audioClip.samples * 2];
            audioClip.GetData(data, 0);
            for (int i = 0; i < data.Length; i++)
            {
                output[2 * i] = data[i];
                output[2 * i + 1] = data[i];
            }

            AudioClip clip = AudioClip.Create(audioClip.name, audioClip.samples, 2, audioClip.frequency, false);
            clip.SetData(output, 0);
            return clip;
        }

        public static Sprite ByteArrayToSprite(byte[] data)
        {
            Texture2D texture;
            try
            {
                UnimageProcessor unimage = new UnimageProcessor();
                unimage.Load(data);
                texture = unimage.GetTexture(false, true, false);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                return sprite;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            texture = new Texture2D(0, 0);
            if (texture.LoadImage(data))
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                return sprite;
            }
            NotificationBarManager.Instance.Show("错误：不支持的图片格式");
            return SpritePlaceholder;
        }
    }
}