using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Global
{
    [RequireComponent(typeof(CanvasScaler))]
    public class ScreenMatcher : MonoBehaviour
    {
        private CanvasScaler canvasScaler;
    
        private void Start()
        {
            canvasScaler = gameObject.GetComponent<CanvasScaler>();
            MatchScreen();
        }

#if UNITY_EDITOR
        void Update()
        {
            MatchScreen();
        }
#endif
    
        private void MatchScreen()
        {
            if (canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize && canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                canvasScaler.matchWidthOrHeight = Screen.height * canvasScaler.referenceResolution.x / Screen.width > canvasScaler.referenceResolution.y ? 0 : 1;
            }
        }
    }
}
