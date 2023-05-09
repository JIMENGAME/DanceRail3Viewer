using DRFV.Enums;
using UnityEngine;

namespace DRFV.Game.Side
{
    public class SideChangerSpectrum : SideChanger
    {
        public Sprite sLight;
        public Sprite sDark;

        public override void SetSide(GameSide gameSide)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var go = transform.GetChild(i).gameObject;
                if (go.TryGetComponent(out SpriteRenderer spectrum))
                {
                    spectrum.sprite = gameSide switch
                    {
                        GameSide.LIGHT => sLight,
                        GameSide.DARK => sDark,
                        GameSide.COLORLESS => sLight,
                        _ => sDark
                    };
                }
            }
        }
    }
}