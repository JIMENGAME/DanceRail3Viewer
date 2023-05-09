using DRFV.Enums;

namespace DRFV.Game
{
    public static class NoteTypeJudge
    {
        public static bool IsTail(NoteKind k)
        {
            if (k == NoteKind.HOLD_END) return true;
            if (k == NoteKind.SLIDE_CENTER) return true;
            if (k == NoteKind.SLIDE_END) return true;
            if (k == NoteKind.FAKE) return true;
            if (k == NoteKind.HOLD_CENTER) return true;
            if (k == NoteKind.FAKE_CENTER) return true;
            if (k == NoteKind.BOOM_CENTER) return true;
            if (k == NoteKind.BOOM_END) return true;
            if (k == NoteKind.HPass_CENTER) return true;
            if (k == NoteKind.HPass_END) return true;
            if (k == NoteKind.LPass_CENTER) return true;
            if (k == NoteKind.LPass_END) return true;
            if (k == NoteKind.MOVER_CENTER) return true;
            if (k == NoteKind.MOVER_END) return true;
            if (k == NoteKind.STEREO_CENTER) return true;
            if (k == NoteKind.STEREO_END) return true;

            return false;
        }
    
    
        public static bool IsTapSound(NoteKind k)
        {
            if (k == NoteKind.TAP) return true;
            if (k == NoteKind.ExTAP) return true;

            return false;
        }

        public static bool IsSlideSound(NoteKind k)
        {
            if (k == NoteKind.HOLD_START) return true;
            if (k == NoteKind.HOLD_END) return true;
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

        public static bool IsTap(NoteKind k)
        {
            if (k == NoteKind.TAP) return true;
            if (k == NoteKind.ExTAP) return true;

            return false;
        }

        public static bool IsFlick(NoteKind k)
        {
            if (k == NoteKind.FLICK) return true;
            if (k == NoteKind.FLICK_LEFT) return true;
            if (k == NoteKind.FLICK_RIGHT) return true;
            if (k == NoteKind.FLICK_UP) return true;
            if (k == NoteKind.FLICK_DOWN) return true;

            return false;
        }

        public static bool IsBitCrash(NoteKind k)
        {
            if (k == NoteKind.HOLD_END) return true;
            if (k == NoteKind.HOLD_CENTER) return true;

            return false;
        }
    }
}