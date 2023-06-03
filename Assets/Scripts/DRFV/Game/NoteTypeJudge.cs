using System;
using DRFV.Enums;
using UnityEngine;

namespace DRFV.Game
{
    public static class NoteTypeJudge
    {
        public static bool IsTail(this NoteData noteData)
        {
            return noteData.IsEnd() || noteData.IsCenter();
        }

        public static bool IsEnd(this NoteData noteData)
        {
            NoteKind k = noteData.kind;
            if (k == NoteKind.HOLD_END) return true;
            if (k == NoteKind.SLIDE_END) return true;
            if (k == NoteKind.FAKE) return true;
            if (k == NoteKind.BOOM_END) return true;
            if (k == NoteKind.HPass_END) return true;
            if (k == NoteKind.LPass_END) return true;
            if (k == NoteKind.MOVER_END) return true;
            if (k == NoteKind.STEREO_END) return true;

            return false;
        }

        public static bool IsCenter(this NoteData noteData)
        {
            NoteKind k = noteData.kind;
            if (k == NoteKind.SLIDE_CENTER) return true;
            if (k == NoteKind.HOLD_CENTER) return true;
            if (k == NoteKind.FAKE_CENTER) return true;
            if (k == NoteKind.BOOM_CENTER) return true;
            if (k == NoteKind.HPass_CENTER) return true;
            if (k == NoteKind.LPass_CENTER) return true;
            if (k == NoteKind.MOVER_CENTER) return true;
            if (k == NoteKind.STEREO_CENTER) return true;

            return false;
        }
    
        public static bool IsTapSound(this NoteData noteData)
        {
            NoteKind k = noteData.kind;
            if (k == NoteKind.TAP) return true;
            if (k == NoteKind.ExTAP) return true;

            return false;
        }

        public static bool IsHold(this NoteData noteData)
        {
            NoteKind k = noteData.kind;
            if (k == NoteKind.HOLD_START) return true;
            if (k == NoteKind.HOLD_END) return true;
            return false;
        }

        public static bool IsSlideSound(this NoteData noteData)
        {
            NoteKind k = noteData.kind;
            if (k == NoteKind.SLIDE_START) return true;
            if (k == NoteKind.SLIDE_END) return true;
            if (k == NoteKind.BOOM) return true;
            if (k == NoteKind.BOOM_END) return true;
            if (k == NoteKind.HPass_END) return true;
            if (k == NoteKind.LPass_END) return true;
            if (k == NoteKind.MOVER_END) return true;
            if (k == NoteKind.STEREO_START) return true;
            if (k == NoteKind.STEREO_END) return true;
            return false;
        }

        public static bool IsTap(this NoteData noteData)
        {
            NoteKind k = noteData.kind;
            if (k == NoteKind.TAP) return true;
            if (k == NoteKind.ExTAP) return true;

            return false;
        }

        public static bool IsFlick(this NoteData noteData)
        {
            NoteKind k = noteData.kind;
            if (k == NoteKind.FLICK_LEFT) return true;
            if (k == NoteKind.FLICK_RIGHT) return true;
            if (k == NoteKind.FLICK_UP) return true;
            if (k == NoteKind.FLICK_DOWN) return true;

            return false;
        }

        public static bool IsAnyFlick(this NoteData noteData)
        {
            return noteData.kind == NoteKind.FLICK || noteData.IsFlick();
        }

        public static bool IsBitCrash(this NoteData noteData)
        {
            NoteKind k = noteData.kind;
            if (k == NoteKind.HOLD_END) return true;
            if (k == NoteKind.HOLD_CENTER) return true;

            return false;
        }
        
        public static bool IsMover(this NoteData noteData)
        {
            NoteKind k = noteData.kind;
            if (k == NoteKind.MOVER_CENTER) return true;
            if (k == NoteKind.MOVER_END) return true;
            if (k == NoteKind.MOVERBOOM_CENTER) return true;
            if (k == NoteKind.MOVERBOOM_END) return true;
            return false;
        }
    }
}