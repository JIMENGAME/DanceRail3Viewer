using System;
using System.Collections.Generic;
using DRFV.Global;
using DRFV.Result;
using DRFV.Select;
using DRFV.Setting;
using UnityEngine;

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

    public void Draw(int i)
    {
        GameObject qwq = Instantiate(linePrefab, linesParent);
        LineRenderer component = qwq
            .GetComponent<LineRenderer>();
#if UNITY_EDITOR
        qwq.GetComponent<LineSizeDebugger>().Init(this);
#endif
        component.useWorldSpace = false;
        component.startColor =
            Mathf.Abs(data[i]) <= noteJudgeRange.PJ
                ? Color.green
                : (Mathf.Abs(data[i]) <= noteJudgeRange.P
                    ? Color.yellow
                    : (Mathf.Abs(data[i]) <=
                       noteJudgeRange.G
                        ? new Color(1f, 0.5f, 0.0f)
                        : Color.red));
        component.endColor =
            Mathf.Abs(data[i + 1]) <= noteJudgeRange.PJ
                ? Color.green
                : (Mathf.Abs(data[i + 1]) <=
                   noteJudgeRange.P
                    ? Color.yellow
                    : (Mathf.Abs(data[i + 1]) <=
                       noteJudgeRange.G
                        ? new Color(1f, 0.5f, 0.0f)
                        : Color.red));
        component.positionCount = 2;
        component.SetPosition(0,
            new Vector3((float)(1.0 + 599.0 / (data.Length - 1) * i),
                (float)(-(double)data[i] * 2.0)));
        component.SetPosition(1,
            new Vector3((float)(1.0 + 599.0 / (data.Length - 1) * (i + 1)),
                (float)(-(double)data[i + 1] * 2.0)));
        component.sortingOrder = 1;
    }

#if UNITY_EDITOR
    private int noteJudgeRangeId;
#endif

    // Start is called before the first frame update
    void Awake()
    {
        int noteJudgeRangeId = GlobalSettings.CurrentSettings.NoteJudgeRange;
#if UNITY_EDITOR
        this.noteJudgeRangeId = noteJudgeRangeId;
#endif
        noteJudgeRange = Util.GetNoteJudgeRange(noteJudgeRangeId);
        linesParent = transform.GetChild(0);
        gameObject.GetComponent<SpriteRenderer>().sprite = BGmsSprites[noteJudgeRangeId];
        float a = Mathf.Min(Screen.width * 0.6f / BGmsSprites[noteJudgeRangeId].texture.width,
            Screen.height * 0.6f / BGmsSprites[noteJudgeRangeId].texture.height);
        transform.localScale = new Vector3(a, a, 1f);
        transform.localPosition = new Vector3(
            -a * BGmsSprites[noteJudgeRangeId].texture.width / 2f / BGmsSprites[noteJudgeRangeId].pixelsPerUnit,
            transform.localPosition.y,
            Screen.height / GetPosZ()); // z = sprite.height / 60% * (Camera.main.fov * 2)
        List<float> msDetails = GameObject.FindWithTag("ResultData").GetComponent<ResultDataContainer>().msDetails;
        while (msDetails.Count < 2)
        {
            msDetails.Add(0);
        }

        data = msDetails.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        float a = Mathf.Min(Screen.width * 0.6f / BGmsSprites[noteJudgeRangeId].texture.width,
            Screen.height * 0.6f / BGmsSprites[noteJudgeRangeId].texture.height);
        transform.localScale = new Vector3(a, a, 1f);
        transform.localPosition = new Vector3(
            -a * BGmsSprites[noteJudgeRangeId].texture.width / 2f / BGmsSprites[noteJudgeRangeId].pixelsPerUnit,
            transform.localPosition.y,
            GetPosZ()); // z = Screen.height / (Camera.main.fov * 2)
#endif
        int index = Mathf.Min(data.Length - 1,
            (int)(progress * (double)(data.Length - 1)));
        while (Counter < index)
        {
            Draw(Counter);
            Counter++;
        }

        progress += 0.01f;
    }

    private float GetPosZ()
    {
        return Mathf.Max(Screen.width / 2f /
                         Camera.VerticalToHorizontalFieldOfView(Camera.main.fieldOfView, Camera.main.aspect),
            Screen.height / 2f / Camera.main.fieldOfView);
    }
}