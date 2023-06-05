#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DRFV.Enums;
using DRFV.Game;
using DRFV.Global;
using DRFV.Select;
using DRFV.Setting;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DRFV.Test
{
    public class TheRO1GameManager : MonoBehaviour
    {
        private DRBFile drbfile;
        private GlobalSettings _globalSettings;
        private AnimationCurve BPMCurve;
        private SongDataContainer _songDataContainer;
        private int measuremax;
        private List<BPM> bpmList = new List<BPM>();
        private List<NOTE>[] noteList = new List<NOTE>[6];
        private List<int> list_hit = new List<int>();
        private List<int> list_flick = new List<int>();
        private int MODE = 4; //key数
        private float[] f_song;
        private float[] f_hit;
        private float[] f_flick;
        private int NOTECOUNT;
        public AudioSource BGMManager;

        public void Start()
        {
            _songDataContainer = GameObject.FindWithTag("").GetComponent<SongDataContainer>();
            _globalSettings = GlobalSettings.CurrentSettings;
            MODE = _globalSettings.GameModeDance == 0 ? 4 : 6;
        }

        private IEnumerator CLoadChart()
        {
            drbfile = new DRBFile();
            drbfile.bpms = new List<BPMS>();
            drbfile.note = new List<NoteData>();
            drbfile.noteHitflag = new List<bool>();
            SonglistReadin();
            drbfile.bpms.Sort((a, b) =>
                Mathf.RoundToInt(a.bpms * 1000.0f - b.bpms * 1000.0f));
            drbfile.note.Sort((a, b) =>
                Mathf.RoundToInt(a.time * 1000.0f - b.time * 1000.0f));

            Keyframe[] BPMKeyframe = new Keyframe[drbfile.bpms.Count + 1];
            BPMKeyframe[0] = new Keyframe(0.0f, drbfile.offset * 1000f);
            float[] BPM_REALTIME = new float[drbfile.bpms.Count + 1];
            for (int i = 0; i < drbfile.bpms.Count; ++i)
            {
                if (i == 0)
                {
                    BPM_REALTIME[i] = drbfile.offset * 1000f;
                }
                else
                {
                    BPM_REALTIME[i] = (drbfile.bpms[i].bpms - drbfile.bpms[i - 1].bpms) *
                        (60.0f / drbfile.bpms[i - 1].bpm * 4.0f * drbfile.beat) *
                        1000.0f + BPM_REALTIME[i - 1];
                }
            }

            BPM_REALTIME[drbfile.bpms.Count] =
                (10000.0f - drbfile.bpms[drbfile.bpms.Count - 1].bpms) *
                (60.0f / drbfile.bpms[drbfile.bpms.Count - 1].bpm * 4.0f *
                 drbfile.beat) * 1000.0f + BPM_REALTIME[drbfile.bpms.Count - 1];
            for (int i = 1; i < drbfile.bpms.Count; ++i)
            {
                BPMKeyframe[i] = new Keyframe(drbfile.bpms[i].bpms, BPM_REALTIME[i]);
            }

            BPMKeyframe[drbfile.bpms.Count] = new Keyframe(10000f, BPM_REALTIME[drbfile.bpms.Count]);
            Util.LinearKeyframe(BPMKeyframe);
            BPMCurve = new AnimationCurve(BPMKeyframe);

            for (int index = 0; index < drbfile.note.Count; ++index)
                drbfile.note[index].ms = BPMCurve.Evaluate(drbfile.note[index].time);
            List<float> list_all = new List<float>();
            List<float> floatList1 = new List<float>();
            List<float> floatList2 = new List<float>();
            List<float> floatList3 = new List<float>();
            List<float> floatList4 = new List<float>();
            List<float> floatList5 = new List<float>();
            float num1 = 0.0f;
            for (int index = 0; index < drbfile.note.Count; ++index)
            {
                float ms = drbfile.note[index].ms;
                if ((drbfile.note[index].kind == NoteKind.HOLD_END || drbfile.note[index].kind == NoteKind.SLIDE_END ||
                     drbfile.note[index].kind == NoteKind.HPass_END || drbfile.note[index].kind == NoteKind.LPass_END ||
                     drbfile.note[index].kind == NoteKind.MOVER_END) && !list_all.Contains(ms) &&
                    !floatList4.Contains(ms))
                {
                    list_all.Add(ms);
                    floatList4.Add(ms);
                }

                if ((drbfile.note[index].kind == NoteKind.BOOM || drbfile.note[index].kind == NoteKind.BOOM_END) &&
                    !list_all.Contains(ms) && !floatList5.Contains(ms))
                {
                    list_all.Add(ms);
                    floatList5.Add(ms);
                }
            }

            for (int index = 0; index < drbfile.note.Count; ++index)
            {
                float ms = drbfile.note[index].ms;
                if (num1 < drbfile.note[index].time)
                    num1 = drbfile.note[index].time;
                if (drbfile.note[index].kind == NoteKind.TAP || drbfile.note[index].kind == NoteKind.ExTAP)
                {
                    if (!list_all.Contains(ms))
                    {
                        if (!floatList1.Contains(ms))
                        {
                            list_all.Add(ms);
                            floatList1.Add(ms);
                        }
                    }
                    else if (!floatList2.Contains(ms))
                        floatList2.Add(ms);
                }

                if (drbfile.note[index].kind == NoteKind.FLICK || drbfile.note[index].kind == NoteKind.FLICK_LEFT ||
                    drbfile.note[index].kind == NoteKind.FLICK_RIGHT || drbfile.note[index].kind == NoteKind.FLICK_UP ||
                    drbfile.note[index].kind == NoteKind.FLICK_DOWN ||
                    drbfile.note[index].kind == NoteKind.HOLD_START ||
                    drbfile.note[index].kind == NoteKind.SLIDE_START)
                {
                    if (!list_all.Contains(ms))
                        list_all.Add(ms);
                    if (!floatList3.Contains(ms))
                        floatList3.Add(ms);
                }
            }

            measuremax = (int)num1 + 1;
            list_all.Sort();
            floatList1.Sort();
            floatList2.Sort();
            floatList3.Sort();
            floatList4.Sort();
            floatList5.Sort();
            for (int index = 0; index < 6; ++index)
                noteList[index] = new List<NOTE>();
            List<LANEDIC> lanedicList = new List<LANEDIC>();
            Random.InitState(_songDataContainer.selectedDiff * 929);
            int num2 = -1;
            float num3 =
                (float)((_songDataContainer.selectedDiff >= 15
                    ? 0.10000000149011612
                    : 0.25999999046325684 - 0.0099999997764825821 * _songDataContainer.selectedDiff) * MODE / 4.0);
            for (int i = 0; i < list_all.Count; i++)
            {
                float num4 = list_all[i] * (1f / 1000f);
                int num5 = 0;
                for (int index = lanedicList.Count - 1; index >= 0; --index)
                {
                    if (lanedicList[index].ms + num3 < num4)
                        lanedicList.RemoveAt(index);
                }

                while (lanedicList.Count > 3)
                    lanedicList.RemoveAt(3);
                if (floatList4.Exists(d => d == list_all[i]))
                {
                    int num6 = num2;
                    if (num6 >= 0)
                    {
                        lanedicList.Insert(0, new LANEDIC(num6, num4));
                        while (lanedicList.Count > 3)
                            lanedicList.RemoveAt(3);
                        MakeNotesTails(num6, num4);
                        ++num5;
                    }
                }
                else
                {
                    int lane = Random.Range(0, MODE);
                    bool flag1 = false;
                    bool flag2 = false;
                    if (floatList1.Exists(d => d == list_all[i]))
                    {
                        if (lanedicList.Count >= (_globalSettings.GameModeDance == 0 ? 2 : 1))
                        {
                            flag1 = true;
                            flag2 = true;
                            foreach (LANEDIC lanedic in lanedicList)
                            {
                                if (lanedic.ln > MODE / 2 - 1)
                                    flag1 = false;
                            }

                            foreach (LANEDIC lanedic in lanedicList)
                            {
                                if (lanedic.ln < MODE / 2)
                                    flag2 = false;
                            }
                        }

                        int num7;
                        for (num7 = 0;
                             (lanedicList.Exists(d => d.ln == lane) ||
                              flag1 && lane < MODE / 2 || flag2 && lane > MODE / 2 - 1) && num7 < 30;
                             ++num7)
                        {
                            lane = Random.Range(0, MODE);
                        }

                        if (num7 >= 30)
                        {
                            foreach (LANEDIC lanedic in lanedicList)
                            {
                                print("d:" + lanedic.ln + " " + lanedic.ms);
                            }
                        }

                        lanedicList.Insert(0, new LANEDIC(lane, num4));
                        while (lanedicList.Count > (_globalSettings.GameModeDance == 0 ? 3 : 2))
                            lanedicList.RemoveAt(_globalSettings.GameModeDance == 0 ? 3 : 2);
                        MakeNotesNormal(lane, num4);
                        ++num5;
                    }

                    num2 = lane;
                }

                int num8;
                if (floatList2.Exists(d => d == list_all[i]))
                {
                    int l = Random.Range(0, MODE);
                    int num9;
                    for (num9 = 0;
                         lanedicList.Exists(d => d.ln == l) && num9 < 30;
                         ++num9)
                        l = Random.Range(0, MODE);
                    if (num9 >= 30)
                    {
                        foreach (LANEDIC lanedic in lanedicList)
                            print("dd:" + lanedic.ln + " " + lanedic.ms);
                    }

                    lanedicList.Insert(0, new LANEDIC(l, num4));
                    while (lanedicList.Count > 3)
                        lanedicList.RemoveAt(3);
                    if (num5 < 3)
                        MakeNotesNormal(l, num4);
                    num8 = num5 + 1;
                    num2 = l;
                }
                else if (floatList3.Exists(d => d == list_all[i]))
                {
                    int l = Random.Range(0, MODE);
                    int num10;
                    for (num10 = 0;
                         lanedicList.Exists(d => d.ln == l) && num10 < 30;
                         ++num10)
                        l = Random.Range(0, MODE);
                    if (num10 >= 30)
                    {
                        foreach (LANEDIC lanedic in lanedicList)
                            print("dd:" + lanedic.ln + " " + lanedic.ms);
                    }

                    lanedicList.Insert(0, new LANEDIC(l, num4));
                    while (lanedicList.Count > 3)
                        lanedicList.RemoveAt(3);
                    if (num5 < 3)
                        MakeNotesPress(l, num4);
                    num8 = num5 + 1;
                    num2 = l;
                }
                else if (floatList5.Exists(d => d == list_all[i]))
                {
                    int l = Random.Range(0, MODE);
                    int num11;
                    for (num11 = 0;
                         lanedicList.Exists(d => d.ln == l) && num11 < 30;
                         ++num11)
                        l = Random.Range(0, MODE);
                    if (num11 >= 30)
                    {
                        foreach (LANEDIC lanedic in lanedicList)
                            print("dd:" + lanedic.ln + " " + lanedic.ms);
                    }

                    lanedicList.Insert(0, new LANEDIC(l, num4));
                    while (lanedicList.Count > 3)
                        lanedicList.RemoveAt(3);
                    if (num5 < 3)
                        MakeNotesBoom(l, num4);
                    num8 = num5 + 1;
                }
            }

            yield return null;
        }

        private void MakeNotesNormal(int lane, float realtime)
        {
            noteList[lane].Add(new NOTE(lane, realtime, 1, null, null));
            ++NOTECOUNT;
            int num = (int)((BGMManager.clip.samples * BGMManager.clip.channels) *
                            (realtime / BGMManager.clip.length));
            if (num + f_hit.Length >= BGMManager.clip.samples * BGMManager.clip.channels ||
                list_hit.Contains(num))
                return;
            list_hit.Add(num);
            for (int index = 0; index < f_hit.Length; ++index)
            {
                if (num + index < f_song.Length)
                    f_song[num + index] += f_hit[index] * 0.5f;
            }
        }

        private void MakeNotesPress(int lane, float realtime)
        {
            noteList[lane].Add(new NOTE(lane, realtime, 3, null, null));
            ++NOTECOUNT;
            int num = (int)(BGMManager.clip.samples * BGMManager.clip.channels *
                            (realtime / BGMManager.clip.length));
            if (num + f_flick.Length >= BGMManager.clip.samples * BGMManager.clip.channels ||
                list_flick.Contains(num))
                return;
            list_flick.Add(num);
            for (int index = 0; index < f_flick.Length; ++index)
            {
                if (num + index < f_song.Length)
                    f_song[num + index] += f_flick[index] * 0.5f;
            }
        }


        private void MakeNotesTails(int lane, float realtime)
        {
            if (noteList[lane].Count <= 0)
                MakeNotesNormal(lane, realtime);
            else if (noteList[lane][noteList[lane].Count - 1].kind == 4)
                noteList[lane].Add(new NOTE(lane, realtime, 3, null, null));
            else
                noteList[lane].Add(new NOTE(lane, realtime, 2, null, null));
            ++NOTECOUNT;
            int num = (int)(BGMManager.clip.samples * BGMManager.clip.channels *
                            (realtime / BGMManager.clip.length));
            if (num + f_flick.Length >= BGMManager.clip.samples * BGMManager.clip.channels ||
                list_flick.Contains(num))
                return;
            list_flick.Add(num);
            for (int index = 0; index < f_flick.Length; ++index)
            {
                if (num + index < f_song.Length)
                    f_song[num + index] += f_flick[index] * 0.5f;
            }
        }

        private void MakeNotesBoom(int lane, float realtime)
        {
            noteList[lane].Add(new NOTE(lane, realtime, 4, null, null));
            ++NOTECOUNT;
            int num = (int)(BGMManager.clip.samples * BGMManager.clip.channels *
                            (realtime / BGMManager.clip.length));
            if (num + f_hit.Length >= BGMManager.clip.samples * BGMManager.clip.channels ||
                list_hit.Contains(num))
                return;
            list_hit.Add(num);
            for (int index = 0; index < f_hit.Length; ++index)
            {
                if (num + index < f_song.Length)
                    f_song[num + index] += f_hit[index] * 0.5f;
            }
        }


        private void SonglistReadin(string file = "")
        {
            string str1;
            if (!string.IsNullOrEmpty(file))
            {
                str1 = new StreamReader(file).ReadToEnd();
                print(str1);
            }
            else
            {
                str1 = Resources.Load<TextAsset>("CHARTS/" + _songDataContainer.songData.keyword + "." +
                                                 _songDataContainer.selectedDiff).text;
            }

            string[] strArray1 = ScriptString.RemoveEnter(ScriptString.RemoveTab(ScriptString.RemoveSlash(str1)))
                .Split('\n');
            for (int index = 0; index < strArray1.Length; ++index)
            {
                if (!(strArray1[index] == ""))
                {
                    if (strArray1[index].Substring(0, 1) == "#")
                    {
                        if (strArray1[index].Substring(0, Mathf.Min(strArray1[index].Length, "#OFFSET".Length)) ==
                            "#OFFSET")
                            drbfile.offset =
                                float.Parse(strArray1[index].Replace("#OFFSET=", "").Replace(";", ""));
                        if (strArray1[index].Substring(0, Mathf.Min(strArray1[index].Length, "#BEAT".Length)) ==
                            "#BEAT")
                            drbfile.beat = float.Parse(strArray1[index].Replace("#BEAT=", "").Replace(";", ""));
                        string str2 = strArray1[index];
                        int length1 = strArray1[index].Length;
                        int count = drbfile.bpms.Count;
                        int length2 = ("#BPM [" + count + "]").Length;
                        int length3 = Mathf.Min(length1, length2);
                        string str3 = str2.Substring(0, length3);
                        count = drbfile.bpms.Count;
                        string str4 = "#BPM [" + count + "]";
                        if (str3 == str4)
                        {
                            string str5 = strArray1[index];
                            string str6 = strArray1[index + 1];
                            string str7 = str5;
                            count = drbfile.bpms.Count;
                            string oldValue1 = "#BPM [" + count + "]=";
                            string s1 = str7.Replace(oldValue1, "").Replace(";", "");
                            string str8 = str6;
                            count = drbfile.bpms.Count;
                            string oldValue2 = "#BPMS[" + count + "]=";
                            string s2 = str8.Replace(oldValue2, "").Replace(";", "");
                            drbfile.bpms.Add(new BPMS
                            {
                                bpm = float.Parse(s1),
                                bpms = float.Parse(s2)
                            });
                            bpmList.Add(new BPM(float.Parse(s2), float.Parse(s1)));
                        }
                    }
                    else
                    {
                        string str9 = strArray1[index].Replace("<", "");
                        string[] strArray2 = str9.Substring(0, str9.Length).Split('>');
                        NoteData noteData = new NoteData();
                        noteData.id = int.Parse(strArray2[0]);
                        noteData.kind = (NoteKind)int.Parse(strArray2[1]);
                        if (noteData.kind == NoteKind.FAKE_CENTER)
                            noteData.kind = NoteKind.SLIDE_CENTER;
                        if (noteData.kind == NoteKind.FAKE)
                            noteData.kind = NoteKind.SLIDE_END;
                        noteData.time = float.Parse(strArray2[2]);
                        noteData.pos = Mathf.Max(-16f, float.Parse(strArray2[3]));
                        noteData.width = Mathf.Min(48f, float.Parse(strArray2[4]));
                        if (noteData.pos + (double)noteData.width > 32.0f)
                            noteData.pos = 32f - noteData.width;
                        noteData.nsc = NoteData.NoteSC.Parse(strArray2[5]);
                        if (_globalSettings.SkillCheckMode)
                            noteData.nsc = NoteData.NoteSC.Parse("0");
                        noteData.parent = int.Parse(strArray2[6]);
                        noteData.mode = ParseNoteAppearMode.ParseToMode(
                            strArray2.Length <= 7 || _globalSettings.SkillCheckMode
                                ? "n"
                                : strArray2[7]);
                        drbfile.note.Add(noteData);
                        drbfile.noteHitflag.Add(false);
                    }
                }
            }
        }
    }

    public class BPM
    {
        public float measure;
        public float bpm;
        public float realtime;

        public BPM(float m, float b)
        {
            measure = m;
            bpm = b;
            realtime = 0.0f;
        }
    }

    public class NOTE
    {
        public int lane;
        public float realtime;
        public int kind;
        public int result;
        public GameObject objNote;
        public GameObject objLong;

        public NOTE(int l, float r, int k, GameObject no, GameObject lo)
        {
            lane = l;
            realtime = r;
            kind = k;
            result = 0;
            objNote = no;
            objLong = lo;
        }

        public void DestroySelf()
        {
            if (objNote) Object.Destroy(objNote);
            if (objLong) Object.Destroy(objLong);
        }
    }

    public class BPMS
    {
        public float bpm;
        public float bpms;
    }

    public class DRBFile
    {
        public string ndname;
        public float offset;
        public float beat;
        public List<BPMS> bpms;
        public List<NoteData> note;
        public List<bool> noteHitflag;
    }

    class LANEDIC
    {
        public int ln;
        public float ms;

        public LANEDIC(int l, float m)
        {
            ln = l;
            ms = m;
        }
    }
}
#endif