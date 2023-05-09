using DRFV.Enums;
using DRFV.Game;
using UnityEngine;

namespace DRFV.Game.HPBars
{
    public class HpBarHelloWinner : HPBar
    {
        public HpBarHelloWinner()
        {
            HpInit = 100f;
            HpMax = 100f;
            barColorLight = new Color(0f, 0.3f, 0f, 1f);
            barColorDark = Color.green;
        }
        public override float PerfectJHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 0;
        }
    
        public override float PerfectHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 0;
        }

        public override float GoodHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 0;
        }
    
        public override float MissHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 500f;
        }
    }
}
