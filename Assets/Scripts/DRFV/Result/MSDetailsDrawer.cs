using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DRFV.Global;
using DRFV.Select;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Result
{
    public class MSDetailsDrawer : MonoBehaviour
    {
        public GameObject linePrefab;
        public float[] data;
        public float width;
        private int Counter;
        public float progress;
        private NoteJudgeRange noteJudgeRange;
        private Transform linesParent;
        public Sprite[] BGmsSprites;
        private float yScale = 1f;
        public float k = 0.6f;
        private float panelScale;
        public float yOffset = 0.05f;
        public Text samplesInfo;

        private void Draw(int i)
        {
            GameObject qwq = Instantiate(linePrefab, linesParent);
            LineRenderer component = qwq
                .GetComponent<LineRenderer>();
            component.startWidth = component.endWidth = width;
            component.startColor = GetColorFromMs(data[i]);
            component.endColor = GetColorFromMs(data[i + 1]);
            component.positionCount = 2;
            component.SetPosition(0,
                new Vector3(1.0f + 599.0f / (data.Length - 1) * i,
                    -data[i] * 2.0f * yScale));
            component.SetPosition(1,
                new Vector3(1.0f + 599.0f / (data.Length - 1) * (i + 1),
                    -data[i + 1] * 2.0f * yScale));
            component.sortingOrder = 1;
        }


        private Color GetColorFromMs(float value)
        {
            value = Mathf.Abs(value);
            if (value <= noteJudgeRange.PJ) return Color.green;
            if (value <= noteJudgeRange.P) return Color.yellow;
            if (value <= noteJudgeRange.G) return new Color(1f, 0.5f, 0.0f);
            return Color.red;
        }

#if UNITY_EDITOR
        private int noteJudgeRangeId;
#endif

        // Start is called before the first frame update
        public void Init()
        {
            int noteJudgeRangeId = GlobalSettings.CurrentSettings.NoteJudgeRange;
#if UNITY_EDITOR
            this.noteJudgeRangeId = noteJudgeRangeId;
#endif
            noteJudgeRange = Util.GetNoteJudgeRange(noteJudgeRangeId);
            linesParent = transform.GetChild(0);
            Sprite bGmsSprite = BGmsSprites[noteJudgeRangeId];
            gameObject.GetComponent<SpriteRenderer>().sprite = bGmsSprite;
            if (noteJudgeRange.G > 100)
            {
                yScale = 100f / noteJudgeRange.G;
            }
            else
            {
                yScale = 1;
            }

            panelScale = Mathf.Min(Screen.width * k * bGmsSprite.pixelsPerUnit / bGmsSprite.texture.width / 100f,
                Screen.height * k * bGmsSprite.pixelsPerUnit / bGmsSprite.texture.height / 100f);
            transform.localScale = new Vector3(panelScale, 0, 1f);
            transform.localPosition = new Vector3(
                -panelScale * bGmsSprite.texture.width / 2f / bGmsSprite.pixelsPerUnit,
                yOffset * Screen.height / bGmsSprite.pixelsPerUnit,
                GetPosZ()); // z = Screen.height / (Camera.main.fov * 2)
            List<float> msDetails = GameObject.FindWithTag("ResultData").GetComponent<ResultDataContainer>().msDetails;
            while (msDetails.Count < 2)
            {
                msDetails.Add(0);
            }

            data = msDetails.ToArray();
            samplesInfo.text = $"平均数：{data.Average():N3}  标准差：{data.StandardDeviation():N3}";
        }

        public IEnumerator ChangeState(bool value, float time, Ease ease)
        {
            if (!value) enbaleAnimation = false;
            transform.DOScaleY(value ? panelScale : 0, time).SetEase(ease);
            yield return new WaitForSecondsRealtime(time);
            if (value) enbaleAnimation = true;
        }

        private bool enbaleAnimation = false;
        private bool generationEnded = false;

        // Update is called once per frame
        void Update()
        {
            if (!enbaleAnimation) return;
#if UNITY_EDITOR
            Sprite bGmsSprite = BGmsSprites[noteJudgeRangeId];
            panelScale = Mathf.Min(Screen.width * k * bGmsSprite.pixelsPerUnit / bGmsSprite.texture.width / 100f,
                Screen.height * k * bGmsSprite.pixelsPerUnit / bGmsSprite.texture.height / 100f);
            transform.localScale = new Vector3(panelScale, panelScale, 1f);
            transform.localPosition = new Vector3(
                -panelScale * bGmsSprite.texture.width / 2f / bGmsSprite.pixelsPerUnit,
                yOffset * Screen.height / bGmsSprite.pixelsPerUnit,
                GetPosZ()); // z = Screen.height / (Camera.main.fov * 2)
#endif
            if (generationEnded) return;
            int index = Mathf.Min(data.Length - 1,
                (int)(progress * (data.Length - 1)));
            while (Counter < index)
            {
                Draw(Counter);
                Counter++;
            }

            generationEnded = Counter >= data.Length - 1;

            progress += 0.01f;
        }

        private float GetPosZ()
        {
            return Mathf.Max(Screen.width / 2f /
                             Camera.VerticalToHorizontalFieldOfView(Camera.main.fieldOfView, Camera.main.aspect),
                Screen.height / 2f / Camera.main.fieldOfView);
        }
    }
}