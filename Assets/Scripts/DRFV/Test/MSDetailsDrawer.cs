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
    private float yScale = 1f;
    public float k = 0.6f;

    public void Draw(int i)
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
    void Awake()
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

        float a = Mathf.Min(Screen.width * k * bGmsSprite.pixelsPerUnit / bGmsSprite.texture.width / 100f,
            Screen.height * k * bGmsSprite.pixelsPerUnit / bGmsSprite.texture.height / 100f);
        transform.localScale = new Vector3(a, a, 1f);
        transform.localPosition = new Vector3(
            -a * bGmsSprite.texture.width / 2f / bGmsSprite.pixelsPerUnit,
            transform.localPosition.y,
            GetPosZ()); // z = Screen.height / (Camera.main.fov * 2)
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
        Sprite bGmsSprite = BGmsSprites[noteJudgeRangeId];
        float a = Mathf.Min(Screen.width * k * bGmsSprite.pixelsPerUnit / bGmsSprite.texture.width / 100f,
            Screen.height * k * bGmsSprite.pixelsPerUnit / bGmsSprite.texture.height / 100f);
        transform.localScale = new Vector3(a, a, 1f);
        transform.localPosition = new Vector3(
            -a * bGmsSprite.texture.width / 2f / bGmsSprite.pixelsPerUnit,
            transform.localPosition.y,
            GetPosZ()); // z = Screen.height / (Camera.main.fov * 2)
#endif
        int index = Mathf.Min(data.Length - 1,
            (int)(progress * (data.Length - 1)));
        while (Counter < index)
        {
            Draw(Counter);
            Counter++;
        }

        if (Counter >= data.Length - 1) return;

        progress += 0.01f;
    }

    private float GetPosZ()
    {
        return Mathf.Max(Screen.width / 2f /
                         Camera.VerticalToHorizontalFieldOfView(Camera.main.fieldOfView, Camera.main.aspect),
            Screen.height / 2f / Camera.main.fieldOfView);
    }
}