using DRFV.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Game.Side
{
    [RequireComponent(typeof(Image))]
    public class SideChangerImage : SideChanger
    {
        public Sprite sLight;
        public Sprite sDark;

        public override void SetSide(GameSide gameSide)
        {
            gameObject.GetComponent<Image>().sprite = gameSide switch
            {
                GameSide.LIGHT => sLight,
                GameSide.DARK => sDark,
                GameSide.COLORLESS => sLight,
                _ => sDark
            };
        }
    }
}