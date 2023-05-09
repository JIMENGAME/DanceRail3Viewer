using DRFV.Global;
using UnityEngine;

namespace DRFV.Login
{
    public class AccountPanel : MonoBehaviour
    {
        public void LoadStory()
        {
            FadeManager.Instance.LoadScene("story");
        }
    }
}
