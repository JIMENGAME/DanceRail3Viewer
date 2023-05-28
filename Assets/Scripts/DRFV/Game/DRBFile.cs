using System;
using System.Collections.Generic;
using System.Text;
using DRFV.Enums;
using DRFV.Game.HPBars;
using DRFV.Global;
using DRFV.inokana;
using UnityEngine;

namespace DRFV.Game
{
    public class DRBFile
    {
        public string ndname = "";
        public float offset;
        public float beat = 1;
        public List<BPM> bpms = new();
        public List<SCS> scs = new();
        public List<NoteData> notes = new();
        public int noteWeightCount;
        public bool noPos;
        public AnimationCurve BPMCurve = null, SCCurve = null;

        private DRBFile()
        {
        }

        private string GetMd5String()
        {
            List<string> chartLines = new List<string>();
            chartLines.Add("#OFFSET=" + offset.ToString("0.##"));
            chartLines.Add("#BEAT=" + beat.ToString("0.##"));
            chartLines.Add("#BPM_NUMBER=" + bpms.Count + ";");
            for (int i = 0; i < bpms.Count; i++)
            {
                BPM bpm = bpms[i];
                chartLines.Add("#BPM [" + i + "]=" + Util.FloatToDRBDecimal(bpm.bpm));
                chartLines.Add("#BPMS[" + i + "]=" + Util.FloatToDRBDecimal(bpm.bpms));
            }

            chartLines.Add("#SCN=" + scs.Count + ";");
            for (int i = 0; i < scs.Count; i++)
            {
                SCS sc = scs[i];
                chartLines.Add("#SC [" + i + "]=" + Util.FloatToDRBDecimal(sc.sc));
                chartLines.Add("#SCI[" + i + "]=" + sc.sci.ToString("0.000"));
            }

            if (noPos)
            {
                chartLines.Add("#NOPOS");
            }

            Dictionary<int, NoteData.MD5Data> exportedData = new();
            List<NoteData> tmpNotes = new List<NoteData>();
            foreach (NoteData data in notes)
            {
                tmpNotes.Add(data);
            }

            tmpNotes.Sort((a, b) =>
            {
                if (!IsSame(a.time, b.time))
                {
                    return a.time - b.time < 0 ? -1 : 1;
                }

                if (a.kind != b.kind)
                {
                    return (int)a.kind - (int)b.kind;
                }

                if (!IsSame(a.pos, b.pos))
                {
                    return a.pos - b.pos < 0 ? -1 : 1;
                }

                if (!IsSame(a.width, b.width))
                {
                    return a.width - b.width < 0 ? -1 : 1;
                }

                if (a.nsc.ToString() != b.nsc.ToString())
                {
                    if (a.nsc.type == b.nsc.type)
                    {
                        if (a.nsc.type == NoteSCType.SINGLE) return a.nsc.value - b.nsc.value < 0 ? -1 : 1;
                        return a.nsc.data[0].realValue - b.nsc.data[0].realValue < 0 ? -1 : 1;
                    }

                    return a.nsc.type == NoteSCType.SINGLE ? -1 : 1;
                }

                return 0;
            });
            foreach (NoteData noteData in tmpNotes)
            {
                NoteData.MD5Data exportedDatum = noteData.GetMd5Data();
                if (exportedData.ContainsKey(exportedDatum.id))
                {
                    NotificationBarManager.Instance.Show("警告：当前选中的曲目有谱面重号");
                    continue;
                }

                exportedData.Add(exportedDatum.id, exportedDatum);
            }

            foreach (NoteData.MD5Data exportedDatum in exportedData.Values)
            {
                if (exportedDatum.parent != null)
                {
                    chartLines.Add(exportedDatum.content + "\\" + (exportedData.ContainsKey((int)exportedDatum.parent)
                        ? exportedData[(int)exportedDatum.parent].content
                        : exportedDatum.content));
                }
                else
                {
                    chartLines.Add(exportedDatum.content);
                }
            }

            return String.Join("/", chartLines);

            bool IsSame(float a, float b)
            {
                return MathF.Abs(a - b) < 0.01;
            }
        }

        public static DRBFile Parse(string chart)
        {
            DRBFile drbFile = new DRBFile();
            string[] chartLines = chart.Split("\n");
            for (int i = 0; i < chartLines.Length; i++)
            {
                //空き行を抜く
                if (chartLines[i] == "") continue;
                //命令行を認識
                if (chartLines[i].Substring(0, 1) == "#")
                {
                    //OFFSET認識
                    if (chartLines[i].StartsWith("#OFFSET"))
                    {
                        string ss = chartLines[i];
                        ss = ss.Replace("#OFFSET=", "");
                        ss = ss.Replace(";", "");
                        drbFile.offset = float.Parse(ss.Trim());
                    }
                    else //BEAT認識
                    if (chartLines[i].StartsWith("#BEAT"))
                    {
                        string ss = chartLines[i];
                        ss = ss.Replace("#BEAT=", "");
                        ss = ss.Replace(";", "");
                        drbFile.beat = float.Parse(ss.Trim());
                    }
                    else //BPM [i]認識
                    if (chartLines[i].StartsWith("#BPM [" + drbFile.bpms.Count + "]"))
                    {
                        string ss = chartLines[i];
                        string ss2 = chartLines[i + 1];
                        ss = ss.Replace("#BPM [" + drbFile.bpms.Count + "]=", "");
                        ss = ss.Replace(";", "");
                        ss2 = ss2.Replace("#BPMS[" + drbFile.bpms.Count + "]=", "");
                        ss2 = ss2.Replace(";", "");
                        BPM bpm = new BPM
                        {
                            bpm = float.Parse(ss),
                            bpms = float.Parse(ss2)
                        };
                        drbFile.bpms.Add(bpm);
                    }
                    else //SC [i]認識
                    if (chartLines[i].StartsWith("#SC [" + drbFile.scs.Count + "]"))
                    {
                        string ss = chartLines[i].Trim();
                        string ss2 = chartLines[i + 1].Trim();
                        ss = ss.Replace("#SC [" + drbFile.scs.Count + "]=", "");
                        ss = ss.Replace(";", "");
                        ss2 = ss2.Replace("#SCI[" + drbFile.scs.Count + "]=", "");
                        ss2 = ss2.Replace(";", "");
                        SCS sc = new SCS
                        {
                            sc = float.Parse(ss),
                            sci = float.Parse(ss2)
                        };
                        drbFile.scs.Add(sc);
                    }
                    else //NoteDesigner認識;
                    if (chartLines[i].StartsWith("#NDNAME"))
                    {
                        string ss = chartLines[i].Trim();
                        ss = ss.Replace("#NDNAME=", "");
                        ss = ss.Replace(";", "");
                        //ss = ss.Replace("\'", "");
                        drbFile.ndname = ss;
                    }
                    else if (chartLines[i].StartsWith("#NOPOS"))
                    {
                        drbFile.noPos = true;
                    }
                    else
                    {
                        if (chartLines[i].StartsWith("#BPM_NUMBER") ||
                            chartLines[i].StartsWith("#SCN") ||
                            chartLines[i].StartsWith("#SCI[") ||
                            chartLines[i].StartsWith("#BPMS[")) continue;
                        Debug.LogError("Error: Unknown command: " + chartLines[i]);
                    }
                }

                //ノーツ行を認識
                else if (chartLines[i].StartsWith("<"))
                {
                    //Notesデータ取得
                    string ss = chartLines[i].Replace("<", "").Trim();
                    string[] sss = ss.Substring(0, ss.Length - 1).Split('>');
                    NoteData note = new NoteData();
                    note.id = int.Parse(sss[0].Trim());
                    note.kind = (NoteKind)int.Parse(sss[1].Trim());
                    if (note.kind == NoteKind.FAKE_CENTER) note.kind = NoteKind.SLIDE_CENTER;
                    if (note.kind == NoteKind.FAKE) note.kind = NoteKind.SLIDE_END;
                    note.time = float.Parse(sss[2].Trim());
                    note.pos = float.Parse(sss[3].Trim());
                    note.width = float.Parse(sss[4].Trim());

                    note.nsc = NoteData.NoteSC.Parse(sss[5].Trim());
                    if (note.nsc.value == 0.0f) note.nsc = NoteData.NoteSC.GetCommonNSC();

                    note.parent = int.Parse(sss[6].Trim());
                    note.mode = sss.Length > 7 ? ParseNoteAppearMode.ParseToMode(sss[7].Trim()) : NoteAppearMode.None;

                    drbFile.notes.Add(note);

                    drbFile.noteWeightCount += HPBar.NoteWeight[(int)note.kind];
                }
                else
                {
                    throw new ArgumentException("Unknown Line: " + chartLines[i]);
                }
            }

            drbFile.bpms.Sort((a, b) => Mathf.RoundToInt(a.bpms * 1000.0f - b.bpms * 1000.0f));
            drbFile.scs.Sort((a, b) => Mathf.RoundToInt(a.sci * 1000.0f - b.sci * 1000.0f));
            drbFile.notes.Sort((a, b) => a.id - b.id);

            return drbFile;
        }

        public void GenerateAttributesOnPlay()
        {
            GenerateBpmCurve();
            GenerateSCCurve();

            for (var i = 0; i < notes.Count; i++)
            {
                var note = notes[i];
                //计算每个音符的位置
                note.ms = BPMCurve.Evaluate(note.time);
                note.dms = SCCurve.Evaluate(note.ms);
            }

            for (var i = 0; i < notes.Count; i++)
            {
                var note = notes[i];
                for (int j = 0; j < notes.Count; j++)
                {
                    if (note.parent == notes[j].id)
                    {
                        note.parent = j;
                        break;
                    }
                }
            }

            for (var i = 0; i < notes.Count; i++)
            {
                var note = notes[i];
                if (note.IsTail())
                {
                    note.parent_time = notes[note.parent].time;
                    note.parent_ms = notes[note.parent].ms;
                    note.parent_dms = notes[note.parent].dms;
                    note.parent_pos = notes[note.parent].pos;
                    note.parent_width = notes[note.parent].width;
                }
            }
        }

        public string GetMD5()
        {
            return Util.GetMD5(Encoding.UTF8.GetBytes(GetMd5String()));
        }

        public struct BPM
        {
            public float bpm;
            public float bpms;
        }

        public struct SCS
        {
            public float sc;
            public float sci;
        }

        private void GenerateBpmCurve()
        {
            if (bpms.Count < 1) return;
            Keyframe[] BPMKeyframe = new Keyframe[bpms.Count + 1];

            BPMKeyframe[0] = new Keyframe(0.0f, offset * 1000.0f);
            float[] BPM_REALTIME = new float[bpms.Count + 1];

            for (int i = 0; i < bpms.Count; i++)
            {
                if (i == 0)
                {
                    BPM_REALTIME[i] = offset * 1000.0f;
                }
                else
                {
                    BPM_REALTIME[i] =
                        (bpms[i].bpms - bpms[i - 1].bpms) *
                        (60 / bpms[i - 1].bpm * 4 * beat) * 1000.0f + BPM_REALTIME[i - 1];
                }
            }

            BPM_REALTIME[bpms.Count] =
                (10000 - bpms[^1].bpms) *
                (60 / bpms[^1].bpm * 4 * beat) * 1000.0f +
                BPM_REALTIME[bpms.Count - 1];
            for (int i = 1; i < bpms.Count; i++)
            {
                BPMKeyframe[i] = new Keyframe(bpms[i].bpms, BPM_REALTIME[i]);
            }

            BPMKeyframe[bpms.Count] = new Keyframe(10000, BPM_REALTIME[bpms.Count]);
            Util.LinearKeyframe(BPMKeyframe);
            BPMCurve = new AnimationCurve(BPMKeyframe);
        }

        private void GenerateSCCurve()
        {
            if (BPMCurve == null) return;
            if (scs.Count == 0)
            {
                SCS sCNS = new SCS();
                sCNS.sc = 1;
                sCNS.sci = 0;
                scs.Add(sCNS);
            }

            float[] SCR = new float[scs.Count + 1];
            SCR[0] = BPMCurve.Evaluate(scs[0].sci);
            for (int i = 1; i < scs.Count; i++)
            {
                SCR[i] = SCR[i - 1] +
                         (BPMCurve.Evaluate(scs[i].sci) - BPMCurve.Evaluate(scs[i - 1].sci)) *
                         scs[i - 1].sc;
            }

            SCR[scs.Count] = SCR[scs.Count - 1] +
                             (BPMCurve.Evaluate(10000) -
                              BPMCurve.Evaluate(scs[^1].sci)) *
                             scs[^1].sc;

            Keyframe[] SCKeyframe = new Keyframe[scs.Count + 2];

            SCKeyframe[0] = new Keyframe(-10000.0f, -10000.0f);
            SCKeyframe[1] = new Keyframe(0.0f, 0.0f);

            for (int i = 0; i < scs.Count; i++)
            {
                SCKeyframe[i + 1] = new Keyframe(BPMCurve.Evaluate(scs[i].sci), SCR[i]);
            }

            SCKeyframe[scs.Count + 1] = new Keyframe(BPMCurve.Evaluate(10000), SCR[scs.Count]);

            Util.LinearKeyframe(SCKeyframe);

            SCCurve = new AnimationCurve(SCKeyframe);
        }
    }

    public class NoteData
    {
        public int id;
        public NoteKind kind;
        public float time;
        public float pos;
        public float width;
        public NoteSC nsc;
        public int parent;
        public float maxtime;
        public NoteAppearMode mode = NoteAppearMode.None;

        public float parent_time;
        public float parent_ms;
        public float parent_dms;
        public float parent_pos;
        public float parent_width;

        public float center;
        public float ms;
        public float dms;

        public bool isJudgeTimeRangeConflicted;

        public MD5Data GetMd5Data()
        {
            return new MD5Data
            {
                content = GetMd5String(),
                id = id,
                parent = this.IsTail() ? parent : null
            };
        }

        private string GetMd5String()
        {
            return $"{(int)kind}+{time:0.00000}+{Util.FloatToDRBDecimal(pos)}+{Util.FloatToDRBDecimal(width)}+{nsc}";
        }

        public class MD5Data
        {
            public string content;
            public int id;
            public int? parent;
        }

        public class NoteSC
        {
            public NoteSCType type;
            public float value = 1.0f;
            public List<MultiData> data = new();

            private NoteSC()
            {
            }

            public static NoteSC GetCommonNSC()
            {
                return new NoteSC
                {
                    type = NoteSCType.SINGLE,
                    value = 1
                };
            }

            public static NoteSC Parse(string str)
            {
                NoteSC noteSc = new NoteSC();
                noteSc.type = str.Contains(":") ? NoteSCType.MULTI : NoteSCType.SINGLE;
                switch (noteSc.type)
                {
                    case NoteSCType.SINGLE:
                        noteSc.value = float.Parse(str);
                        break;
                    case NoteSCType.MULTI:
                        string[] split = str.Split(";");
                        foreach (string s in split)
                        {
                            noteSc.data.Add(MultiData.Parse(s));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return noteSc;
            }

            public override string ToString()
            {
                return type switch
                {
                    NoteSCType.SINGLE => Util.FloatToDRBDecimal(value),
                    NoteSCType.MULTI => String.Join(";", data),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            public class MultiData
            {
                public float realValue;
                public float value;

                private MultiData()
                {
                }

                public static MultiData Parse(string str)
                {
                    string[] split = str.Split(":");
                    if (split.Length != 2) throw new ArgumentException();
                    return new MultiData
                    {
                        realValue = float.Parse(split[0]),
                        value = float.Parse(split[1])
                    };
                }

                public override string ToString()
                {
                    return $"{Util.FloatToDRBDecimal(realValue)}:{Util.FloatToDRBDecimal(value)}";
                }
            }
        }
    }
}