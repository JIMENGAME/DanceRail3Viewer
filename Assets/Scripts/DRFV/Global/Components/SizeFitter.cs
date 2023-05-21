using UnityEngine;

namespace DRFV.Global.Components
{
    [RequireComponent(typeof(RectTransform))]
    public class SizeFitter : MonoBehaviour
    {
        private float originalPosX, originalPosY;
        private float originalSizeX, originalSizeY;

        public float originalSizeXParent, originalSizeYParent;

        private RectTransform rectTransform;
        // Start is called before the first frame update
        void Start()
        {
            rectTransform = gameObject.GetComponent<RectTransform>();
            Vector2 sizeDelta = rectTransform.sizeDelta;
            Vector2 anchoredPosition = rectTransform.anchoredPosition;
            originalSizeX = sizeDelta.x;
            originalSizeY = sizeDelta.y;
            originalPosX = anchoredPosition.x;
            originalPosY = anchoredPosition.y;
            Vector2 vector2 = ((RectTransform) transform.parent).sizeDelta;
            rectTransform.sizeDelta = new Vector2(originalSizeX == 0 ? 0 : vector2.x * originalSizeX / originalSizeXParent, originalSizeY == 0 ? 0 : vector2.y * originalSizeY / originalSizeYParent);
            rectTransform.anchoredPosition = new Vector2(originalPosX == 0 ? 0 : vector2.x * originalPosX / originalSizeXParent, originalPosY == 0 ? 0 : vector2.y * originalPosY / originalSizeYParent);
        }

#if UNITY_EDITOR
        // Update is called once per frame
        void Update()
        {
            Vector2 vector2 = ((RectTransform) transform.parent).sizeDelta;
            rectTransform.sizeDelta = new Vector2(originalSizeX == 0 ? 0 : vector2.x * originalSizeX / originalSizeXParent, originalSizeY == 0 ? 0 : vector2.y * originalSizeY / originalSizeYParent);
            rectTransform.anchoredPosition = new Vector2(originalPosX == 0 ? 0 : vector2.x * originalPosX / originalSizeXParent, originalPosY == 0 ? 0 : vector2.y * originalPosY / originalSizeYParent);
        }
#endif
    }
}
