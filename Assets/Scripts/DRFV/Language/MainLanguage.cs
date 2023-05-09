using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Language
{
    public class MainLanguage : MonoBehaviour
    {
        public Text mainSelect;
        public Text mainMultiplayer;
        public Text mainShop;
        public Text mainSettings;

        void Start()
        {
            mainSelect.text = LanguageManager.Instance.GetText("main.select");
            mainMultiplayer.text = LanguageManager.Instance.GetText("main.multiplayer");
            mainShop.text = LanguageManager.Instance.GetText("main.shop");
            mainSettings.text = LanguageManager.Instance.GetText("main.settings");
        }
    }
}
