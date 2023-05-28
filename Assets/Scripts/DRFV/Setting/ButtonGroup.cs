using System;
using System.Collections.Generic;
using DRFV.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Setting
{
    public class ButtonGroup : MonoBehaviour
    {
        protected Button[] Buttons;
    
        public Sprite imageButton, imageButtonPressed;

        protected int selected;

        public TheSettingsManager TheSettingsManager;

        public ButtonGroupType type;

        // Start is called before the first frame update
        public void Init(TheSettingsManager theSettingsManager)
        {
            TheSettingsManager = theSettingsManager;
            List<Button> buttonList = new List<Button>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var component = transform.GetChild(i).gameObject.GetComponent<Button>();
                if (!component) continue;
                buttonList.Add(component);
            }

            Buttons = buttonList.ToArray();
            selected = type switch
            {
                ButtonGroupType.COMBO => TheSettingsManager.currentSettings.ComboDisp,
                ButtonGroupType.SMALL_JUDGE => TheSettingsManager.currentSettings.SmallJudgeDisp,
                ButtonGroupType.AUTOPLAY_HINT => TheSettingsManager.currentSettings.AutoplayHint,
                ButtonGroupType.GAME_SIDE_GROUP => TheSettingsManager.currentSettings.GameSide,
                ButtonGroupType.SCORE_TYPE => (int) TheSettingsManager.currentSettings.ScoreType,
                _ => throw new ArgumentOutOfRangeException()
            };
            for (int i = 0; i < Buttons.Length; i++)
            {
                var i1 = i;
                Buttons[i].onClick.AddListener(() => { OnClick(i1); UpdateButtonSprite(i1); });
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

            for (int i = 0; i < Buttons.Length; i++)
            {
                Buttons[i].gameObject.GetComponent<Image>().sprite = i == selected ? imageButtonPressed: imageButton;
            }
        }

        protected void UpdateButtonSprite(int id)
        {
            for (int i = 0; i < Buttons.Length; i++)
            {
                Buttons[i].gameObject.GetComponent<Image>().sprite = i == id ? imageButtonPressed: imageButton;
            }

            selected = id;
        }

        protected virtual void OnClick(int id)
        {
            switch (type)
            {
                case ButtonGroupType.COMBO:
                    TheSettingsManager.currentSettings.ComboDisp = id;
                    break;
                case ButtonGroupType.SMALL_JUDGE:
                    TheSettingsManager.currentSettings.SmallJudgeDisp = id;
                    break;
                case ButtonGroupType.AUTOPLAY_HINT:
                    TheSettingsManager.currentSettings.AutoplayHint = id;
                    break;
                case ButtonGroupType.GAME_SIDE_GROUP:
                    TheSettingsManager.currentSettings.GameSide = id;
                    break;
                case ButtonGroupType.SCORE_TYPE:
                    TheSettingsManager.currentSettings.ScoreType = (SCORE_TYPE) id;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}