using System;
using DRFV.Enums;

namespace DRFV.Enums
{
    public enum GameSide
    {
        LIGHT = 0,
        DARK = 1,
        COLORLESS = 2
    }
    public enum GameComboDisplay
    {
        NONE = 0,
        COMBO = 1,
        SCORE = 2,
        MSCORE = 3,
        ACCURACY = 4,
        LAGRANGE = 5
    }
    public enum GameSubJudgeDisplay
    {
        NONE = 0,
        FAST_SLOW = 1,
        MS = 2
    }
    public enum BarType
    {
        DEFAULT = 0,
        EASY = 1,
        HARD = 2,
        DANKAI = 3
    }
    public enum NoteKind
    {
        TAP = 1,
        ExTAP = 2,
        HOLD_START = 3,
        HOLD_END = 4,
        SLIDE_START = 5,
        SLIDE_CENTER = 6,
        SLIDE_END = 7,
        FAKE = 8,
        FLICK = 9,
        BOOM = 10,
        HOLD_CENTER = 11,
        FAKE_CENTER = 12,
        FLICK_LEFT = 13,
        FLICK_RIGHT = 14,
        FLICK_UP = 15,
        FLICK_DOWN = 16,
        BOOM_CENTER = 17,
        BOOM_END = 18,
        HPass_CENTER = 19,
        HPass_END = 20,
        LPass_CENTER = 21,
        LPass_END = 22,
        MOVER_CENTER = 23,
        MOVER_END = 24,
        STEREO_START = 25,
        STEREO_CENTER = 26,
        STEREO_END = 27,
        MOVERBOOM_CENTER = 28,
        MOVERBOOM_END = 29
    }
    public enum NoteSCType
    {
        SINGLE = 0,
        MULTI = 1
    }
    public enum NoteAppearMode
    {
        None = 0,
        Left = 1,
        Right = 2,
        High = 3,
        Jump = 4
    }

    public enum JudgeType
    {
        PERFECT_J = 0,
        PERFECT = 1,
        GOOD = 2,
        MISS = 3
    }
}

namespace DRFV.Game
{
    public static class ParseNoteAppearMode
    {
        public static NoteAppearMode ParseToMode(string str)
        {
            return str.ToLower() switch
            {
                "n" => NoteAppearMode.None,
                "l" => NoteAppearMode.Left,
                "r" => NoteAppearMode.Right,
                "h" => NoteAppearMode.High,
                "p" => NoteAppearMode.Jump,
                _ => NoteAppearMode.None
            };
        }

        public static string ParseToString(NoteAppearMode mode, bool isUpper = true)
        {
            string str = mode switch
            {
                NoteAppearMode.None => "n",
                NoteAppearMode.Left => "l",
                NoteAppearMode.Right => "r",
                NoteAppearMode.High => "h",
                NoteAppearMode.Jump => "p",
                _ => "n"
            };
            return isUpper ? str.ToUpper() : str;
        }
    }
}