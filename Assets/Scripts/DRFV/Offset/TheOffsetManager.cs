using System;
using System.Collections;
using DRFV.Enums;
using DRFV.Game;
using DRFV.Global.Managers;
using DRFV.inokana;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Offset
{
    public class TheOffsetManager : MonoBehaviour
    {
        public ProgressManager progressManager;
        public float NoteOffset;
        public AudioSource BGMManager;

        [Range(1, 30)] public int NoteSpeed = 12;

        [SerializeField] GameObject prefabEffect;

        [SerializeField] Sprite arror;

        private int tapSize, tapAlpha;

        public GameObject notesUp;

        public InputManager inputManager;

        public GameObject NotePrefab;

        public Sprite tapSprite;

        private bool timingStarted;

        public bool running;

        public InputField offsetInput;

        public float NowTime => progressManager.NowTime - NoteOffset;

        // Start is called before the first frame update
        void Start()
        {
            GlobalSettings currentSettings = GlobalSettings.CurrentSettings;
            tapSize = currentSettings.TapSize;
            tapAlpha = currentSettings.TapAlpha;
            offsetInput.text = currentSettings.Offset + "";
            progressManager.Init(() => { NotificationBarManager.Instance.Show("计时器精准度可能有问题，请与inokana取得联系"); },
                () => NotificationBarManager.Instance.Show("DspBuffer数值过小"), () => 1.0f);
            StartCoroutine(StartC());
        }

        private IEnumerator StartC()
        {
            if (FadeManager.Instance == null) yield return new WaitForSecondsRealtime(2f);
            else yield return null;
            StartTiming();
        }

        public void SetOffset()
        {
            try
            {
                NoteOffset = float.Parse(offsetInput.text);
            }
            catch (FormatException)
            {
                offsetInput.text = NoteOffset + "";
            }
        }

        public IEnumerator GenerateNote()
        {
            int i = 0;
            while (running)
            {
                int ms = 1000 + i * 2000;
                if (progressManager.NowTime < ms - 2000)
                    yield return new WaitWhile(() => progressManager.NowTime < ms - 2000);
                GameObject note = Instantiate(NotePrefab, notesUp.transform);
                note.GetComponent<SpriteRenderer>().sprite = tapSprite;
                note.GetComponent<SpriteRenderer>().sortingOrder = 2;
                note.GetComponent<TheNoteOffset>().SetGMIMG(this, inputManager);
                note.GetComponent<TheNoteOffset>().Ready(new()
                {
                    id = i,
                    time = 1 + i * 2,
                    pos = 6,
                    center = 8,
                    mode = NoteAppearMode.None,
                    nsc = NoteData.NoteSC.GetCommonNSC(),
                    ms = ms,
                    dms = ms,
                    width = 4,
                    kind = NoteKind.TAP,
                    isJudgeTimeRangeConflicted = false
                }, tapSize, tapAlpha);
                note.GetComponent<TheNoteOffset>().StartC();
                i++;
            }
        }

        public void StartTiming()
        {
            timingStarted = true;
            StartCoroutine(GenerateNote());
            progressManager.AddStartDelay(0.1f);
            progressManager.StartTiming();
            StartCoroutine(ListenMusic());
        }

        public IEnumerator ListenMusic()
        {
            while (running)
            {
                BGMManager.PlayScheduled(AudioSettings.dspTime);
                yield return new WaitForSeconds(8f * BGMManager.pitch);
                BGMManager.Stop();
            }
        }

        public Sprite GetSpriteArror()
        {
            return arror;
        }

        public void Judge(Vector3 pos, float width)
        {
            GameObject go = Instantiate(prefabEffect, pos, Quaternion.identity);
            if (go)
            {
                go.transform.localScale = new Vector3(width * 0.2f + 2.0f, width * 0.2f + 2.0f, width * 0.2f + 2.0f);
            }
        }

        public void Quit()
        {
            GlobalSettings a = GlobalSettings.CurrentSettings;
            a.Offset = NoteOffset;
            GlobalSettings.CurrentSettings = a;
            FadeManager.Instance.Back();
        }

        // Update is called once per frame
        void Update()
        {
            progressManager.OnUpdate();
        }
    }
}