using System;
using DRFV.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Game.Side
{
    public class SideChangerColorBlock : SideChanger
    {
        public ColorBlockType ColorBlockType = ColorBlockType.SLIDER;
        public ColorBlock cLight = new()
        {
            normalColor = Color.black, highlightedColor = Color.black, pressedColor = Color.black,
            selectedColor = Color.black, disabledColor = Color.black, colorMultiplier = 1, fadeDuration = 0.1f
        };

        public ColorBlock cDark = new()
        {
            normalColor = Color.white, highlightedColor = Color.white, pressedColor = Color.white,
            selectedColor = Color.white, disabledColor = Color.white, colorMultiplier = 1, fadeDuration = 0.1f
        };

        public Color cSelectionLight = new(93f/255f, 114f/255f, 141f, 192f/255f);
        public Color cSelectionDark = new(168f/255f, 206f/255f, 1f, 192f/255f);

        public override void SetSide(GameSide gameSide)
        {
            ColorBlock colorBlock = gameSide switch
            {
                GameSide.LIGHT => cLight,
                GameSide.DARK => cDark,
                GameSide.COLORLESS => cLight,
                _ => cDark
            };
            switch (ColorBlockType)
            {
                case ColorBlockType.SLIDER:
                    gameObject.GetComponent<Slider>().colors = colorBlock;
                    break;
                case ColorBlockType.INPUT_FIELD:
                    InputField inputField = gameObject.GetComponent<InputField>();
                    inputField.colors = colorBlock;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum ColorBlockType
    {
        SLIDER = 0,
        INPUT_FIELD = 1,
        BUTTON = 3
    }
}