using UnityEngine;

namespace DRFV.Global.Components
{
    public class OpenURLButton : MonoBehaviour
    {
        public string url;

        public void OpenURL()
        {
            Application.OpenURL(url);
        }
    }
}
