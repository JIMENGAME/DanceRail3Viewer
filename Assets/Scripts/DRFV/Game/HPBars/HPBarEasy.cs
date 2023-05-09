using DRFV.Enums;
using DRFV.Game;
using UnityEngine;

namespace DRFV.Game.HPBars
{
    public class HPBarEasy : HPBar
    {
        public HPBarEasy()
        {
            HpInit = 0f;
            HpMax = 100f;
            isCheap = false;
            barColorLight = new Color(0f, 0.3f, 0f, 1f);
            barColorDark = Color.green;
        }
        public override float PerfectJHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 300f * NoteWeight[(int) noteKind] / totalNotesWeigh;
        }
    
        public override float PerfectHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 150f * NoteWeight[(int) noteKind] / totalNotesWeigh;
        }

        public override float GoodHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 0f;
        }
    
        public override float MissHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 0.5f * NoteWeight[(int) noteKind] * theGameManager.SkillDamage;
        }
    }
}
