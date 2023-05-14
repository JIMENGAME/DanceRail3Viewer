using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SizeScaler : MonoBehaviour
{
    public float k;

    private RectTransform rectTransform;


    // Start is called before the first frame update
    void Awake()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
        var localScale = rectTransform.localScale;
        rectTransform.sizeDelta = new Vector2(Screen.width / localScale.x * k, Screen.height / localScale.y * k);
    }

#if UNITY_EDITOR
    private void Update()
    {
        var localScale = rectTransform.localScale;
        rectTransform.sizeDelta = new Vector2(Screen.width / localScale.x * k, Screen.height / localScale.y * k);
    }
#endif
}