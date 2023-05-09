using DRFV.Enums;
using DRFV.Game;
using UnityEngine;

namespace DRFV.Game.HPBars
{
    public class HPBarDefault : HPBar
    {
        public HPBarDefault()
        {
            HpInit = 100f;
            HpMax = 100f;
            barColorLight = new Color(0f, 0.3f, 0f, 1f);
            barColorDark = Color.green;
        }

        public override float PerfectJHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 150f * NoteWeight[(int) noteKind] / totalNotesWeigh;
        }
    
        public override float PerfectHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 75f * NoteWeight[(int) noteKind] / totalNotesWeigh;
        }

        public override float GoodHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 0.5f * NoteWeight[(int) noteKind] * theGameManager.SkillDamage;
        }
    
        public override float MissHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return NoteWeight[(int) noteKind] * theGameManager.SkillDamage;
        }
    }
}
