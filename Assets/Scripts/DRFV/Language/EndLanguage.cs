using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Language
{
    public class EndLanguage : MonoBehaviour
    {
        public Text retry;

        public Text exit;
        // Start is called before the first frame update
        void Start()
        {
            retry.text = LanguageManager.Instance.GetText("end.retry");
            exit.text = LanguageManager.Instance.GetText("end.exit");
        }
    }
}
