using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Language
{
    public class SettingLanguage : MonoBehaviour
    {
        public Text exit;
        public Text save;
        public Text reset;

        public Text noteStyle;
        public Text hiFps;
        public Text paramEQ;
        public Text gater;
        public Text tap;
        public Text combo;
        public Text judge;
        public Text fcap;

        public Text scoreIO;
        public Text scoreImport;
        public Text scoreExport;
        public Text scoreIOStatus;

        public Text autoplayHint;
        public Text autoplayHintBanner;
        public Text autoplayHintProgramInfo;

        public Text about;
        public Text aboutDisable;
        // Start is called before the first frame update
        public void Start()
        {
            exit.text = LanguageManager.Instance.GetText("settings.exit");
            save.text = LanguageManager.Instance.GetText("settings.save");
            reset.text = LanguageManager.Instance.GetText("settings.reset");
            noteStyle.text = LanguageManager.Instance.GetText("settings.notestyle");
            hiFps.text = LanguageManager.Instance.GetText("settings.hifps");
            paramEQ.text = LanguageManager.Instance.GetText("settings.parameq");
            gater.text = LanguageManager.Instance.GetText("settings.gater");
            tap.text = LanguageManager.Instance.GetText("settings.tap");
            combo.text = LanguageManager.Instance.GetText("settings.combo");
            judge.text = LanguageManager.Instance.GetText("settings.judge");
            fcap.text = LanguageManager.Instance.GetText("settings.fcap");
            scoreIO.text = LanguageManager.Instance.GetText("settings.score");
            scoreImport.text = LanguageManager.Instance.GetText("settings.score.import");
            scoreExport.text = LanguageManager.Instance.GetText("settings.score.export");
            autoplayHint.text = LanguageManager.Instance.GetText("settings.autoplay");
            autoplayHintBanner.text = LanguageManager.Instance.GetText("settings.autoplay.banner");
            autoplayHintProgramInfo.text = LanguageManager.Instance.GetText("settings.autoplay.programinfo");
            about.text = LanguageManager.Instance.GetText("settings.about");
            aboutDisable.text = LanguageManager.Instance.GetText("settings.about.disable");
            SetScoreIOStatus("settings.score.status.default");
        }

        public void SetScoreIOStatus(string id)
        {
            scoreIOStatus.text = LanguageManager.Instance.GetText(id);
        }
    }
}
