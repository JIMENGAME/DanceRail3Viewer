using UnityEngine;

namespace DRFV.Global
{
    public class BackgroundManager : MonoBehaviour
    {
        public RectTransform imageBackground;

        private Vector3 backgroundStartPos;
    
        void Start()
        {
            backgroundStartPos = imageBackground.position;
        }
    
        void Update()
        {
            imageBackground.position = backgroundStartPos +
                                       new Vector3(
                                           20.0f * Mathf.Sin(Time.realtimeSinceStartup / 50.0f * Mathf.PI), 0.0f,
                                           0.0f);
        }
    }
}
