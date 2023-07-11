using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DRFV.Enums;
using DRFV.JsonData;
using DRFV.Game;
using DRFV.inokana;
using DRFV.Result;
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
#if false
        private static readonly AnimationCurve OneThirdWeightedCurve =
            new(new Keyframe(0, 0, 0, 0, 1 / 3f, 1 / 3f), new Keyframe(1, 1, 0, 0, 1 / 3f, 1 / 3f));
#endif

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
            foreach (KeyValuePair<string, JToken> valuePair in endTypeShortJObject)
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

        public static Grade ScoreToRank(int score)
        {
            return score switch
            {
                < 0 => Grade.ERR,
                > 3000000 =>  Grade.ERR,
                < 2000000 => Grade.F, // F
                < 2100000 => Grade.D, // D
                < 2200000 => Grade.C, // C
                < 2300000 => Grade.B, // B
                < 2400000 => Grade.B_Plus,
                < 2450000 => Grade.A,
                < 2500000 => Grade.A_Plus,
                < 2550000 => Grade.AA,
                < 2600000 => Grade.AA_Plus,
                < 2650000 => Grade.AAA,
                < 2700000 => Grade.AAA_Plus,
                < 2750000 => Grade.S,
                < 2800000 => Grade.S_Plus,
                < 2850000 => Grade.SS,
                < 2900000 => Grade.SS_Plus,
                < 2950000 => Grade.SSS,
                < 3000000 => Grade.SSS_Plus,
                _ => Grade.APJ
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public static Sprite ByteArrayToSprite(byte[] data)
        {
            Texture2D texture = LoadTexture2DFromByteArray(data);
            if (texture == null)
            {
                NotificationBarManager.Instance.Show("错误：不支持的图片格式");
                return SpritePlaceholder;
            }

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            return sprite;
        }

        public static Texture2D LoadTexture2DFromByteArray(byte[] data)
        {
            try
            {
                using UnimageProcessor unimageProcessor = new UnimageProcessor();
                unimageProcessor.Load(data);
                return unimageProcessor.GetTexture(noLongerReadable:false);
            }
            catch (UnimageException)
            {
                Texture2D texture = new Texture2D(0, 0);
                if (texture.LoadImage(data))
                {
                    return texture;
                }

                return null;
            }
        }
        
        public static float ScoreToRate(float score, int hard, float speed)
        {
            score = Mathf.Clamp(score, 0f, 3000000f);
            float k = score switch
            {
                <= 2400000f => score / 2400000f,
                < 2450000f => SongRateEase((score - 2400000f) / 50000f) * 0.1f + 0f,
                < 2500000f => SongRateEase((score - 2450000f) / 50000f) * 0.1f + 0.1f,
                < 2550000f => SongRateEase((score - 2500000f) / 50000f) * 0.2f + 0.2f,
                < 2600000f => SongRateEase((score - 2550000f) / 50000f) * 0.2f + 0.4f,
                < 2650000f => SongRateEase((score - 2600000f) / 50000f) * 0.2f + 0.6f,
                < 2700000f => SongRateEase((score - 2650000f) / 50000f) * 0.2f + 0.8f,
                < 2750000f => SongRateEase((score - 2700000f) / 50000f) * 0.2f + 1.0f,
                < 2800000f => SongRateEase((score - 2750000f) / 50000f) * 0.2f + 1.2f,
                < 2850000f => SongRateEase((score - 2800000f) / 50000f) * 0.3f + 1.4f,
                < 2900000f => SongRateEase((score - 2850000f) / 50000f) * 0.4f + 1.7f,
                < 2950000f => SongRateEase((score - 2900000f) / 50000f) * 0.4f + 2.1f,
                <= 3000000f => SongRateEase((score - 2950000f) / 50000f) * 0.5f + 2.5f,
                _ => Single.NaN
            };

            return (score > 2400000f) ? (k + hard * speed) : (k * hard * speed);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float SongRateEase(float x)
            {
                x = Mathf.Clamp01(x);
                return 3 * x * x - 2 * x * x * x;
            }
        }

        public delegate bool BooleanDelegate();

#if false
        public static AnimationCurve GetAnimationCurveFromDumpedFile(string path)
        {
            WrapMode postWrapMode = WrapMode.ClampForever, preWrapMode = WrapMode.ClampForever;
            bool keyPart = false;
            List<string> keys = new List<string>();
            List<Keyframe> keyframes = new List<Keyframe>();
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (keyPart)
                    {
                        if (line != "") keys.Add(line);
                        else
                        {
                            Keyframe keyframe = new Keyframe(0, 0);
                            foreach (string key in keys)
                            {
                                string[] keyValuePair = key.Split(": ");
                                if (keyValuePair.Length != 2) continue;
                                switch (keyValuePair[0])
                                {
                                    case "time":
                                        if (!float.TryParse(keyValuePair[1].Replace(",", ""), out float time))
                                            time = 0f;
                                        keyframe.time = time;
                                        break;
                                    case "value":
                                        if (!float.TryParse(keyValuePair[1].Replace(",", ""), out float value))
                                            value = 0f;
                                        keyframe.value = value;
                                        break;
                                    case "inTangent":
                                        if (!float.TryParse(keyValuePair[1].Replace(",", ""), out float inTangent))
                                            inTangent = 0f;
                                        keyframe.inTangent = inTangent;
                                        break;
                                    case "outTangent":
                                        if (!float.TryParse(keyValuePair[1].Replace(",", ""), out float outTangent))
                                            outTangent = 0f;
                                        keyframe.outTangent = outTangent;
                                        break;
                                    case "inWeight":
                                        if (!float.TryParse(keyValuePair[1].Replace(",", ""), out float inWeight))
                                            inWeight = 0f;
                                        keyframe.inWeight = inWeight;
                                        break;
                                    case "outWeight":
                                        if (!float.TryParse(keyValuePair[1].Replace(",", ""), out float outWeight))
                                            outWeight = 0f;
                                        keyframe.outWeight = outWeight;
                                        break;
                                    case "weightedMode":
                                        if (!Enum.TryParse(keyValuePair[1], true, out WeightedMode weightedMode))
                                            weightedMode = WeightedMode.None;
                                        keyframe.weightedMode = weightedMode;
                                        break;
                                }
                            }

                            keys.Clear();
                            keyframes.Add(keyframe);
                        }
                    }

                    if (line.StartsWith("postWrapMode: "))
                    {
                        string postWrapModeStr = line.Substring("postWrapMode: ".Length);
                        if (!Enum.TryParse(postWrapModeStr, true, out postWrapMode))
                        {
                            postWrapMode = WrapMode.ClampForever;
                        }
                    }

                    if (line.StartsWith("preWrapMode: "))
                    {
                        string preWrapModeStr = line.Substring("preWrapMode: ".Length);
                        if (!Enum.TryParse(preWrapModeStr, true, out preWrapMode))
                        {
                            preWrapMode = WrapMode.ClampForever;
                        }
                    }

                    if (line == "keys: ")
                    {
                        streamReader.ReadLine();
                        keyPart = true;
                    }
                }
            }

            AnimationCurve animationCurve = new AnimationCurve(keyframes.ToArray())
            {
                postWrapMode = postWrapMode,
                preWrapMode = preWrapMode
            };
            return animationCurve;
        }

        public static void DumpAnimationCurve(Stream stream, AnimationCurve animationCurve)
        {
            stream.WriteLine("postWrapMode: " + animationCurve.postWrapMode);
            stream.WriteLine("preWrapMode: " + animationCurve.preWrapMode);
            stream.WriteLine("keys: ");
            stream.WriteLine();
            Keyframe[] dumpTargetKeys = animationCurve.keys;
            foreach (Keyframe key in dumpTargetKeys)
            {
                stream.WriteLine("time: " + key.time.ToString("0.00000"));
                stream.WriteLine("value: " + key.value.ToString("0.00000"));
                stream.WriteLine("inTangent: " + key.inTangent.ToString("0.00000"));
                stream.WriteLine("outTangent: " + key.outTangent.ToString("0.00000"));
                stream.WriteLine("inWeight: " + key.inWeight.ToString("0.00000"));
                stream.WriteLine("outWeight: " + key.outWeight.ToString("0.00000"));
                stream.WriteLine("weightedMode: " + key.weightedMode);
                stream.WriteLine();
            }
        }

        public static void WriteLine(this Stream stream)
        {
            stream.WriteLine("\n");
        }

        public static void WriteLine(this Stream stream, string str)
        {
            stream.Write(Encoding.UTF8.GetBytes(str + "\n"));
        }
#endif
    }
}