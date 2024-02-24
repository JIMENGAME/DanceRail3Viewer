using UnityEngine;

namespace DRFV.Global.Components
{
    public class OpenURLButton : MonoBehaviour
    {
        [SerializeField] private string url;

        public void OpenURL()
        {
            Application.OpenURL(url);
        }
    }
}
