using System.IO;
using DRFV.Global;
using DRFV.Global.Managers;
using UnityEngine;

namespace DRFV.Login
{
    public class AccountPanel : MonoBehaviour
    {
        public void LoadStory()
        {
            if (!Directory.Exists(StaticResources.Instance.dataPath + "resources/STORY")) return;
            FadeManager.Instance.LoadScene("story");
        }
    }
}
