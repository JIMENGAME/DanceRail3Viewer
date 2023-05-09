using DRFV.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Game.Side
{
    [RequireComponent(typeof(Image))]
    public class SideChangerImageColor : SideChanger
    {
        public Color cLight = Color.black;
        public Color cDark = Color.white;
        
        public override void SetSide(GameSide gameSide)
        {
            gameObject.GetComponent<Image>().color = gameSide switch {
                GameSide.LIGHT => cLight,
                GameSide.DARK => cDark,
                GameSide.COLORLESS => cLight,
                _ => cDark
            };
        }
    }
}