#nullable enable
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
using DRFV.Select;
using DRFV.Setting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DRFV.Global
{
    public static class Util
    {
        public static Dictionary<string, EndType> endTypeFromShort = new();
        public static Regex keywordRegex = new("[^a-z0-9-_]");
        private static NoteJudgeRange[] noteJudgeRanges;

        public static void Init()
        {
            endTypeFromShort = DeserializeObject<Dictionary<string, EndType>>("end_type_short");
            noteJudgeRanges = DeserializeObject<NoteJudgeRange[]>("note_judge_range");
        }

        private static T? DeserializeObject<T>(string path)
        {
            return JsonConvert.DeserializeObject<T>(Resources.Load<TextAsset>(path).text);
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
            if (directoryInfo.Parent == null || directoryInfo.Parent.Exists)
            {
                directoryInfo.Create();
            }
            else
            {
                CreateDirectory(directoryInfo.Parent);
            }
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
            return id switch
            {
                0 => new NoteJudgeRange { displayName = "Cytus2", PJ = 70, P = 200, G = 400 },
                1 => new NoteJudgeRange { displayName = "Phigros", PJ = 40, P = 80, G = 160 },
                2 => new NoteJudgeRange { displayName = "DR3 NORMAL", PJ = 30, P = 60, G = 100 },
                3 => new NoteJudgeRange { displayName = "Arcaea", PJ = 25, P = 50, G = 100 },
                4 => new NoteJudgeRange { displayName = "DR3 HARD", PJ = 20, P = 40, G = 60 },
                5 => new NoteJudgeRange { displayName = "DR3 EXTREME", PJ = 10, P = 20, G = 30 },
                6 => new NoteJudgeRange { displayName = "DR3 EXTREME+", PJ = 10, P = 10, G = 10 },
                _ => new NoteJudgeRange { displayName = "", PJ = 30, P = 60, G = 100 }
            };
        }

        public static int GetNoteJudgeRangeLimit()
        {
            return 2;
        }

        public static int GetNoteJudgeRangeCount()
        {
            return 7;
        }

        public static float Variance(this float[] data)
        {
            float average = data.Average();
            return (float)data.Sum(x => Math.Pow(x - average, 2)) / data.Length;
        }

        public static int ScoreToRank(int score)
        {
            switch (score)
            {
                case < 0:
                case > 3000000:
                    return -1;
                case < 2000000:
                    return 0; // F
                case < 2100000:
                    return 1; // D
                case < 2200000:
                    return 2; // C
                case < 2300000:
                    return 3; // B
                case < 2400000:
                    return 4; // B+
                case < 2450000:
                    return 5; // A
                case < 2500000:
                    return 6; // A+
                case < 2550000:
                    return 7; // AA
                case < 2600000:
                    return 8; // AA+
                case < 2650000:
                    return 9; // AAA
                case < 2700000:
                    return 10; // AAA+
                case < 2750000:
                    return 11; // S
                case < 2800000:
                    return 12; // S+
                case < 2850000:
                    return 13; // SS
                case < 2900000:
                    return 14; // SS+
                case < 2950000:
                    return 15; // SSS
                case < 3000000:
                    return 16; // SSS+
                default:
                    return 17; // APJ
            }
        }
    }
}