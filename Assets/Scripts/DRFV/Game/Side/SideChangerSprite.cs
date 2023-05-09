using DRFV.Enums;
using UnityEngine;

namespace DRFV.Game.Side
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SideChangerSprite : SideChanger
    {
        public Color cLight = Color.black;
        public Color cDark = Color.white;

        public override void SetSide(GameSide gameSide)
        {
            gameObject.GetComponent<SpriteRenderer>().color = gameSide switch {
                GameSide.LIGHT => cLight,
                GameSide.DARK => cDark,
                GameSide.COLORLESS => cLight,
                _ => cDark
            };
        }
    }
}