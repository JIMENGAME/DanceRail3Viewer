using DRFV.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Game.Side
{
    [RequireComponent(typeof(Text))]
    public class SideChangerTextColor : SideChanger
    {
        public Color cLight = Color.black;
        public Color cDark = Color.white;
        public Color cOutlineLight = new Color(1f, 1f, 1f, 128f / 255f);
        public Color cOutlineDark = new Color(0f, 0f, 0f, 128f / 255f);
        
        public override void SetSide(GameSide gameSide)
        {
            gameObject.GetComponent<Text>().color = gameSide switch {
                GameSide.LIGHT => cLight,
                GameSide.DARK => cDark,
                GameSide.COLORLESS => cLight,
                _ => cDark
            };
            if (gameObject.TryGetComponent(out Outline outline))
            {
                outline.effectColor = gameSide switch {
                    GameSide.LIGHT => cOutlineLight,
                    GameSide.DARK => cOutlineDark,
                    GameSide.COLORLESS => cOutlineLight,
                    _ => cOutlineDark
                };
            }
        }
    }
}