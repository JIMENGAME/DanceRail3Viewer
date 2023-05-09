using UnityEngine;

namespace DRFV.Global
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
