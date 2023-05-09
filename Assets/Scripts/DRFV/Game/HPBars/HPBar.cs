using DRFV.Enums;
using DRFV.Game;
using UnityEngine;

namespace DRFV.Game.HPBars
{
    public class HPBar
    {
        public float HpInit { get; protected set; } 
        public float HpMax { get; protected set;}
        public bool isCheap = true;
        public Color barColorLight { get; protected set; }
        public Color barColorDark { get; protected set; }

        public virtual float PerfectJHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 0f;
        }
    
        public virtual float PerfectHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 0f;
        }
    
        public virtual float GoodHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 0f;
        }
    
        public virtual float MissHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 0f;
        }

        public virtual void OnRefresh()
        {
        
        }

        public virtual void OnInit()
        {
        
        }

        public static int[] NoteWeight = new int[28]
        {
            1, // 无
            3, // Tap
            3, // ExTap
            1, // Hold (Start)
            1, // Hold (End)
            1, // Slide (Start)
            1, // Slide (Center)
            1, // Slide (End)
            2, // Fake
            2, // Flick
            3, // BOOM
            1, // Hold (Center)
            1, // Fake (Center)
            2, // Flick (Left)
            2, // Flick (Right)
            2, // Flick (Up)
            2, // Flick (Down)
            3, // BOOM (Center)
            3, // BOOM (End)
            2, // HPass (Center)
            2, // HPass (End)
            2, // LPass (Center)
            2, // LPass (End)
            2, // Mover (Center)
            2, // Mover (End)
            2, // 未知
            2, // 未知
            2 // 未知
        };

    }
}
