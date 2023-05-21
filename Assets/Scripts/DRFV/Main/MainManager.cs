using DRFV.Global;
using DRFV.Global.Components;
using DRFV.Global.Managers;
using DRFV.Login;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Main
{
    public class MainManager : MonoBehaviour
    {
        public Text version;

        public GameObject[] disableInOffline;

        public GameObject InputWindowPrefab;

        public void Init()
        {
            version.text = "v" + Application.version;
            AccountInfo.Instance.UpdateAccountPanel();
            if (AccountInfo.Instance.acountStatus == AccountInfo.AcountStatus.LOGINED) return;
            foreach (GameObject o in disableInOffline)
            {
                o.SetActive(false);
            }
        }

        public void LoadScene(string name)
        {
            FadeManager.Instance.LoadScene(name);
        }

        public void LoadHadouTest()
        {
            if (RuntimeSettingsManager.Instance.hadouTest)
            {
                FadeManager.Instance.LoadScene("hadoutest");
                return;
            }

            InputWindow inputWindow = Instantiate(InputWindowPrefab, GameObject.FindWithTag("MainCanvas").transform)
                .GetComponent<InputWindow>();
            inputWindow.Show(null, null,
                new byte[]
                {
                    73, 87, 97, 110, 116, 84, 111, 77, 97, 107, 101, 76, 111, 118, 101, 87, 105, 116, 104, 72, 97, 100,
                    111, 117, 50, 51, 51, 51
                }, value =>
                {
                    RuntimeSettingsManager.Instance.hadouTest = value;
                    if (value) FadeManager.Instance.LoadScene("hadoutest");
                });
        }

        public void ExitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}