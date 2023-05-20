using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SizeScaler : MonoBehaviour
{
    public float k;

    private RectTransform rectTransform;

    public float scaleX, scaleY;


    // Start is called before the first frame update
    void Awake()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(scaleX == 0 ? 0 : Screen.width / scaleY * k, scaleY == 0 ? 0 : Screen.height / scaleY * k);
    }

#if UNITY_EDITOR
    private void Update()
    {
        rectTransform.sizeDelta = new Vector2(scaleX == 0 ? 0 : Screen.width / scaleY * k, scaleY == 0 ? 0 : Screen.height / scaleY * k);
    }
#endif
}