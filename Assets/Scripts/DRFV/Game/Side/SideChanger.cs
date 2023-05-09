using DRFV.Enums;
using UnityEngine;

namespace DRFV.Game.Side
{
    public abstract class SideChanger : MonoBehaviour
    {
        public abstract void SetSide(GameSide gameSide);
    }
}
