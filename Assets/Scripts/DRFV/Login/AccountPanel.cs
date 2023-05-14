using DRFV.Global;
using UnityEngine;

namespace DRFV.Login
{
    public class AccountPanel : MonoBehaviour
    {
        public void LoadStory()
        {
            if (RuntimeSettingsManager.Instance.removedStory) return;
            FadeManager.Instance.LoadScene("story");
        }
    }
}
