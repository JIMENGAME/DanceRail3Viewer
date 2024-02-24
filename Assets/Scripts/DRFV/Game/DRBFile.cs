#define USE_BPM_CURVE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DRFV.Enums;
using DRFV.Game.HPBars;
using DRFV.Global;
using DRFV.Global.Utilities;
using DRFV.inokana;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
        public List<NoteData> fakeNotes = new();
        public int noteWeightCount;
        public bool noPos;
        private AnimationCurve BPMCurve = null;
        public AnimationCurve SCCurve = null;
        public int TotalNotes => notes.Count;
        public float LastNoteTime { get; private set; }
        public float SingleHp { get; private set; }
        private List<BPMCalculate> _bpmCalculates;

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
                chartLines.Add("#BPM [" + i + "]=" + GameUtil.FloatToDRBDecimal(bpm.bpm));
                chartLines.Add("#BPMS[" + i + "]=" + GameUtil.FloatToDRBDecimal(bpm.time));
            }

            chartLines.Add("#SCN=" + scs.Count + ";");
            for (int i = 0; i < scs.Count; i++)
            {
                SCS sc = scs[i];
                chartLines.Add("#SC [" + i + "]=" + GameUtil.FloatToDRBDecimal(sc.sc));
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

            foreach (NoteData data in fakeNotes)
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
                try
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
                                time = float.Parse(ss2)
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
                                // sc = Mathf.Clamp(float.Parse(ss), -1000f, 1000f),
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
                        NoteData note = NoteData.Parse(chartLines[i]);

                        if (note.isFake)
                        {
                            drbFile.fakeNotes.Add(note);
                        }
                        else
                        {
                            drbFile.notes.Add(note);
                            drbFile.noteWeightCount += HPBar.NoteWeight[(int)note.kind];
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Unknown Line");
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("Error Statement: " + chartLines[i]);
                    throw;
                }
            }

            drbFile.bpms.Sort((a, b) => Mathf.RoundToInt(a.time * 1000.0f - b.time * 1000.0f));
            drbFile.scs.Sort((a, b) => Mathf.RoundToInt(a.sci * 1000.0f - b.sci * 1000.0f));
            drbFile.notes.Sort((a, b) => a.id - b.id);

            return drbFile;
        }

        public void GenerateAttributesOnPlay(int tier)
        {
            // Stopwatch stopwatch = new Stopwatch();
            // stopwatch.Start();
#if USE_BPM_CURVE
            GenerateBpmCurve(notes.OrderByDescending(noteData => noteData.time).ToArray()[0].time);
#else
            GenerateBpmEvent();
#endif
            GenerateSCCurve();


            SingleHp = CalculateSingleHp(TotalNotes, tier);

            foreach (var note in notes)
            {
                //计算每个音符的位置
                note.ms = CalculateDRBFileTime(note.time);
                note.dms = SCCurve.Evaluate(note.ms);
            }

            foreach (var note in fakeNotes)
            {
                //计算每个音符的位置
                note.ms = CalculateDRBFileTime(note.time);
                note.dms = SCCurve.Evaluate(note.ms);
            }

            // stopwatch.Stop();
            // Debug.Log(stopwatch.Elapsed.TotalMilliseconds);

            List<float> timeList = new();
            timeList.AddRange(notes.Select(data => data.ms));
            timeList.AddRange(fakeNotes.Select(data => data.ms));

            LastNoteTime = timeList.Count > 0 ? timeList.OrderByDescending(time => time).ToList()[0] : -1f;

            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].realId = i;
            }

            for (int i = 0; i < fakeNotes.Count; i++)
            {
                fakeNotes[i].realId = i;
            }

            foreach (var note in notes)
            {
                for (int j = 0; j < notes.Count; j++)
                {
                    if (note.parent == notes[j].id)
                    {
                        note.parent = j;
                        notes[j].isLast = false;
                        break;
                    }
                }
            }

            foreach (var note in fakeNotes)
            {
                for (int j = 0; j < fakeNotes.Count; j++)
                {
                    if (note.parent == fakeNotes[j].id)
                    {
                        note.parent = j;
                        fakeNotes[j].isLast = false;
                        break;
                    }
                }
            }

            foreach (var note in notes.Where(note => note.IsTail()))
            {
                note.parent_time = notes[note.parent].time;
                note.parent_ms = notes[note.parent].ms;
                note.parent_dms = notes[note.parent].dms;
                note.parent_pos = notes[note.parent].pos;
                note.parent_width = notes[note.parent].width;
            }

            foreach (var note in fakeNotes.Where(note => note.IsTail()))
            {
                note.parent_time = fakeNotes[note.parent].time;
                note.parent_ms = fakeNotes[note.parent].ms;
                note.parent_dms = fakeNotes[note.parent].dms;
                note.parent_pos = fakeNotes[note.parent].pos;
                note.parent_width = fakeNotes[note.parent].width;
            }
        }

        public string GetMD5()
        {
            return Util.GetMD5(Encoding.UTF8.GetBytes(GetMd5String()));
        }

        public struct BPM
        {
            public float bpm;
            public float time;
        }

        public struct SCS
        {
            public float sc;
            public float sci;
        }

        private class BPMCalculate
        {
            public float startTime;
            public float endTime;
            public float value;

            public BPMCalculate(float bpm, float startTime)
            {
                value = bpm;
                this.startTime = startTime;
            }
        }

        public float CalculateDRBFileTime(float time)
        {
#if USE_BPM_CURVE
            return BPMCurve.Evaluate(time);
#else
            if (_bpmCalculates == null) return Single.NaN;
            var realTime = 0f;
            foreach (var i in _bpmCalculates)
            {
                if (time > i.endTime)
                {
                    realTime += (i.endTime - i.startTime) * (60f / i.value);
                }
                else if (time >= i.startTime)
                {
                    realTime += (time - i.startTime) * (60f / i.value);
                    // break;
                }
            }

            return realTime * 1000f * (beat * 4f) + offset * 1000.0f;
#endif
        }

#if USE_BPM_CURVE
        private void GenerateBpmCurve(float endTime)
        {
            if (bpms.Count < 1) return;
            Keyframe[] BPMKeyframe = new Keyframe[bpms.Count + 1];

            BPMKeyframe[0] = new Keyframe(0.0f, offset * 1000.0f);
            float[] BPM_REALTIME = new float[bpms.Count + 1];

            BPM_REALTIME[0] = offset * 1000.0f;
            for (int i = 1; i < bpms.Count; i++)
            {
                BPM_REALTIME[i] =
                    (bpms[i].time - bpms[i - 1].time) * (4 * beat) *
                    (60 / bpms[i - 1].bpm) * 1000.0f + BPM_REALTIME[i - 1];
            }

            BPM_REALTIME[bpms.Count] =
                (endTime - bpms[^1].time) * (4 * beat) *
                (60 / bpms[^1].bpm) * 1000.0f +
                BPM_REALTIME[bpms.Count - 1];
            for (int i = 1; i < bpms.Count; i++)
            {
                BPMKeyframe[i] = new Keyframe(bpms[i].time, BPM_REALTIME[i]);
            }

            BPMKeyframe[bpms.Count] = new Keyframe(endTime, BPM_REALTIME[bpms.Count]);
            Util.LinearKeyframe(BPMKeyframe);
            BPMCurve = new AnimationCurve(BPMKeyframe);
        }
#else
        private void GenerateBpmEvent()
        {
            _bpmCalculates = new List<BPMCalculate>();
            bpms.ToList().ForEach(bpm =>
            {
                _bpmCalculates.Add(new BPMCalculate(bpm.bpm, bpm.bpms));
                if (_bpmCalculates.Count >= 2)
                {
                    _bpmCalculates[^2].endTime = _bpmCalculates[^1].startTime;
                }
            });
            _bpmCalculates[^1].endTime = Single.PositiveInfinity;
        }
#endif
        private void GenerateSCCurve()
        {
            // if (BPMCurve == null) return;
            if (scs.Count == 0)
            {
                SCS sCNS = new SCS();
                sCNS.sc = 1;
                sCNS.sci = 0;
                scs.Add(sCNS);
            }

            float[] SCR = new float[scs.Count + 1];
            SCR[0] = CalculateDRBFileTime(scs[0].sci);
            for (int i = 1; i < scs.Count; i++)
            {
                SCR[i] = SCR[i - 1] +
                         (CalculateDRBFileTime(scs[i].sci) - CalculateDRBFileTime(scs[i - 1].sci)) *
                         scs[i - 1].sc;
            }

            SCR[scs.Count] = SCR[scs.Count - 1] +
                             (CalculateDRBFileTime(10000) -
                              CalculateDRBFileTime(scs[^1].sci)) *
                             scs[^1].sc;

            Keyframe[] SCKeyframe = new Keyframe[scs.Count + 2];

            SCKeyframe[0] = new Keyframe(-10000.0f, -10000.0f);
            SCKeyframe[1] = new Keyframe(0.0f, 0.0f);

            for (int i = 0; i < scs.Count; i++)
            {
                SCKeyframe[i + 1] = new Keyframe(CalculateDRBFileTime(scs[i].sci), SCR[i]);
            }

            SCKeyframe[scs.Count + 1] = new Keyframe(CalculateDRBFileTime(10000), SCR[scs.Count]);

            Util.LinearKeyframe(SCKeyframe);

            SCCurve = new AnimationCurve(SCKeyframe);
        }

        private float CalculateSingleHp(int maxCombo, int tier)
        {
            float hpCoefficient = tier >= 14 ? 0.8f : 1;

            // print($"{recallCoefficient}, {maxCombo}, {rating}, {CurrentHpBarItem.HpBarType}");

            if (maxCombo >= 1000) return (96f / maxCombo + 0.08f) * hpCoefficient;
            if (maxCombo >= 600) return (32f / maxCombo + 0.2f) * hpCoefficient;
            if (maxCombo > 0) return (80f / maxCombo + 0.2f) * hpCoefficient;

            return 0;
        }
    }

    public class NoteData
    {
        #region ChartConstant

        public int id;
        public NoteKind kind;
        public float time;
        public float pos;
        public float width;
        public NoteSC nsc;
        public int parent;
        public float maxtime;
        public NoteAppearMode mode = NoteAppearMode.None;
        public bool isFake;

        #endregion

        #region RuntimeGenerated

        public int realId;
        public float parent_time;
        public float parent_ms;
        public float parent_dms;
        public float parent_pos;
        public float parent_width;

        public float center;
        public float ms;
        public float dms;

        public bool isJudgeTimeRangeConflicted;
        public bool isLast = true;

        #endregion

        public static NoteData Parse(string line)
        {
            NoteData note = new NoteData();
            string ss = line.Replace("<", "").Trim();
            string[] sss = ss.Substring(0, ss.Length - 1).Split('>');

            note.id = int.Parse(sss[0].Trim());
            note.kind = (NoteKind)int.Parse(sss[1].Trim());
            if (note.kind == NoteKind.FAKE_CENTER) note.kind = NoteKind.SLIDE_CENTER;
            if (note.kind == NoteKind.FAKE) note.kind = NoteKind.SLIDE_END;
            note.time = float.Parse(sss[2].Trim());
            note.pos = float.Parse(sss[3].Trim());
            note.width = float.Parse(sss[4].Trim());

            note.nsc = NoteSC.Parse(sss[5].Trim());
            if (note.nsc.value == 0.0f) note.nsc = NoteSC.GetCommonNSC();

            note.parent = int.Parse(sss[6].Trim());
            if (sss.Length > 7) note.mode = ParseNoteAppearMode.ParseToMode(sss[7].Trim());
            if (sss.Length > 8) note.isFake = int.Parse(sss[7]) == 1;

            return note;
        }

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
            List<string> list = new List<string>();
            list.Add(((int)kind).ToString());
            list.Add(time.ToString("0.00000"));
            list.Add(GameUtil.FloatToDRBDecimal(pos));
            list.Add(GameUtil.FloatToDRBDecimal(width));
            list.Add(mode == NoteAppearMode.Jump ? (this.IsTail() ? "0" : "1") : nsc.ToString());
            if (mode != NoteAppearMode.None)
            {
                list.Add(ParseNoteAppearMode.ParseToString(mode));
            }

            if (isFake)
            {
                list.Add("1");
            }

            return string.Join(",", list);
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
                try
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
                catch (Exception)
                {
                    Debug.LogError("Error Statement: " + str);
                    throw;
                }
            }

            public override string ToString()
            {
                return type switch
                {
                    NoteSCType.SINGLE => GameUtil.FloatToDRBDecimal(value),
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
                    try
                    {
                        string[] split = str.Split(":");
                        if (split.Length != 2) throw new ArgumentException();
                        return new MultiData
                        {
                            realValue = float.Parse(split[0]),
                            value = float.Parse(split[1])
                        };
                    }
                    catch (Exception)
                    {
                        Debug.LogError("Error Statement: " + str);
                        throw;
                    }
                }

                public override string ToString()
                {
                    return $"{GameUtil.FloatToDRBDecimal(realValue)}:{GameUtil.FloatToDRBDecimal(value)}";
                }
            }
        }
    }
}