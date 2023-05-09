using DRFV.Setting;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Language
{
    public class LanguageButonGroup : ButtonGroup
    {
        public SettingLanguage settingLanguage;

        public string[] languageIdentities = {"chinese", "english"};
        // Start is called before the first frame update
        void Start()
        {
            string language = PlayerPrefs.GetString("Language", "chinese");
            bool flag = false;
            for (int i = 0; i < languageIdentities.Length; i++)
            {
                if (language.Equals(languageIdentities[i]))
                {
                    selected = i;
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                LanguageManager.Instance.SetLanguage(languageIdentities[0]);
                settingLanguage.Start();
            }
            for (int i = 0; i < Buttons.Length; i++)
            {
                var i1 = i;
                Buttons[i].onClick.AddListener(() => { UpdateButtonSprite(i1); });
                Buttons[i].transition = Selectable.Transition.SpriteSwap;
                SpriteState state = new SpriteState
                {
                    highlightedSprite = null,
                    pressedSprite = imageButtonPressed,
                    selectedSprite = null,
                    disabledSprite = null
                };
                Buttons[i].gameObject.GetComponent<Image>().sprite = i == selected ? imageButtonPressed : imageButton;
                Buttons[i].spriteState = state;
            }

            UpdateButtonSprite(selected);
        }

        protected override void OnClick(int id)
        {
            LanguageManager.Instance.SetLanguage(languageIdentities[id]);
            settingLanguage.Start();
        }
    }
}
